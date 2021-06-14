using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wit.BaiduAip.Speech;

public class ListenAudio : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string appID = "";

    public string apiKey = "";              //填写自己的apiKey
    public string secretKey = "";         //填写自己的secretKey

    public Text DescriptionText;

    private AudioClip _clipRecord;
    private Asr _asr;
    public string currentDeviceName = string.Empty;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("按下了按钮");
        this.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        DescriptionText.text = "正在录音...";

        _clipRecord = Microphone.Start(currentDeviceName, false, 30, 16000);
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("松开了按钮");
        this.transform.localScale = new Vector3(1f, 1f, 1f);
        DescriptionText.text = "等待返回...";
        Microphone.End(null);
        Debug.Log("[WitBaiduAip demo]end record");
        var data = Asr.ConvertAudioClipToPCM16(_clipRecord);
        StartCoroutine(_asr.Recognize(data, s =>
        {
            DescriptionText.text = s.result != null && s.result.Length > 0 ? s.result[0] : "未识别到声音";
            SmartMusic.audioCmd = DescriptionText.text;
        }));
    }

    // Start is called before the first frame update
    private void Start()
    {
        _asr = new Asr(apiKey, secretKey);
        StartCoroutine(_asr.GetAccessToken());

        Debug.Log("获取麦克风");
        //获取麦克风设备，判断是否有麦克风设备
        if (Microphone.devices.Length > 0)
        {
            currentDeviceName = Microphone.devices[0];
        }
        else
        {
            Debug.LogError("没有获取麦克风");
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }
}