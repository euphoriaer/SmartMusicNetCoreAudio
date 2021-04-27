using System.Collections.Generic;

public class MsgMusicMenu : MsgBase
{
    public MsgMusicMenu(MusicMenu musics) : base("MsgMusicMenu")
    {
        this.musicNames = musics;
    }

    public MsgMusicMenu() : base("MsgMusicMenu")
    {
        musicNames = new MusicMenu();
    }

    /// <summary>
    /// 当前播放音乐
    /// </summary>
    public int currMusicNnmber;

    /// <summary>
    /// 命令客户端发，服务器转发
    /// </summary>
    public MusicMenu musicNames;
}

public class MusicMenu
{

   public List<string> names = new List<string>();
}