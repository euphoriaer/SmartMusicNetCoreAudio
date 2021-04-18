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
    public static string iP = "127.0.0.1";//服务器地址

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

    private void Test()
    {
        NetManager.NetConfig("127.0.0.1", 8888);
        NetManager.Update();
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddMsgListener("MsgMoveTest", OnMsgMoveTest);
        FinishInit();
    }

    private void OnMsgOrder(MsgBase msgBase)
    {

        return;
    }

    private void OnMsgMoveTest(MsgBase msgBase)
    {
        
        //MsgMoveTest msg = msgBase as MsgMoveTest;
        //Debug.LogError("-------收到移动消息" + msg.id);
        //CtrlMove syncMove = ConnectManager.FindPlayer(msg.id).GetComponent<CtrlMove>();

        //syncMove.SyncMove(msg.v, msg.h);
    }

    private void OnConnectSucc(string err)
    {
        isConnect = true;//连接成功发送 Ping
        MsgInstance instance = new MsgInstance("手机客户端");
        NetManager.Send(instance);
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
        NetManager.NetConfig("127.0.0.1", 8888);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);//加入监听

        NetManager.iPconfig = iP;
        NetManager.ipPort = 8888;
        NetManager.Connect(FinishInit);
    }

    private void FinishInit()
    {
    
        NetManager.AddMsgListener("MsgOrder", OnMsgOrder);
        NetManager.AddMsgListener("MsgInstance", OnMsgInstance);
        NetManager.AddMsgListener("MsgTest", OnMsgOrder);
    }

    private void OnMsgInstance(MsgBase msgBase)
    {
        Debug.Log("服务器收到消息id，");
    }

    private void UpMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.UpMusic.ToString());
        NetManager.Send(msgOrder);
    }

    private void DownMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        MsgOrder msgOrder = new MsgOrder(SmartOrder.DownMusic.ToString());
        NetManager.Send(msgOrder);
    }

    private void PauseMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        MsgOrder msgOrder = new MsgOrder(SmartOrder.PauseMusic.ToString());
        NetManager.Send(msgOrder);
    }

    private void StopMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }
        MsgOrder msgOrder = new MsgOrder(SmartOrder.StopMusic.ToString());
        NetManager.Send(msgOrder);

    }

    private void PlayMusic()
    {
        if (!isConnect)
        {
            ShowState("未连接服务器");
        }


        MsgOrder msgOrder = new MsgOrder(SmartOrder.PlayMusic.ToString());
        NetManager.Send(msgOrder);
    }

    /// <summary>
    ///  发送消息
    /// </summary>
    /// <param name="msg"></param>
   

    // Update is called once per frame
    private void Update()
    {
        if (isConnect)
        {
            NetManager.Update();
        }
    }
}