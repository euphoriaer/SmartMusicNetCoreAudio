using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SmartMusic : MonoBehaviour
{
    [Space]
    public static string iP = "127.0.0.1";

    public Button connect;
    public Button downMusic;
    public InputField IPAddress1;
    public Text musicList;
    public Button MusicListbtn;
    public Text MusicState;
    public Button pause;
    public Button play;
    public Transform root;
   
    public Button stop;
    public Button upMusic;
    //private static int port = 8888;
    private bool isConnect = false;
    //服务器地址

    private void ConnectServer()
    {
        NetManager.NetConfig(iP, 8888);

        NetManager.iPconfig = iP;
        NetManager.ipPort = 8888;
        NetManager.Connect(FinishInit);
    }

    private void DownMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.DownMusic.ToString());
        NetManager.Send(msgOrder);
    }

    private void FinishInit()
    {
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);//加入监听
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnClose);//加入监听
        NetManager.AddMsgListener("MsgOrder", OnMsgOrder);
        NetManager.AddMsgListener("MsgInstance", OnMsgInstance);
        NetManager.AddMsgListener("MsgTest", OnMsgOrder);
        NetManager.AddMsgListener("MsgMusicMenu", OnMsgMusicMenu);
    }

    private void OnClose(string err)
    {
        Debug.Log("服务器关闭");
    }

    private void GetMusicList()
    {
        MsgOrder msg = new MsgOrder("MusicList");
        NetManager.Send(msg);
    }

    private void OnConnectSucc(string err)
    {
        MsgInstance instance = new MsgInstance("手机客户端");
        NetManager.Send(instance);

        isConnect = true;//连接成功发送 Ping
        Debug.Log("发送id：手机客户端2");
        string m = "服务器连接成功";
        ShowState(m);

        Debug.Log("发送id：手机客户端3");
    }

    private void OnMsgInstance(MsgBase msgBase)
    {
        Debug.Log("服务器收到消息id，");
    }

    private void OnMsgMoveTest(MsgBase msgBase)
    {
        //MsgMoveTest msg = msgBase as MsgMoveTest;
        //Debug.LogError("-------收到移动消息" + msg.id);
        //CtrlMove syncMove = ConnectManager.FindPlayer(msg.id).GetComponent<CtrlMove>();

        //syncMove.SyncMove(msg.v, msg.h);
    }

    private void OnMsgMusicMenu(MsgBase msgBase)
    {
        Debug.Log("收到音乐目录");
        MsgMusicMenu msg = msgBase as MsgMusicMenu;
        StringBuilder musicList = new StringBuilder();
        foreach (var item in msg.musicNames.names)
        {
            musicList.AppendLine(item);
        }

        this.musicList.text = musicList.ToString();
    }

    private void OnMsgOrder(MsgBase msgBase)
    {
        return;
    }

    private void PauseMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.PauseMusic.ToString());
        NetManager.Send(msgOrder);
    }

    private void PlayMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.PlayMusic.ToString());
        NetManager.Send(msgOrder);
    }

    /// <summary>
    /// 音乐状态显示
    /// </summary>
    /// <param name="message"></param>
    private void ShowState(String message)
    {
        MusicState.text = message;
    }

    //服务器端口号
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
        musicList = root.Find("MusicList/Text").GetComponent<Text>();
        MusicListbtn = root.Find("MusicListBtn").GetComponent<Button>();
        musicList.text = "";

        play.onClick.AddListener(PlayMusic);
        stop.onClick.AddListener(StopMusic);
        pause.onClick.AddListener(PauseMusic);
        downMusic.onClick.AddListener(DownMusic);
        upMusic.onClick.AddListener(UpMusic);
        connect.onClick.AddListener(ConnectServer);
        MusicListbtn.onClick.AddListener(GetMusicList);

        ConnectServer();
    }

    private void StopMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.StopMusic.ToString());
        NetManager.Send(msgOrder);
    }

    // Update is called once per frame
    private void Update()
    {
        
         
            NetManager.Update();
        
    }

    private void UpMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.UpMusic.ToString());
        NetManager.Send(msgOrder);
    }

    /// <summary>
    ///  发送消息
    /// </summary>
    /// <param name="msg"></param>
}