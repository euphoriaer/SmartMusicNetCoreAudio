using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;

internal class NetManager
{
    public static Socket listenfd;

    //客户端Socket 及状态信息
    public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

    //Select 的检查列表
    private static List<Socket> checkRead = new List<Socket>();

    /// <summary>
    ///ping间隔时间，默认30秒
    /// </summary>
    public static long pingInterval = 30;

    /// <summary>
    /// 开启服务端监听
    /// </summary>
    /// <param name="listenPort">监听端口</param>
    public static void StarLoop(int listenPort)
    {
        //socket
        listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //Bind
        IPAddress IPAdr = IPAddress.Parse("0.0.0.0");
        IPEndPoint ipEp = new IPEndPoint(IPAdr, listenPort);
        listenfd.Bind(ipEp);
        //Listen
        listenfd.Listen(0);
        Console.WriteLine("[服务器]启动成功");
        //Loop
        while (true)
        {
            ResetCheckRead();//重置checkRead
            Socket.Select(checkRead, null, null, 1000);
            //检查可读对象
            for (int i = checkRead.Count - 1; i >= 0; i--)
            {
                Socket s = checkRead[i];
                if (s == listenfd)
                {
                    ReadListenfd(s);
                }
                else
                {
                    ReadClientfd(s);
                }
            }
        }
    }

    /// <summary>
    /// 读取listenfd
    /// </summary>
    /// <param name="listenfd"></param>
    public static void ReadListenfd(Socket listenfd)
    {
        try
        {
            Socket clientfd = listenfd.Accept();

            Console.WriteLine("Accept：" + clientfd.RemoteEndPoint.ToString());

            ClientState state = new ClientState();

            state.socket = clientfd;
            clients.Add(clientfd, state);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Accept fail：" + ex.ToString());
        }
    }

    /// <summary>
    /// 填充checkRead列表
    /// </summary>
    public static void ResetCheckRead()
    {
        checkRead.Clear();
        checkRead.Add(listenfd);
        foreach (var s in clients.Values)
        {
            checkRead.Add(s.socket);
        }
    }

    /// <summary>
    /// 读取Clientfd
    /// </summary>
    /// <param name="clientfd"></param>
    public static void ReadClientfd(Socket clientfd)
    {
        ClientState state = clients[clientfd];
        ByteArray readBuff = state.readBuff;
        //接收
        int count = 0;
        //缓冲区不够，清除，若依旧不够，返回
        //缓冲区长度1024，单条协议超过缓冲区长度会发生错误，根据需要调整长度
        if (readBuff.remain <= 0)
        {
            OnReceiveData(state);
            readBuff.MoveBytes();
        }
        if (readBuff.remain <= 0)
        {
            Console.WriteLine("Receive fail,maybe msg length>buff capacity:缓冲区长度1024，单条协议超过缓冲区长度会发生错误，根据需要调整长度");
            Close(state);
            return;
        }

        try
        {
            count = clientfd.Receive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0);// error 服务端使用同步 接收，客户端是异步接收，可尝试异步服务端
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Receive SocketException" + ex.ToString());
            Close(state);
            return;
        }
        //客户端关闭
        if (count <= 0)
        {
            Console.WriteLine("Socket Close" + clientfd.RemoteEndPoint.ToString());
            Close(state);
            return;
        }

