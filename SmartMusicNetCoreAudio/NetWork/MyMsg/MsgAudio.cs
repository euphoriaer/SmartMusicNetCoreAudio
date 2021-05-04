public class MsgAudio : MsgBase
{
    public MsgAudio(string protoName) : base("MsgAudio")
    {
    }
    public MsgAudio() : base("MsgAudio")
    {

    }
    /// <summary>
    /// 声音大小0-1
    /// </summary>
    public int audio;
}