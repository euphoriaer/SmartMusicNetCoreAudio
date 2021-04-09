using NetCoreAudio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SmartMusicNetCoreAudio
{
    internal class Program
    {
        private static string iP = "127.0.0.1";//服务器地址
        private static int port = 9910;//服务器端口号

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
            string iP = Console.ReadLine();
            if (iP == "1" || iP == "" || iP == null)
            {
                iP = IPAddress.Loopback.ToString();
            }
            //192.168.58.131

            Net.ContrentNet(iP, port, "智能音箱1");
            //加入退出事件，通知服务器关闭socket
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);

            FindMusic();
            ShowMenu();
            KeyFind();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("通知服务器关闭连接");
            Net.SendMessage("关闭连接");//todo 字符串判断关闭，最好还是协议头和协议体
            Net.Close();
        }

        /// <summary>
        /// 查看音乐目录
        /// </summary>
        public static void ShowMusicMenu()
        {
            int nameNum = 0;
            foreach (var item in musicName)
            {
                Console.WriteLine(String.Format("--------{0}{1}", nameNum, item));
                nameNum++;
            }
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
            Console.WriteLine("|----------0.退出--------------|");
            Console.WriteLine("|------------------------------|");
            Console.WriteLine("|------按对应键选择，0退出-----|");
            Console.WriteLine("|------------------------------|");
        }

        public static void KeyFind()
        {
            Console.WriteLine("开始播放音乐");

            while (true)
            {
                ConsoleKeyInfo info = Console.ReadKey();
                Console.WriteLine("按下了：" + info.KeyChar);
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
                        Console.WriteLine("按键不对，请选择0-7");
                        break;
                }
            }
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

            //播放
            //try
            //{
            //}
            //catch (Exception)
            //{
            //}

            priPlaymusic(currtMusic);
        }

        public static void priPlaymusic(string path)
        {
            player.Play(path);
        }
    }
}