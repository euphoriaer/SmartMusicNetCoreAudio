using System;

public partial class MsgHandler
{
    public static void MsgAudio(ClientState c, MsgBase msgBase)
    {
        //命令只有客户端发
        MsgAudio msg = msgBase as MsgAudio;
        Console.WriteLine("[服务器] 转发协议：" + msg.ToString());

        MsgAudio msg2 = new MsgAudio(msg.ToString());
        msg2.audio = msg.audio;
        if (ConnectManager.entity.ContainsKey("嵌入式端"))
        {
            NetManager.Send(ConnectManager.entity["嵌入式端"], msg2);
            Console.WriteLine("[服务器]:回复嵌入式端：" + msg2.ToString());
        }
        else
        {
            Console.WriteLine("[服务器]:嵌入式端未上线：" + msg2.ToString());
        }
    }
}