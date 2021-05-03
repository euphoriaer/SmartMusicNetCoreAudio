using SmartMusicNetCoreAudio;
using System;

public partial class MsgHandler
{
    public static void MsgAudio(MsgBase msgBase)
    {
        Console.WriteLine("收到音量改变的命令");
        MsgAudio msg = msgBase as MsgAudio;
        MainStar.ChangeAudio(msg.audio);
    }

    public static void MsgOrder(MsgBase msgBase)
    {
        MsgOrder msg = msgBase as MsgOrder;
        Console.WriteLine("收到命令：" + msg.order);
        Function(msg.order);

    }

    private static void Function(string str)
    {//PlayMusic, StopMusic, PauseMusic, DownMusic, UpMusic,
        if (str == "PlayMusic")
        {
            MainStar.PlayMusic();
        }
        else if (str == "PauseMusic")
        {
            MainStar.PauseMusic();
        }
        else if (str == "UpMusic")
        {
            MainStar.UpMusic();
        }
        else if (str == "DownMusic")
        {
            MainStar.DownMusic();
        }
        else if (str == "StopMusic")
        {
            MainStar.StopMusic();
        }
        else if (str == "MusicList")
        {
            MainStar.ShowMusicMenu();
        }
        
        return;
    }
}