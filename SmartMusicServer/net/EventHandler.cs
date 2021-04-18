using System;

public partial class EventHandler
{
    public static void OnDisconnect(ClientState c)
    {
        Console.WriteLine("Close");
       
    }

    public static void OnTimer()
    {
        CheckPing();
       
    }

    /// <summary>
    /// ping检查 默认间隔时间x4，即120秒视为断开连接
    /// </summary>
    private static void CheckPing()
    {
        //现在的时间戳
        long timeNow = NetManager.GetTimeStamp();
        //遍历，删除
        foreach (var s in NetManager.clients.Values)
        {
            if (timeNow - s.lastPingTime > NetManager.pingInterval * 4)
            {
                ConnectManager.RemovePlayer(s.id);
                ConnectManager.RemoveMsg(s.id);
                Console.WriteLine("Ping Close" + s.socket.RemoteEndPoint.ToString());
                NetManager.Close(s);
                //continue;
                return;//TODO 每次遍历找到一个断开连接的就 退出遍历，书中说继续遍历，有可能会出错，尝试下，将遍历进行完毕是否会出错，如果出错，为什么会出错，使用continue？跳过当前遍历可否？
            }
        }
    }
}