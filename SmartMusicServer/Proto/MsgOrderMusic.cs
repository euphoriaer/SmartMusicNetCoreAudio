using SmartMusicServer.Proto;
using System.Net.Sockets;

/// <summary>
/// 智能音箱功能
/// </summary>
public class MsgOrderMusic : MsgBase
{
    public MsgOrderMusic() : base("MsgOrder")
    {
    }

    /// <summary>
    /// 要操作的对象（智能音箱ID） 嵌入式回
    /// </summary>
    public string id = "";

    /// <summary>
    /// 音箱命令 服务器回
    /// </summary>
    public SmartState order;

    /// <summary>
    /// 音箱当前状态，嵌入式回
    /// </summary>
    public SmartState current;


    public Socket client;
}

public enum SmartState
{
    Play, UpMusic, DownMusic, Pause, Stop, AudioUp, AudioDrop,Close
}