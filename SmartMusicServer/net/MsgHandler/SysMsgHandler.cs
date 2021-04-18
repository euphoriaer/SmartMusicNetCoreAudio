using System;

/// <summary>
/// 消息管理器，是收到消息，利用委托进行分发消息用的
/// </summary>
public  partial  class MsgHandler
{
    public static void MsgPing(ClientState c, MsgBase msgBase)
    {
        Console.WriteLine("[服务器]:收到MsgPing");
        c.lastPingTime = NetManager.GetTimeStamp();
        MsgPong msgPong = new MsgPong();
        NetManager.Send(c, msgPong);
        Console.WriteLine("[服务器]发送MsgPong");
    }
}