using System;
using System.Collections.Generic;
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
    public GameObject MusicList;
    public Button MusicListbtn;
    public GameObject musicListConnect;
    public Text MusicState;
    public Button pause;
    public GameObject pictureMusic;
    public Button play;
    public Transform root;
    [Header("声音")]
    public Slider Volume;
    [Header("旋转速度")]
    public float speed = 20;
    public Button stop;
    public Button upMusic;
    public ListenAudio audioCmdcs;

    public static string audioCmd = "";
    //private static int port = 8888;
    private bool isConnect = false;

    //服务器地址

    private bool isRotate = false;

    private List<GameObject> lineMusic = new List<GameObject>();

    public void AddNewLineMusic(string content)
    {
        //GameObject con = new GameObject(content, typeof(Text));//创建新的
        GameObject con = Instantiate(musicListConnect, MusicList.transform, false);//实例化对象,设置父物体且不缩放
        con.name = content;
        Debug.Log("实例化对象" + con.name);

        Text cont = con.GetComponent<Text>();
        cont.text = content;//设置内容

        lineMusic.Add(con);//加入到容器中
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

    /// <summary>
    /// 音乐状态显示
    /// </summary>
    /// <param name="message"></param>
    public void ShowState(String message)
    {
        MusicState.text = message;
    }

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

    public void DownMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.DownMusic.ToString());
        NetManager.Send(msgOrder);
        isRotate = true;
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

    private void OnMsgInstance(MsgBase msgBase)
    {
        Debug.Log("服务器收到消息id，");
    }

    private void OnMsgMusicMenu(MsgBase msgBase)
    {
        Debug.Log("收到音乐目录");
        MsgMusicMenu msg = msgBase as MsgMusicMenu;
        StringBuilder musicList = new StringBuilder();

        if (lineMusic.Count != 0)//=0说明没有获取目录
        {
            //
            Debug.Log("将未播放的置黑");
            foreach (var item in lineMusic)
            {
                //设置颜色,将未播放的音乐置黑
                item.GetComponent<Text>().color = Color.black;
            }
        }
        else
        {
            foreach (var item in msg.musicNames.names)
            {
                AddNewLineMusic(item);
            }
        }

        Debug.Log("当前播放的音乐为：" + msg.currMusicNnmber + lineMusic[msg.currMusicNnmber].name);

        //设置颜色
        Text currText = lineMusic[msg.currMusicNnmber].GetComponent<Text>();
        currText.color = Color.red;
    }

    private void OnMsgOrder(MsgBase msgBase)
    {
        return;
    }

    public void PauseMusic()
    {
        isRotate = false;
        MsgOrder msgOrder = new MsgOrder(SmartOrder.PauseMusic.ToString());
        NetManager.Send(msgOrder);
    }

    public void PlayMusic()
    {
        isRotate = true;
        MsgOrder msgOrder = new MsgOrder(SmartOrder.PlayMusic.ToString());
        NetManager.Send(msgOrder);
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
        audioCmdcs= root.Find("PictureMusic").GetComponent<ListenAudio>();
        IPAddress1.text = "121.4.107.99";
        MusicList = root.Find("MusicList").GetComponent<Transform>().gameObject;

        musicListConnect = root.Find("MusicList/MusicListConrent").GetComponent<Transform>().gameObject;

        MusicListbtn = root.Find("MusicListBtn").GetComponent<Button>();
        Volume = root.Find("Volume").GetComponent<Slider>();
        pictureMusic = root.Find("PictureMusic").GetComponent<Transform>().gameObject;
        play.onClick.AddListener(PlayMusic);
        stop.onClick.AddListener(StopMusic);
        pause.onClick.AddListener(PauseMusic);
        downMusic.onClick.AddListener(DownMusic);
        upMusic.onClick.AddListener(UpMusic);
        connect.onClick.AddListener(ConnectServer);
        MusicListbtn.onClick.AddListener(GetMusicList);
    }

    public void StopMusic()
    {
        isRotate = false;
        MsgOrder msgOrder = new MsgOrder(SmartOrder.StopMusic.ToString());
        NetManager.Send(msgOrder);
    }
    private float currentVolume = 0;
    private float time=0;
    private const float timeall= 0.1f;
    // Update is called once per frame
    private void Update()
    {
        time += Time.deltaTime;
        if (isConnect)
        {
            NetManager.Update();
        }
        if (isRotate)
        {
            pictureMusic.transform.Rotate(Vector3.forward, speed);
        }
        //如果语音命令不为空就执行语音命令
        if (audioCmd!="")
        {
          
            Debug.LogError(audioCmd);
            

            if (audioCmd.Contains("播放"))
            {
                

                if (lineMusic.Count!=0)
                {
                    foreach (var item in lineMusic)
                    {
                        //检查音乐列表，是否有cmd命令播放的音乐
                        Text cont = item.GetComponent<Text>();
                        if (audioCmd.Contains(cont.text))
                        {
                            
                            Debug.LogError("播放指定音乐" + cont.text);
                            //tudo 播放指定音乐
                            break;
                        }
                    }
                }
                PlayMusic();

            }
            if (audioCmd.Contains("暂停"))
            {
                PauseMusic();
            }
            if (audioCmd.Contains("停止"))
            {
                StopMusic();
            }
            if (audioCmd.Contains("上一曲"))
            {
                UpMusic();
            }
            if (audioCmd.Contains("下一曲"))
            {
                DownMusic();
            }
            audioCmd = "";
        }

        //如果 音量发生变化且计时结束 就发送消息
        if (Mathf.Abs(currentVolume-Volume.value)>=0.1&&time>=timeall)//todo 小技巧 不要用float进行 == 判定，误差
        {
            currentVolume = Volume.value;//
            int curVolume = (int)(Volume.value * 100);
            Debug.Log("现在的音量为：" + curVolume);
            MsgAudio msg = new MsgAudio();
            msg.audio = curVolume;
            NetManager.Send(msg);
            time = 0;

            //发送音量协议
        }
    }

    public void UpMusic()
    {
        MsgOrder msgOrder = new MsgOrder(SmartOrder.UpMusic.ToString());
        NetManager.Send(msgOrder);
        isRotate = true;
    }
}