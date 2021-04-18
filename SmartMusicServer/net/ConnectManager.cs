using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;

public class ConnectManager
{
    /// <summary>
    /// 客户端列表
    /// </summary>
    public static Dictionary<string, ClientState> entity = new Dictionary<string, ClientState>();

    /// <summary>
    /// id列表
    /// </summary>
    public static List<string> msgs = new List<string>();

    /// <summary>
    /// 连接列表
    /// </summary>
    public static List<Socket> sockets = new List<Socket>();

    /// <summary>
    /// 移除消息
    /// </summary>
    /// <param name="id"></param>
    public static void RemoveMsg(string id)
    {
        msgs.Remove(id);
    }


    /// <summary>
    /// 添加对象
    /// </summary>
    /// <param name="id"></param>
    /// <param name="player"></param>
    public static void AddPlayer(string id, ClientState player)
    {
        if (!entity.ContainsKey(id))
        {
            entity.Add(id, player);
        }
        
    }
    /// <summary>
    /// 移除对象
    /// </summary>
    /// <param name="id"></param>
    public static void RemovePlayer(string id)
    {
        if (entity.ContainsKey(id))
        {
            entity.Remove(id);
        }
        else
        {
            Console.WriteLine("要删除的对象不存在");
        }
    }

   

    public static void RemovePlayer(ClientState client)
    {
        if (client == null)
        {
            return;
        }

        if (entity.ContainsValue(client))
        {
            entity.Remove(client.id);
        }
        else
        {
            Console.WriteLine("要删除的玩家不存在");
        }
    }

    ///// <summary>
    ///// 广播消息(全体)
    ///// </summary>
    ///// <param name="msg"></param>
    //public static void DistributeMsg(MsgBase msg)
    //{
    //    foreach (var player in players.Values)
    //    {
    //        player.Send(msg);
    //    }
    //}
    /// <summary>
    ///  广播消息
    /// </summary>
    /// <param name="msg"></param>
    public static void NewMethod(MsgBase msg)
    {
        Console.WriteLine("[服务器]:发送消息");
        for (int i = 0; i < ConnectManager.sockets.Count; i++)
        {
            NetManager.Send(ConnectManager.sockets[i], msg);
            Console.WriteLine("[服务器]:回复客户端数量" + i);
        }
    }

}