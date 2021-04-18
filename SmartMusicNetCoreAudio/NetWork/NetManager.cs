using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

/// <summary>
/// 使用方法：设置ip 设置port 添加update驱动，进行连接
/// </summary>
public static class NetManager
{
    /// <summary>
    /// ip
    /// </summary>
    public static String iPconfig;
    /// <summary>
    /// 端口
    /// </summary>
    public static int ipPort;

    ///定义套接字
    private static Socket socket;

    ///接收缓冲区
    private static ByteArray readBuff;

    ///写入队列
    private static Queue<ByteArray> writeQueue;

    /// <summary>
    /// 事件委托类型
    /// </summary>
    /// <param name="err"></param>
    public delegate void EventListener(string err);

    /// <summary>
    /// 事件监听列表
    /// </summary>
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    /// <summary>
    /// 消息委托类型
    /// </summary>
    /// <param name="msgBase"></param>
    public delegate void MsgListener(MsgBase msgBase);

    /// <summary>
    /// 消息监听列表
    /// </summary>
    public static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    private static bool isConnecting = false;

    private static bool isClosing = false;

    /// <summary>
    /// 消息列表
    /// </summary>
    private static List<MsgBase> msgList = new List<MsgBase>();

    /// <summary>
    /// 消息列表的长度
    /// </summary>
    private static int msgCount = 0;

  

    /// <summary>
    /// 每一次Update处理的消息量
    /// </summary>
    private static readonly int MAX_MESSAGE_FIRE = 10;

    /// <summary>
    /// 是否启用心跳
    /// </summary>
    public static bool isUsePing = true;

    /// <summary>
    /// 心跳间隔时间，默认30秒
    /// </summary>
    public static DateTime pingInterval=new DateTime(30);

    /// <summary>
    /// 上一次发送Ping的时间
    /// </summary>
    private static DateTime lastPingTime ;

