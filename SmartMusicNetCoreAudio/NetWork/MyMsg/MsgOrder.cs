public class MsgOrder : MsgBase
{
    public MsgOrder(string order) : base("MsgOrder")
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

public class MsgTest : MsgBase
{
    public MsgTest() : base("MsgTest")
    {
    }
}

public enum SmartOrder
{
    PlayMusic, StopMusic, PauseMusic, DownMusic, UpMusic,
}