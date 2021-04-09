using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class SmartMusic : MonoBehaviour
{
    public Button play;
    public Button stop;
    public Button pause;
    public Button downMusic;
    public Button upMusic;
    public Button connect;
    public Text MusicState;
    public InputField IPAddress1;

    private bool isConnect = false;

    public Transform root;

    [Space]
    public static string iP;//服务器地址

    private static int port = 9910;//服务器端口号

    public Socket socketSend;

    // Start is called before the first frame update
    private void Start()
    {
        root = GameObject.Find("Canvas").GetComponent<Transform>();
        play = root.Find("Play").GetComponent<Button>();
        stop = root.Find("Stop").GetComponent<Button>();
        pause = root.Find("Pause").GetComponent<Button>();
        downMusic = root.Find("DownMusic").GetComponent<Button>();
        upMusic = root.Find("UpMusic").GetComponent<Button>();
        connect = root.Find("Connect").GetComponent<Button>();
        MusicState = root.Find("MusicState").GetComponent<Text>();
        IPAddress1 = root.Find("IPAddress").GetComponent<InputField>();

        play.onClick.AddListener(PlayMusic);
        stop.onClick.AddListener(StopMusic);
        pause.onClick.AddListener(PauseMusic);
        downMusic.onClick.AddListener(DownMusic);
        upMusic.onClick.AddListener(UpMusic);
        connect.onClick.AddListener(ConnectServer);
        socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    /// <summary>
    /// 音乐状态显示
    /// </summary>
    /// <param name="message"></param>
    private void ShowState(String message)
    {
        MusicState.text = message;
    }

    private void ConnectServer()
    {
        if (socketSend == null || socketSend.Connected)
        {
            ShowState("服务器已经连接");
            return;
        }
        if (iP == "" || iP == null)
        {
            iP = IPAddress.Loopback.ToString();//如果ip为空 则监听本机
        }
        else
        {
            iP = IPAddress1.text;
        }
        IPAddress _iP = IPAddress.Parse(iP);
        IPEndPoint _point = new IPEndPoint(_iP, port);
        socketSend.Connect(_point);
        ShowState("服务器连接");
        isConnect = true;

        //todo 开启线程不停接收消息，客户端暂时可以只发送，不接受
    }

    private void UpMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        SendMessage("上一曲");
    }

    private void DownMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        SendMessage("下一曲");
    }

    private void PauseMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        SendMessage("暂停");
    }

    private void StopMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        SendMessage("停止");
    }

    private void PlayMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        SendMessage("播放");
    }

    /// <summary>
    ///  发送消息
    /// </summary>
    /// <param name="msg"></param>
    public void SendMessage(string msg)
    {
        byte[] msgs = System.Text.Encoding.UTF8.GetBytes(msg);//转成字节数组
                                                              //发送消息
        socketSend.Send(msgs);
    }

    // Update is called once per frame
    private void Update()
    {

    }
}