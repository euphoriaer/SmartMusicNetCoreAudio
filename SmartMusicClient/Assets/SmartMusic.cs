using System;
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
        ShowState("服务器已连接");
        iP = IPAddress1.text;
        NetManager.NetConfig(iP, 8888);
        NetManager.iPconfig = iP;
        NetManager.ipPort = 8888;
        NetManager.Connect(iP, 8888);
        FinishInit();
    }

    private void DownMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.DownMusic.ToString());
        NetManager.Send(msgOrder);
    }

    private void FinishInit()
    {
        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, (err) =>
        {
            OnConnectSucc(err);//todo 加入unity相关操作就会堵死。
        });//加入监听

        NetManager.AddMsgListener("MsgOrder", OnMsgOrder);
        NetManager.AddMsgListener("MsgInstance", OnMsgInstance);
        NetManager.AddMsgListener("MsgTest", OnMsgOrder);
        NetManager.AddMsgListener("MsgMusicMenu", OnMsgMusicMenu);
    }

    private void GetMusicList()
    {
        MsgOrder msg = new MsgOrder("MusicList");
        NetManager.Send(msg);
    }

    public void OnConnectSucc(string err)
    {
        isConnect = true;
        //NetManager.NetConfig("127.0.0.1", 8888);
        ////NetManager.Update();
        //FinishInit();
        //NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        //NetManager.AddMsgListener("MsgInstance", OnMsgInstance);
        ////NetManager.AddMsgListener("MsgMove", OnMsgMove);
        //NetManager.AddMsgListener("MsgMoveTest", OnMsgMoveTest);

        MsgInstance instance = new MsgInstance("手机客户端");
        NetManager.Send(instance);

        Debug.Log("发送id：手机客户端");
        //string m = "服务器连接成功";
        //todo ShowState(""); 委托会阻塞线程，收不到任何消息，且无任何报错，用lambda表达式可解决？不可。。。

        //Debug.Log("发送id：手机客户端3");
    }

    private void OnMsgInstance(MsgBase msgBase)
    {
        Debug.Log("服务器收到消息id，");
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
    public void ShowState(String message)
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
    }

    private void StopMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.StopMusic.ToString());
        NetManager.Send(msgOrder);
    }

    // Update is called once per frame
    private void Update()
    {
        if (isConnect)
        {
            NetManager.Update();
        }
    }

    private void UpMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.UpMusic.ToString());
        NetManager.Send(msgOrder);
    }
}