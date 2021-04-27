using System;

public partial class MsgHandler
{
    public static void MsgMusicMenu(ClientState c, MsgBase msgBase)//todo 反射那里通过invoke 传过来两个参数
    {
        try
        {
            MsgMusicMenu msg = msgBase as MsgMusicMenu;

            MsgMusicMenu msg2 = new MsgMusicMenu();

            foreach (var item in msg.musicNames.names)
            {
                msg2.musicNames.names.Add(item);
            }
            msg2.currMusicNnmber = msg.currMusicNnmber;
            if (ConnectManager.entity.ContainsKey("手机客户端"))
            {
                NetManager.Send(ConnectManager.entity["手机客户端"], msg2);

                Console.WriteLine("[服务器]:发送音乐目录");
            }
            else
            {
                Console.WriteLine("[服务器]:音乐目录不存在");
            }
        }
        catch (Exception)
        {
        }
    }
}