    /// <summary>
    /// 上一次收到Pong的时间
    /// </summary>
    private static DateTime lastPongTime;

    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }

    /// <summary>
    /// 删除事件监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
        }

        if (eventListeners[netEvent] == null)
        {
            eventListeners.Remove(netEvent);
        }
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    /// <param name="netEvent">事件类型</param>
    /// <param name="err">要传给回调方法的字符串</param>
    private static void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    /// <summary>
    /// 添加消息监听,将消息添加到字典的列表里
    /// </summary>
    /// <param name="msgName">消息名</param>
    /// <param name="listener">要执行的函数</param>
    public static void AddMsgListener(string msgName, MsgListener listener)
    {
        //添加
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        else
        {
            msgListeners[msgName] = listener;
            //新增
        }
    }

    /// <summary>
    /// 删除消息监听
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="listener"></param>
    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
        }
        //删除
        if (msgListeners[msgName] == null)
        {
            msgListeners.Remove(msgName);
        }
    }

    /// <summary>
    /// 分发消息
    /// </summary>
    /// <param name="msgName"></param>
    /// <param name="msgBase"></param>
    private static void FireMsg(string msgName, MsgBase msgBase)
    {
       
        if (msgListeners.ContainsKey(msgName))
        {
           
            msgListeners[msgName](msgBase); //error 理解一下字典后面加（）的传参， 因为字典的 value是委托类型，此处将参数传给此委托。委托执行时，才能根据传进来的参数进行处理
            
        }
    }

    public static void Connect(Action action)
    {
        //状态判断
        if (socket != null && socket.Connected)
        {
            Console.WriteLine("Connect fail,already connected!");
            return;
        }

        if (isConnecting)
        {
            Console.WriteLine("Connect fail,is connecting");
            return;
        }

        //初始化成员
        InitState(action);

        //参数设置
        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect(iPconfig, ipPort, ConnectCallback, socket);
    }

    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState; //可以使用 as 进行强制类型转换
            socket.EndConnect(ar);
            Console.WriteLine("Socket Connect Success");
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = true;
            //开始接收
            //socket.Receive(readBuff.bytes, readBuff.writeIdx, readBuff.remain,0);
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket Connect fail" + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }

    /// <summary>
    /// Receive回调
    /// </summary>
    /// <param name="ar"></param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
       
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //获取接收数据长度
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                Close();
                return;
            }
            readBuff.writeIdx += count;
            //处理二进制消息
            OnReceiveData();
            //继续接收数据
            if (readBuff.remain > 8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket); //回调自己，递归
        }
        catch (SocketException ex)
        {
            //PanelManager.Open<TipPanel>("服务器关闭，请重新连接");
            //PanelManager.Open<LoginPanel>();
            //Console.WriteLine("Socket Receive fail:" + ex.ToString());
        }
    }

    /// <summary>
    /// 数据处理
    /// </summary>
    public static void OnReceiveData()
    { 
        //消息长度
        if (readBuff.length <= 2)
        {
            return;
        }
        //获取消息体长度
        int readIdx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readIdx + 1] << 8) | bytes[readIdx]);
        if (readBuff.length < bodyLength)
        {
            return;
        }
        readBuff.readIdx += 2;

        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount); //TODO out的使用，必须在形参中添加 out int count 书里面没加，未知原因
        if (protoName == "")
        {
            Console.WriteLine("OnReceveData MshBase.DecodeName fail：协议名为空");
            return;
        }

        readBuff.readIdx += nameCount;
        //解析协议体

        int bodyCount = bodyLength - nameCount;

        MsgBase msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        //添加到消息队列
        lock (msgList)
        {
            msgList.Add(msgBase);
            
        }
        msgCount++;
        //继续读取消息
        if (readBuff.length > 2)
        {
            OnReceiveData(); //递归
        }
    }

    /// <summary>
    /// NetWork Update 网络事件驱动，必须添加（否则监听不会生效）
    /// </summary>
    public static void Update()
    {
        MsgUpdate();
        if (socket == null || !socket.Connected)
        {
            //NetManager.Connect(FinishInit); //ip地址 127.0.0.1本地机
            //当连接断开，自动尝试连接 且停止发送ping
            return;
        }
        //PingUpdate();
    }

   

    /// <summary>
    /// 设置要连接的服务器的ip地址和端口
    /// </summary>
    /// <param name="iP">IP地址</param>
    /// <param name="port">端口</param>
    public static void NetConfig(string iP, int port)
    {
        iPconfig = iP;
        ipPort = port;
    }

    /// <summary>
    /// 更新消息
    /// </summary>
    public static void MsgUpdate()
    {
      
        //初步判断，提升效率
        if (msgCount == 0)
        {
            return;
        }
        //重复处理消息
        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
         
            //获取第一条消息
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                    FireMsg(msgBase.protoName, msgBase);
                }
            }

            //error 这里会出现无法分发消息的错误，分发消息 消息会空 ，在前面给予消息完毕后 直接分发不再 锁的外面分发消息，为什么会出错，目前不知道，猜测和锁有关
            //if (msgBase != null)
            //{
            //    Debug.Log("分发消息");
            //    FireMsg(msgBase.protoName, msgBase);
            //}
            //else
            //{
            //    Debug.Log("消息为空");
            //    break;
            //}
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private static void InitState(Action action)
    {
        Console.WriteLine("初始化");
        //定义socke
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //接收缓冲区
        readBuff = new ByteArray();
        //写入队列
        writeQueue = new Queue<ByteArray>();
        //是否正在接收
        isConnecting = false;

        //是否正在关闭
        isClosing = false;

        //消息列表
        msgList = new List<MsgBase>();

        //消息列表长度
        msgCount = 0;

        //上一次发送ping的时间，重置时间
        lastPingTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

        //上一次收到Pong的时间，重置
        lastPongTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();

        //监听Pong协议
        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
            Console.WriteLine("MsgPong监听添加");
            action.Invoke();
        }
    }

    private static void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
        Console.WriteLine("收到Pong----------"); //error 收不到Pong？
    }

    /// <summary>
    /// 发送Ping协议
    /// </summary>
    private static void PingUpdate()
    {
        if (!isUsePing)
        {
            return;
        }

        //发送Ping
        if (SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Second - lastPingTime.Second > pingInterval.Second)
        {
            MsgPing msgPing = new MsgPing();
            Console.WriteLine("发送：ping");
            Send(msgPing);
            lastPingTime = SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc();
        }
        //检测Pong时间
        if (SystemClock.Instance.GetCurrentInstant().ToDateTimeUtc().Second - lastPingTime.Second > pingInterval.Second * 4)
        {
            Console.WriteLine("关闭连接");
            Close();
        }
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    public static void Close()
    {
        //状态判断
        if (socket == null || !socket.Connected)
        {
            return;
        }

        //还有数据在发送
        if (writeQueue.Count > 0)
        {
            isClosing = true;
        }
        else
        {
            //没有数据在发送
            socket.Close();
            isConnecting = false;
            FireEvent(NetEvent.Close, "");
        }
    }

    /// <summary>
    /// 发送数据
    /// </summary>
    /// <param name="msg"></param>
    public static void Send(MsgBase msg)
    {
        if (socket == null || !socket.Connected)
        {
            //PanelManager.CloseAll();//todo  断开了socket的链接，关闭所有面板，打开登录面板准备重新连接
            //PanelManager.Open<LoginPanel>();
            //PanelManager.Open<TipPanel>("断开连接，请重新登录");
            Console.WriteLine("socket 没有就绪");
            return;
        }

        if (!isConnecting)
        {
            Console.WriteLine("服务器没有连接");
            return;
        }

        if (isClosing)
        {
            Console.WriteLine("连接关闭");
            return;
        }
        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];

        //组装长度
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);

        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);

        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        //写入队列
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;

        //WriteQueue的长度
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        //send 发送
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    private static void SendCallback(IAsyncResult ar)
    {
        //获取state,EndSend 的处理结果
        Socket socket = (Socket)ar.AsyncState;
        //状态判断
        if (socket == null || !socket.Connected)
        {
            return;
        }

        //EndSend

        int count = socket.EndSend(ar);
        //获取写入队列的第一条数据
        ByteArray ba;
        lock (writeQueue)
        {
            ba = writeQueue.First();
        }

        //完整发送
        try
        {
            ba.readIdx += count;
            if (ba.length == 0)
            {
                lock (writeQueue)
                {
                    writeQueue.Dequeue();
                    ba = writeQueue.First();
                }
            }
            //继续发送
            if (ba.length != 0)
            {
                socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket); //sendCallback调用自己，递归
            }
            else if (isClosing)
            {
                socket.Close();
                //正在关闭
            }
        }
        catch (Exception)
        {

         
        }
      
    }

    public enum NetEvent //TODO，理解一下：enum可以放在class里，也可以放在class外面，区别就是不同的访问级别
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }
}