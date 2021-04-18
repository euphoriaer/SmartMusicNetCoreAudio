using System.Collections.Generic;

public class MsgInstance : MsgBase
{
    public MsgInstance() : base("MsgInstance")
    {
        ids = new IDs();
    }

    //public Player player;

    /// <summary>
    /// 客户端送 实体名
    /// </summary>
    public string id = "";

    // public string id = "";
    /// <summary>
    /// 服务端回
    /// </summary>
    public IDs ids; //error List 要经过class包裹才能用Json
}

public class IDs
{
    public List<string> entitys;
}
