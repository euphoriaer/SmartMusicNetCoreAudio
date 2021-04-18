using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SmartMusicServer
{
    internal class Program
    {
        private static IPAddress ip = IPAddress.Any;//本机IP，要监听的端口,any监听所有ip,IPAddress.Parse将字符串转为ip
        private static object port = 9910;
        private static readonly int clientsNum = 10;

        private static void Main(string[] args)
        {
            Console.WriteLine("Hello SmartMusicServer!");
            //Console.WriteLine("输入当前服务器的IP，开启监听,输入1默认本机IP");//监听特定端口可以指定 ip 否则any监听所有ip
            //string ipStr=Console.ReadLine();
            //if (ipStr=="1"||ipStr==""||ipStr==null)
            //{
            //    ipStr = IPAddress.Loopback.ToString();
            //}
            ////192.168.58.131

            Thread listemThread = new Thread(LisTenNet);
            listemThread.IsBackground = true;
            listemThread.Start(8888);//传数据
            Console.WriteLine("按下0，退出服务器");//使用线程连接
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                //主线程
                if (info.Key == ConsoleKey.D0)
                {
                    return;
                }
            }
        }

        private static void LisTenNet(object port)
        {
            NetManager.StarLoop((int)port);
        }

        private static void MyNewNet()
        {
            Socket socketWitch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint point = new IPEndPoint(ip, Convert.ToInt32(port));
            //监听
            socketWitch.Bind(point);
            Console.WriteLine("socket监听成功" + point);//使用线程连接
            socketWitch.Listen(clientsNum);//同一时间进入的 socket数量

            Thread listemThread = new Thread(Listen);

            listemThread.IsBackground = true;
            listemThread.Start(socketWitch);//传数据
        }

        // private static Dictionary<string, Socket> clients = new Dictionary<string, Socket>();
        private static List<Socket> clients = new List<Socket>();

        //todo 只执行一次，需要写协议，将事件和消息分开，区分发的是消息，还是客户端姓名，必须要姓名，否则不知道消息转发给谁，需要消息头，消息体
        //可以考虑所有消息全部转发，不区分，在不同客户端处理
        //todo 全部转发 用list，选择转发用Dic
        /// <summary>
        /// 监听函数
        /// </summary>
        /// <param name="o"></param>
        private static void Listen(object o)//因为要开线程，所以只能传object,监听线程
        {
            while (true)
            {
                try
                {
                    Socket socketWitch = o as Socket;
                    //等待客户端连接，并且创建负责通信的socket,
                    socketSend = socketWitch.Accept();
                    //将负责通信的socket 存起来，等待转发消息
                    clients.Add(socketSend);
                    //连接成功
                    Console.WriteLine("连接成功" + socketSend.RemoteEndPoint.ToString());
                    Console.WriteLine(socketSend.RemoteEndPoint + ":发送消息: " + "服务器收到连接");

                    SendMessage(socketSend, "服务器收到连接");
                    //todo 判断连接状态，是否断开

                    Thread th = new Thread(Receive);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch (Exception)
                {
                }
            }
        }

        //负责通信的socket
        private static Socket socketSend;

        /// <summary>
        /// 当连接成功，为新客户端开启线程
        /// </summary>
        /// <param name="o"></param>
        private static void Receive(Object o)
        {
            while (true)
            {
                try
                {
                    Socket socketSend = o as Socket;
                    //负责通信的socket，开始接受消息
                    byte[] buffer = new byte[1024 * 1024 * 2];//字节数
                                                              //实际接收到的数量
                    int r = socketSend.Receive(buffer);//将收到的字节数组传给buffer，返回int表示收到多少个字节

                    if (r == 0)
                    {
                        //客户端下线,跳出循环接收数据
                        Console.WriteLine(socketSend.RemoteEndPoint + ":断开连接");
                        socketSend.Close();//下线断开连接
                        break;
                        //todo 使用select 多路复用进行优化
                    }
                    //todo 使用心跳机制确保客户端活跃
                    string str = Encoding.UTF8.GetString(buffer, 0, r);//todo  从0读到r，优化,加入长度信息，就可以不从0开始读
                    Console.WriteLine(socketSend.RemoteEndPoint + ":收到消息: " + str);
                    //todo 转发收到的消息
                    foreach (var item in clients)
                    {
                        Console.WriteLine("发送消息" + item.RemoteEndPoint + str);
                        SendMessage(item, str);//将收到的消息转发给所有客户端，在收到消息的客户端处理逻辑
                    }
                    if (str == "关闭连接")
                    {
                        Console.WriteLine("已关闭");//todo 虚假的关闭，还是要用事件
                        clients.Remove(socketSend);//关闭后 从队列清除
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="targetCilent">客户端socket对象</param>
        /// <param name="msg">消息内容</param>
        public static void SendMessage(Socket targetCilent, string msg)
        {
            byte[] msgs = System.Text.Encoding.UTF8.GetBytes(msg);//转成字节数组
            //发送消息
            socketSend.Send(msgs);
        }
    }
}