        //消息处理
        readBuff.writeIdx += count;
        //处理二进制消息
        OnReceiveData(state);
        //移动缓冲区
        readBuff.CheckAndMoveBytes();
    }

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="state"></param>
    public static void Close(ClientState state)
    {
        //error 下线从列表删除对应玩家
        //if (state.player.id!=null)
        //{
        //ConnectManager.RemovePlayer(state.player.id);
        if (state == null)
        {
            return;
        }
        ConnectManager.RemoveMsg(state.id);

        //}
        //事件分发
        MethodInfo mei = typeof(EventHandler).GetMethod("OnDisconnect");//TODO 理解一下Methodinfo 和后面的 invoke 答：利用反射进行函数调用
        object[] ob = { state };
        mei.Invoke(null, ob);
        //关闭
        // ConnectManager.RemovePlayer(state);
        state.socket.Close();
        clients.Remove(state.socket);
    }

    /// <summary>
    /// 数据处理
    /// </summary>
    /// <param name="state"></param>
    public static void OnReceiveData(ClientState state)
    {
        ByteArray readBuff = state.readBuff;
        //消息长度
        if (readBuff.length <= 2)
        {
            return;
        }
        Int16 bodyLength = readBuff.ReadInt16();
        //消息体
        if (readBuff.length < bodyLength)
            return;
        //解析协议名
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);

        if (protoName == "")
        {
            Console.WriteLine("OnReceiveData MsgBase.DecodeName fail:协议名为空");
            Close(state);
        }
        readBuff.readIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = new MsgBase("null");
        if (bodyCount >= 0)
        {
            msgBase = MsgBase.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        }
        else
        {
            msgBase = null;
        }

        readBuff.readIdx += bodyCount;

        readBuff.CheckAndMoveBytes();
        if (protoName == null)
        {
            return;
        }
        //分发消息
        MethodInfo mi = typeof(MsgHandler).GetMethod(protoName);//Todo 消息分发（根据不同名字消息分发，使用反射？那能否用委托？） 此处用的GetMethod 是反射？得到一个类的所有方法名，并根据string赋予 MethodInfo（系统方法）
        //Todo RoomMsgHandle无法分发到消息 7.3.7服务端处理协议     答：RoomMsgHandle 里面的类改名为 MsgHandle（使用继承不行）
        object[] o = { state, msgBase };
        // error Console.WriteLine("[服务器]Receive：" + protoName);
        if (mi != null)
        {
            mi.Invoke(null, o); //error 服务端结合反射，进行了函数调用且传参，客户端根据委托，利用字典进行调用（执行委托）且传参
        }
        else
        {
            Console.WriteLine("OnReceiveData invoke fail:mi=null " + protoName);
        }
        //继续读取消息
        if (readBuff.length > 2)
        {
            OnReceiveData(state);
        }
    }

    /// <summary>
    /// 定时器
    /// </summary>
    public static void Timer()
    {
        //消息分发
        MethodInfo mei = typeof(EventHandler).GetMethod("OnTimer");
        object[] ob = { };
        mei.Invoke(null, ob);
    }

    /// <summary>
    /// 发送
    /// </summary>
    /// <param name="cs">客户端连接状态</param>
    /// <param name="msg"></param>
    public static void Send(ClientState cs, MsgBase msg)
    {//状态判断
        if (cs == null)
        {
            return;
        }
        if (!cs.socket.Connected)
        {
            return;
        }
        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);

        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];

        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);

        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);

        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        //TODO,简化代码，不设置回调，可以参考客户端,设置回调

        try
        {
            cs.socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket Close on BeginSend" + ex.ToString());
        }
    }

    /// <summary>
    /// 利用socket对象进行发送信息
    /// </summary>
    /// <param name="cs"></param>
    /// <param name="msg"></param>
    public static void Send(Socket cs, MsgBase msg)
    {//状态判断
        if (cs == null)
        {
            return;
        }
        if (!cs.Connected)
        {
            return;
        }
        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);

        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];

        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);

        //组装名字
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);

        //组装消息体
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        //TODO,简化代码，不设置回调，可以参考客户端,设置回调

        try
        {
            cs.BeginSend(sendBytes, 0, sendBytes.Length, 0, null, null);
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Socket Close on BeginSend" + ex.ToString());
        }
    }

    /// <summary>
    /// 获取时间戳 （提问：1970年的原因，32位，64位）
    /// </summary>
    /// <returns></returns>
    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    }
}