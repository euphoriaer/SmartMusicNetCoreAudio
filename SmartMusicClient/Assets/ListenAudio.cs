using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wit.BaiduAip.Speech;

public class ListenAudio : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string appID = "";

    public string apiKey = "";              //��д�Լ���apiKey
    public string secretKey = "";         //��д�Լ���secretKey

    public Text DescriptionText;

    private AudioClip _clipRecord;
    private Asr _asr;
    public string currentDeviceName = string.Empty;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("�����˰�ť");
        this.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        DescriptionText.text = "����¼��...";

        _clipRecord = Microphone.Start(currentDeviceName, false, 30, 16000);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("�ɿ��˰�ť");
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        DescriptionText.text = "�ȴ�����...";
        Microphone.End(null);
        Debug.Log("[WitBaiduAip demo]end record");
        var data = Asr.ConvertAudioClipToPCM16(_clipRecord);
        StartCoroutine(_asr.Recognize(data, s =>
        {
            DescriptionText.text = s.result != null && s.result.Length > 0 ? s.result[0] : "δʶ������";
            SmartMusic.audioCmd = DescriptionText.text;
        }));
    }

    // Start is called before the first frame update
    private void Start()
    {
        _asr = new Asr(apiKey, secretKey);
        StartCoroutine(_asr.GetAccessToken());

        Debug.Log("��ȡ��˷�");
        //��ȡ��˷��豸���ж��Ƿ�����˷��豸
        if (Microphone.devices.Length > 0)
        {
            currentDeviceName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("û�л�ȡ��˷�");
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}