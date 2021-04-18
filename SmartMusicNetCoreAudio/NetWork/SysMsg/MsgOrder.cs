using System;

public class MsgOrder : MsgBase
{
    public MsgOrder(String order) : base("MsgOrder")
    {
        this.order = order;
    }
    public MsgOrder() : base("MsgOrder")
    {

    }
    /// <summary>
    /// 命令客户端发，服务器转发
    /// </summary>
    public string order;
}

public enum SmartOrder
{
    PlayMusic, StopMusic, PauseMusic, DownMusic, UpMusic,
}