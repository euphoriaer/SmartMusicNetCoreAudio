using System.Net.Sockets;

public class ClientState
{
    /// <summary>
    /// socket
    /// </summary>
    public Socket socket;

    /// <summary>
    /// 需要读取的数据
    /// </summary>
    public ByteArray readBuff = new ByteArray();

    /// <summary>
    /// Ping
    /// </summary>
    public long lastPingTime = 0;

    public string id;
}