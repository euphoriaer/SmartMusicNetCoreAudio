using System;

public partial class MsgHandler
{
    public static void MsgOrder(ClientState c, MsgBase msgBase)
    {
        
            //命令只有客户端发
            MsgOrder msg = msgBase as MsgOrder;
            Console.WriteLine("[服务器] 转发协议：" + msg.order.ToString());

            MsgOrder msgPong = new MsgOrder(msg.order.ToString());

        if (ConnectManager.entity.ContainsKey("嵌入式端"))
        {

            NetManager.Send(ConnectManager.entity["嵌入式端"], msgPong);
            Console.WriteLine("[服务器]:回复嵌入式端："+msgPong.order.ToString());
        }
        else
        {
            Console.WriteLine("[服务器]:嵌入式端未上线：" + msgPong.order.ToString());
        }

       
       
    }


}

public partial class MsgHandler
{
    public static void MsgTest(ClientState c, MsgBase msgBase)
    {
        Console.WriteLine("[服务器]:MsgTest-----------------");
        c.lastPingTime = NetManager.GetTimeStamp();
        MsgTest msgPong = new MsgTest();
        NetManager.Send(c, msgPong);
        Console.WriteLine("[服务器]MsgTest--------------------");
    }
}