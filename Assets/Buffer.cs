using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Buffer
{
    struct Packet
    {
        public int pos;
        public int size;
    };

    MemoryStream stream; //실제 메모리
    List<Packet> list; 
    int pos = 0;

    Object o = new Object();

    public Buffer()
    {
        stream = new MemoryStream();
        list = new List<Packet>();
    }

    public int Write(byte[] bytes, int length)
    {
        Packet packet = new Packet();

        packet.pos = pos;
        packet.size = length;

        lock (o)
        { //데이터를 수정할 때는 아무도 못 건들게 락을 건다.
            list.Add(packet);


            stream.Position = pos;
            stream.Write(bytes, 0, length);
            stream.Flush();
            pos += length;
        }

        return length;
    }

    public int Read(ref byte[] bytes, int length)
    {
        if (list.Count <= 0)
            return -1;

        int ret = 0;
        lock (o)
        {
            Packet packet = list[0];

            int dataSize = Math.Min(length, packet.size);//실제 데이터 사이즈, 저장된 사이즈 중에 작은거 읽어옴.
            stream.Position = packet.pos; //데이터 시작 지점이 저장되어 있음.
            ret = stream.Read(bytes, 0, dataSize);

            if (ret > 0)
                list.RemoveAt(0);

            if (list.Count == 0)
            {//생략 가능함. 한 번 더 깨끗하게 지워줌.
                byte[] b = stream.GetBuffer();
                Array.Clear(b, 0, b.Length);

                stream.Position = 0;
                stream.SetLength(0);

                pos = 0;
            }
        }

        return ret;
    }
}
