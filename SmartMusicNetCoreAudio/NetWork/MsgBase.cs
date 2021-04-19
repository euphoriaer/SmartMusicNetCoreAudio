using LitJson;
using System;


public class MsgBase
{
    /// <summary>
    /// 协议名(可以写成构造函数，让子类必须传参数过来)
    /// </summary>
    public string protoName = "";

    public MsgBase(string protoName)
    {
        this.protoName = protoName;
    }

    /// <summary>
    /// 编码协议体
    /// </summary>
    /// <param name="msgBase">协议对象（需要编码的数据）</param>
    /// <returns></returns>
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonMapper.ToJson(msgBase);
        return System.Text.Encoding.UTF8.GetBytes(s);
    }

    /// <summary>
    /// 编码协议名
    /// </summary>
    /// <param name="msgBase"></param>
    /// <returns></returns>
    public static byte[] EncodeName(MsgBase msgBase)
    {//名字和长度
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msgBase.protoName);

        Int16 len = (Int16)nameBytes.Length;
        //申请Bytes数组
        byte[] bytes = new byte[2 + len];
        //组装2字节的长度信息
        bytes[0] = (byte)(len % 256);
        bytes[1] = (byte)(len / 256);
        //组装名字bytes
        Array.Copy(nameBytes, 0, bytes, 2, len);

        return bytes;
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="protoName">协议名</param>
    /// <param name="bytes">缓冲区</param>
    /// <param name="offset">起始位置</param>
    /// <param name="count">长度</param>
    /// <returns></returns>
    public static MsgBase Decode(string protoName, byte[] bytes, int offset, int count)
    {
      
        string s = System.Text.Encoding.UTF8.GetString(bytes, offset, count);
        //MsgBase msgbase = (MsgBase)JsonUtility.FromJson(s, Type.GetType(protoName));
        MsgBase msgbase = (MsgBase)JsonMapper.ToObject(s, Type.GetType(protoName));//error  使用LitJson进行解码如果 
                                                                                   //error 使用LitJson进行解码,JsonUtility会解析失败且没有错误提示
       
        return msgbase;
    }

    /// <summary>
    /// 解码协议名
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static string DecodeName(byte[] bytes, int offset, out int count)
    {
        //必须大于2字节
        if (offset + 2 > bytes.Length)
        {
            count = 0;
            return "";
        }
        //读取长度
        Int16 len = (Int16)((bytes[offset + 1 << 8]) | bytes[offset]);

        //长度必须足够
        if (offset + 2 + len > bytes.Length)
        {
            count = 0;
            return "";
        }
        //解析
        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        return name;
    }
}