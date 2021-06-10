using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

//RequireComponent�������������Ҫ���ڲ����Լ�¼�Ƶ�����,����Ҫ����ɾ��,ͬʱע��ɾ��ʹ������Ĵ���
[RequireComponent(typeof(AudioListener)), RequireComponent(typeof(AudioSource))]
public class AudioControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    //�ٶ�����ʶ�����key
    //string appId = "";
    public string appID = "";
    public string apiKey = "";              //��д�Լ���apiKey
    public string secretKey = "";         //��д�Լ���secretKey
    public Text logText;
    private string logString="";

    //��¼accesstoken����
    private string accessToken = string.Empty;

    //����ʶ��Ľ��
    public string asrResult = string.Empty;

    //����Ƿ�����˷�
    public bool isHaveMic = false;

    //��ǰ¼���豸����
    public string currentDeviceName = string.Empty;

    //¼��Ƶ��,����¼������(8000,16000)
    public int recordFrequency = 15000;

    //�ϴΰ���ʱ���
    private double lastPressTimestamp = 0;

    //��ʾ¼�������ʱ��
    private int recordMaxLength = 100;

    //ʵ��¼������(����unity��¼������ָ������,����ʶ���ϴ�ʱ����ϴ��������Ч�ֽ�)
    //ͨ�����ֶ�,��ȡ��Ч¼������,�ϴ�ʱ����е���Ч���ֽ����ݼ���
    private int trueLength = 100;

    //�洢¼����Ƭ��
    [HideInInspector]
     public AudioClip saveAudioClip;

    //��ǰ��ť�µ��ı�
    public Text textBtn;

    //��ʾ������ı�
    public Text textResult;

    //��Դ
    public AudioSource audioSource;

    void Start()
    {
        Debug.Log("��ȡ��˷�");
        //��ȡ��˷��豸���ж��Ƿ�����˷��豸
        if (Microphone.devices.Length > 0)
        {
            isHaveMic = true;
            currentDeviceName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("û�л�ȡ��˷�");
        }

        //��ȡ������
        textBtn = this.transform.GetChild(0).GetComponent<Text>();
        audioSource = this.GetComponent<AudioSource>();
        textResult = this.transform.parent.GetChild(1).GetComponent<Text>();
     

    }
    private StringBuilder buildString;
    private void LogBox(string message)
    {
        buildString.AppendLine(message);
        logText.GetComponent<Text>().text = buildString.ToString();
    }

    /// <summary>
    /// ��ʼ¼��
    /// </summary>
    /// <param name="isLoop"></param>
    /// <param name="lengthSec"></param>
    /// <param name="frequency"></param>
    /// <returns></returns>
    public bool StartRecording(bool isLoop = false) //8000,16000
    {
        Debug.Log("��ʼ¼��");
        if (isHaveMic == false || Microphone.IsRecording(currentDeviceName))
        {
            return false;
        }

        //��ʼ¼��
        /*
         * public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency);
         * deviceName   ¼���豸����.
         * loop         ����ﵽ����,�Ƿ������¼
         * lengthSec    ָ��¼���ĳ���.
         * frequency    ��Ƶ������   
         */

        lastPressTimestamp = GetTimestampOfNowWithMillisecond();

        saveAudioClip = Microphone.Start(currentDeviceName, isLoop, recordMaxLength, recordFrequency);

        return true;
    }

    /// <summary>
    /// ¼������,����ʵ�ʵ�¼��ʱ��
    /// </summary>
    /// <returns></returns>
    public int EndRecording()
    {
        if (isHaveMic == false || !Microphone.IsRecording(currentDeviceName))
        {
            return 0;
        }

        //����¼��
        Microphone.End(currentDeviceName);

        //����ȡ��,������©¼��ĩβ
        return Mathf.CeilToInt((float)(GetTimestampOfNowWithMillisecond() - lastPressTimestamp) / 1000f);
    }

    /// <summary>
    /// ��ȡ���뼶���ʱ���,���ڼ��㰴��¼��ʱ��
    /// </summary>
    /// <returns></returns>
    public double GetTimestampOfNowWithMillisecond()
    {
        return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
    }

    /// <summary>
    /// ����¼����ť
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        textBtn.text = "�ɿ�ʶ��";
        StartRecording();
    }

    /// <summary>
    /// �ſ�¼����ť
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        textBtn.text = "��ס˵��";
        trueLength = EndRecording();
        if (trueLength > 1)
        {
            Debug.Log("��ʼ¼��");
            audioSource.PlayOneShot(saveAudioClip);
            StartCoroutine(_StartBaiduYuYin());
        }
        else
        {
            textResult.text = "¼��ʱ������";
        }
    }

    /// <summary>
    /// ��ȡaccessToken��������
    /// </summary>
    /// <returns></returns>
    IEnumerator _GetAccessToken()
    {
        var uri =
            string.Format(
                "https://openapi.baidu.com/oauth/2.0/token?grant_type=client_credentials&client_id={0}&client_secret={1}",
                apiKey, secretKey);
        UnityWebRequest unityWebRequest = UnityWebRequest.Get(uri);
        yield return unityWebRequest.SendWebRequest();

        if (unityWebRequest.isDone)
        {
            //������Կ�����Json,���˱Ƚ�������������ƥ���accessToken
            Match match = Regex.Match(unityWebRequest.downloadHandler.text, @"access_token.:.(.*?).,");
            if (match.Success)
            {
                //��ʾ����ƥ�䵽��accessToken
                LogBox("�õ���token");
                accessToken = match.Groups[1].ToString();
            }
            else
            {
                textResult.text = "��֤����,��ȡAccessTokenʧ��!!!";
            }
        }
    }

   

   
    public void AsrData(byte[] data)
    {
        // ����APPID/AK/SK
        var APP_ID = "��� App ID";
        var API_KEY = "��� Api Key";
        var SECRET_KEY = "��� Secret Key";
        var client = new Baidu.Aip.Speech.Asr(appID,apiKey, secretKey);
        client.Timeout = 60000;  // �޸ĳ�ʱʱ��
        //var data = File.ReadAllBytes(pcmPath);

        // ��ѡ����
        var options = new Dictionary<string, object>
     {
        {"dev_pid", 1537}
     };
        client.Timeout = 120000; // �������ϳ����������ø���ĳ�ʱʱ��. ms
        var result = client.Recognize(data, "pcm", 16000, options);
        LogBox("ʶ��ɹ�:" + result);
        Console.Write(result);
    }
    /// <summary>
    /// ��������ʶ������
    /// </summary>
    /// <returns></returns>
    IEnumerator _StartBaiduYuYin()
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            yield return _GetAccessToken();
        }

        asrResult = string.Empty;
        LogBox("����¼������");
        //����ǰ¼������ΪPCM16
        float[] samples = new float[recordFrequency * trueLength * saveAudioClip.channels];
        saveAudioClip.GetData(samples, 0);
        var samplesShort = new short[samples.Length];
        for (var index = 0; index < samples.Length; index++)
        {
            samplesShort[index] = (short)(samples[index] * short.MaxValue);
        }
        byte[] datas = new byte[samplesShort.Length * 2];
        AsrData(datas);
       
        //Buffer.BlockCopy(samplesShort, 0, datas, 0, datas.Length);

        //string url = string.Format("{0}?cuid={1}&token={2}", "https://vop.baidu.com/server_api", SystemInfo.deviceUniqueIdentifier, accessToken);

        //WWWForm wwwForm = new WWWForm();
        //wwwForm.AddBinaryData("audio", datas);

        //UnityWebRequest unityWebRequest = UnityWebRequest.Post(url, wwwForm);

        //unityWebRequest.SetRequestHeader("Content-Type", "audio/pcm;rate=" + recordFrequency);

        //yield return unityWebRequest.SendWebRequest();

        //if (string.IsNullOrEmpty(unityWebRequest.error))
        //{
        //    asrResult = unityWebRequest.downloadHandler.text;
        //    if (Regex.IsMatch(asrResult, @"err_msg.:.success"))
        //    {
        //        Match match = Regex.Match(asrResult, "result.:..(.*?)..]");
        //        if (match.Success)
        //        {
        //            asrResult = match.Groups[1].ToString();
        //        }
        //    }
        //    else
        //    {
        //        asrResult = "ʶ����Ϊ��";
        //    }
        //    textResult.text = asrResult;
        //}
    }
}