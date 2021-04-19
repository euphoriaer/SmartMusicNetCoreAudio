using System;
using UnityEngine;

public class ByteArray
{//默认大小
    private const int DEFAULT_SIZE = 1024;

    //初始大小
    private int initSize = 0;

    //缓冲区
    public byte[] bytes;

    //读写位置
    public int readIdx = 0;

    public int writeIdx = 0;


    //容量
    private int capacity = 0;

    //剩余空间
    public int remain
    {
        get
        {
            return capacity - writeIdx;
        }
    }

    //数据长度
    public int length
    {
        get
        {
            return writeIdx - readIdx;
        }
    }

    public ByteArray(int size = DEFAULT_SIZE)
    {
        bytes = new byte[size];
        capacity = size;
        initSize = size;
        readIdx = 0;
        writeIdx = 0;
    }

    //构造函数
    public ByteArray(byte[] defaultBytes)
    {
        bytes = defaultBytes;
        capacity = defaultBytes.Length;
        initSize = defaultBytes.Length;
        readIdx = 0;
        writeIdx = defaultBytes.Length;
    }

    /// <summary>
    /// 重设尺寸
    /// </summary>
    /// <param name="size"></param>
    public void ReSize(int size)
    {
        if (size < length)
            return;

        if (size < initSize)
            return;

        int n = 1;

        while (n < size)
            n *= 2;

        capacity = n;
        byte[] newBytes = new byte[capacity];

        Array.Copy(bytes, readIdx, newBytes, 0, writeIdx - readIdx);

        bytes = newBytes;

        writeIdx = length;
        readIdx = 0;
    }

    /// <summary>
    /// 移动数据
    /// </summary>
    public void CheckAndMoveBytes()
    {
        if (length < 8)
        {
            MoveBytes();
        }
    }

    public void MoveBytes()
    {
        Array.Copy(bytes, readIdx, bytes, 0, length);
        writeIdx = length;
        readIdx = 0;
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="bs">要写的数据</param>
    /// <param name="offset">开始写入的位置</param>
    /// <param name="count">写入的数据数量</param>
    /// <returns></returns>
    public int Write(byte[] bs, int offset, int count)
    {
        if (remain < count)
        {
            ReSize(length + count);
        }
        Array.Copy(bs, offset, bytes, writeIdx, count);
        writeIdx += count;
        return count;
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="bs">传出的数据</param>
    /// <param name="offset">开始读的位置</param>
    /// <param name="count">读取的数据数量</param>
    /// <returns></returns>
    public int Read(byte[] bs, int offset, int count)
    {
        count = Math.Min(count, length);
        Array.Copy(bytes, 0, bs, offset, count);

        readIdx += count;
        CheckAndMoveBytes();
        return count;
    }

    /// <summary>
    /// 读取Int16
    /// </summary>
    /// <returns></returns>
    public Int16 ReadInt16()
    {
        if (length < 2)
        {
            return 0;
        }

        //TODO 理解一下这句ret 和<<
        Int16 ret = (Int16)((bytes[1] << 8) | bytes[0]);
        readIdx += 2;
        CheckAndMoveBytes();
        return ret;
    }

    public Int32 ReadInt32()
    {
        if (length < 4)
        {
            return 0;
        }
        //TODO 理解一下这句<< |
        Int32 ret = (Int32)((bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0]);
        readIdx += 4;
        CheckAndMoveBytes();
        return ret;
    }

    //打印缓冲区（调试用）
    public override string ToString()
    {
        return BitConverter.ToString(bytes, readIdx, writeIdx);
    }

    public string Debug()
    {
        return string.Format("readIdx({0}) writeIdx({1}) bytes({2})", readIdx, writeIdx, BitConverter.ToString(bytes, 0, bytes.Length));
    }
}