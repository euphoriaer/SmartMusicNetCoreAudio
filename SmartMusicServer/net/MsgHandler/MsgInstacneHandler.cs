using System;

public partial class MsgHandler
{
    public static void MsgInstance(ClientState c, MsgBase msgBase)//todo 反射那里通过invoke 传过来两个参数
    {
        try
        {
            MsgInstance msgInstance = msgBase as MsgInstance;
            Console.WriteLine("[服务器]:收到客户端实例化请求:content=" + msgInstance.id);
            

            if (ConnectManager.entity.ContainsKey(msgInstance.id))
            {
                Console.WriteLine("[服务器]:要添加的对象已经存在:重置");

                ConnectManager.entity[msgInstance.id] = c;
            }
            else
            {
                c.id = msgInstance.id;

                //将连接进来的对象添加到列表  id 和 client对象
                ConnectManager.AddPlayer(msgInstance.id, c);
                ConnectManager.sockets.Add(c.socket);
                //对id添加，赋值
                ConnectManager.msgs.Add(msgInstance.id);
            }

          
            MsgOrder msg = new MsgOrder("PlayMusic");
            NetManager.Send(c, msg);
            Console.WriteLine("[服务器]:发送开始播放音乐");
          
        }
        catch (Exception)
        {

            
        }
        
    }




  
}