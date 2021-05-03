using NetCoreAudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace SmartMusicNetCoreAudio
{
    internal class MainStar
    {
        private static string iP = "127.0.0.1";//服务器地址
        private static int port = 8888;//服务器端口号
        private static string AudioProcessCommand = "amixer -M set PCM {0}";
        private static string musicPath;

        /// <summary>
        /// 音乐路径
        /// </summary>
        private static List<string> musicListPath = new List<string>();

        private static List<string> musicName = new List<string>();

        private static Player player = new Player();

        /// <summary>
        /// 当前选中的音乐
        /// </summary>
        private static string currtMusic = "";

        private static int currtMusicInt = 0;

        private static void Main(string[] args)
        {
            Console.WriteLine("Hello SmartMusic!");

            Console.WriteLine("输入目标服务器的IP，输入1默认本机IP");//监听特定端口可以指定 ip 否则any监听所有ip
            Console.WriteLine("121.4.107.99");
            string iP = Console.ReadLine();
            if (iP ==""|| iP == null)
            {
                iP = "121.4.107.99";
            }
            else if (iP == "1")
            {
                iP = IPAddress.Loopback.ToString();
            }

            //Socket Test=new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //IPAddress iP2 = IPAddress.Parse(iP);
            //IPEndPoint point = new IPEndPoint(iP2, port);
            //Test.Connect(point);//连接远程服务器

            //Console.WriteLine("开始连接服务器");
            //192.168.58.131

            Net.ContrentNet(iP, port, "嵌入式端");
            //加入退出事件，通知服务器关闭socket
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            FindMusic();
            ShowMenu();
            KeyFind();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("通知服务器关闭连接");

            Net.Close();
        }

        /// <summary>
        /// 查看音乐目录
        /// </summary>
        public static void ShowMusicMenu()
        {
            int nameNum = 0;
            MsgMusicMenu msg = new MsgMusicMenu();
            foreach (var item in musicName)
            {
                msg.musicNames.names.Add(item);
                Console.WriteLine(String.Format("--------{0}{1}", nameNum, item));
                nameNum++;
            }
            msg.currMusicNnmber = currtMusicInt;
            NetManager.Send(msg, Net.socketSend);
        }

        private static void SendCurrentMusic()
        {
            MsgMusicMenu msg = new MsgMusicMenu();
            foreach (var item in musicName)
            {
                msg.musicNames.names.Add(item);
            }
            msg.currMusicNnmber = currtMusicInt;
            NetManager.Send(msg, Net.socketSend);
        }

        public static void ShowMenu()
        {
            Console.WriteLine("|------------------------------|");
            Console.WriteLine("|----------音乐播放------------|");
            Console.WriteLine("|------------------------------|");
            Console.WriteLine("|----------1.播放--------------|");
            Console.WriteLine("|----------2.暂停--------------|");
            Console.WriteLine("|----------3.停止--------------|");
            Console.WriteLine("|----------4.上一曲------------|");
            Console.WriteLine("|----------5.下一曲------------|");
            Console.WriteLine("|----------6.音乐目录----------|");
            Console.WriteLine("|----------7.菜单--------------|");
            Console.WriteLine("|----------8.AD负责音量键------|");
            Console.WriteLine("|----------0.退出--------------|");
            Console.WriteLine("|------按对应键选择，0退出-----|");
            Console.WriteLine("|------------------------------|");
        }

        private static float audio = 5;

        public static void KeyFind()
        {
            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                
                switch (info.Key)
                {
                    case ConsoleKey.D1:
                        Console.WriteLine("播放音乐");
                        PlayMusic();
                        break;

                    case ConsoleKey.D2:
                        Console.WriteLine("暂停");
                        PauseMusic();
                        break;

                    case ConsoleKey.D3:
                        Console.WriteLine("停止");
                        StopMusic();
                        break;

                    case ConsoleKey.D4:
                        Console.WriteLine("上一曲");
                        UpMusic();
                        break;

                    case ConsoleKey.D5:
                        Console.WriteLine("下一曲");
                        DownMusic();
                        break;

                    case ConsoleKey.D6:
                        Console.WriteLine("音乐目录");
                        ShowMusicMenu();
                        break;

                    case ConsoleKey.D7:
                        Console.WriteLine("目录");
                        ShowMenu();
                        break;

                    case ConsoleKey.D0:
                        Console.WriteLine("退出");

                        StopMusic();
                        return;
                        break;

                    case ConsoleKey.A:

                        if (audio <= 100)
                        {
                            audio += 5;

                            ChangeAudio(audio);
                            
                        }
                        else if(audio>100)
                        {
                            audio = 100;
                            ChangeAudio(audio);
                            
                        }
                        break;

                    case ConsoleKey.D:

                        if ( audio >= 0)
                        {
                            audio -= 5;
                            ChangeAudio(audio);

                           
                        }
                        else if (audio < 0)
                        {
                            audio = 0;
                            ChangeAudio(audio);
                            
                        }
                        break;
                    //case ConsoleKey.DownArrow:
                    //case ConsoleKey.S:
                    //    Console.WriteLine("Down");
                    //    break;
                    //case ConsoleKey.LeftArrow:
                    //case ConsoleKey.D:
                    //    Console.WriteLine("Left");
                    //    break;
                    //case ConsoleKey.RightArrow:
                    //case ConsoleKey.A:
                    //    Console.WriteLine("Right");
                    //    break;
                    default:
                        Console.WriteLine(info.Key);
                        Console.WriteLine("按键不对，请选择0-8");
                        break;
                }
            }
        }

        protected static Process StartBashProcess(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            return process;
        }

        /// <summary>
        /// 在音乐目录寻找音乐
        /// </summary>
        public static void FindMusic()
        {
            //string rootPath = "..\\..\\";

            //Console.WriteLine("当前文件夹路径为：" + rootPath);
            try
            {
                //musicPath = rootPath + "/Music";
                musicPath = "Music";

                //拿到music下的所有音乐
                string path = musicPath;
                DirectoryInfo root = new DirectoryInfo(path);
                FileInfo[] files = root.GetFiles();

                foreach (var item in files)
                {
                    Console.WriteLine("音乐文件的路径" + item);

                    musicListPath.Add(item.FullName);
                    musicName.Add(Path.GetFileName(item.FullName));
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 下一曲
        /// </summary>
        public static void DownMusic()
        {
            player.Stop();

            //获得当前选中项,且往下
            int index = currtMusicInt;

            index++;
            if (index == musicListPath.Count)
            {
                index = 0;
            }
            currtMusicInt = index;//选中索引 往下
            currtMusic = musicListPath[index];

            priPlaymusic(currtMusic);
            SendCurrentMusic();
        }

        /// <summary>
        /// 上一曲
        /// </summary>
        public static void UpMusic()
        {
            player.Stop();

            //获得当前选中项,且往上
            int index = currtMusicInt;

            index--;
            if (index < 0)
            {
                index = musicListPath.Count - 1;
            }
            currtMusicInt = index;//选中索引 往上
            currtMusic = musicListPath[index];
            priPlaymusic(currtMusic);
            SendCurrentMusic();
        }

        /// <summary>
        /// 停止音乐
        /// </summary>
        public static void StopMusic()
        {
            try
            {
                player.Stop();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 暂停当前的音乐
        /// </summary>
        public static void PauseMusic()
        {
            try
            {
                player.Pause();
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 播放音乐
        /// </summary>
        public static void PlayMusic()
        {
            if (player.Paused)
            {
                player.Resume();
                return;
            }
            if (musicListPath.Count != 0 && musicListPath != null && currtMusic == "")
            {
                currtMusic = musicListPath[currtMusicInt];//音乐路径存在，且当前音乐无，播放第一个音乐
            }
            currtMusic = musicListPath[currtMusicInt];

           

            priPlaymusic(currtMusic);
        }

        /// <summary>
        /// 修改音量 0-100
        /// </summary>
        /// <param name="audioNum"></param>
        public static void ChangeAudio(float audioNum)
        {
            
            if (player.Playing)
            {
                Console.WriteLine("当前音量为" + audioNum);
                //error 修改声音
                string cmd = string.Format(AudioProcessCommand, audioNum);
                var tempProcess = StartBashProcess(cmd);
                //tempProcess.WaitForExit();
                //player.Audio = audioNum;
            }
        }

        public static void priPlaymusic(string path)
        {
            player.Play(path);
        }
    }
}