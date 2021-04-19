using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SmartMusicNetCoreAudio
{
    internal static class Net
    {
        public static Socket socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        /// <summary>
        /// 网络连接
        /// </summary>
        /// <param name="ip">服务器ip</param>
        /// <param name="port">端口号</param>
        /// <param name="clientName">客户端姓名</param>
        public static void ContrentNet(string ip, int port, String clientName)
        {
            //连接网络
            IPAddress iP = IPAddress.Parse(ip);
            IPEndPoint point = new IPEndPoint(iP, port);
            socketSend.Connect(point);//连接远程服务器

            Console.WriteLine("开始连接服务器");

            //连接成功就开始接收消息,开一个线程不停的接收服务器消息
            Thread th = new Thread(Receive);
            th.IsBackground = true;
            th.Start();

            //开启接收之后，向服务器发送 客户端姓名，用一个tag表示是否连接
            MsgInstance msg = new MsgInstance(clientName);

            SendMessage(msg, socketSend);//todo 只执行一次，需要写协议，将事件和消息分开，区分发的是消息，还是客户端姓名，必须要姓名，否则不知道消息转发给谁，
                                         //可以考虑所有消息全部转发，不区分，在不同客户端处理
                                         //todo 全部转发 用list，选择转发用Dic
        }

        /// <summary>
        ///  发送消息
        /// </summary>
        /// <param name="msg"></param>
        public static void SendMessage(MsgBase msg, Socket socket)
        {
            NetManager.Send(msg, socket);
            //try
            //{
            //    byte[] msgs = System.Text.Encoding.UTF8.GetBytes(msg);//转成字节数组
            //                                                          //发送消息
            //    socketSend.Send(msgs);
            //}
            //catch (Exception)
            //{
            //}
        }

        public static void Receive()
        {
            while (true)
            {
                try
                {
                    //Socket socketSend = o as Socket;
                    //负责通信的socket，开始接受消息，直接用socketsend会阻塞，无法接受消息

                    byte[] buffer = new byte[1024 * 1024 * 2];//接收数据的容器
                    int r = socketSend.Receive(buffer);//将一次数据放图buffer缓冲区

                    string s = Encoding.UTF8.GetString(buffer, 0, r);
                    Console.WriteLine("收到了服务器消息：" + s);

                    ByteArray byteArray = new ByteArray(buffer);
                    MsgBase msg = NetManager.OnReceiveData(byteArray);
                    MsgOrder order = msg as MsgOrder;
                    Console.WriteLine("收到命令：" + order.order);
                    Function(order.order);
                }
                catch (Exception)
                {
                }
            }
        }

        private static void Function(string str)
        {//PlayMusic, StopMusic, PauseMusic, DownMusic, UpMusic,
            if (str == "PlayMusic")
            {
                Program.PlayMusic();
            }
            else if (str == "PauseMusic")
            {
                Program.PauseMusic();
            }
            else if (str == "UpMusic")
            {
                Program.UpMusic();
            }
            else if (str == "DownMusic")
            {
                Program.DownMusic();
            }
            else if (str == "StopMusic")
            {
                Program.StopMusic();
            }
            else if(str == "MusicList")
            {
                Program.ShowMusicMenu();
            }

            return;
        }

        public static void Close()
        {
            socketSend.Close();
        }
    }
}