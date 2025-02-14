namespace DummyClient;

using ServerCore;
using System.Net;
using System.Text;

public enum PacketID
{
    PlayerInfoReq = 1,
    Test = 2,

}


class PlayerInfoReq
{
    public byte testByte;
    public long playerId;
    public string name;
    public class Skill
    {
        public int id;
        public short level;
        public float duration;
        public class Attribute
        {
            public int att;

            public void Read(ReadOnlySpan<byte> s, ref ushort count)
            {
                this.att = BitConverter.ToInt32(s.Slice(count, s.Length - count));
                count += sizeof(int);
            }

            public bool Write(Span<byte> s, ref ushort count)
            {
                bool success = true;
                success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), att);
                count += sizeof(int);
                return success;
            }
        }
        public List<Attribute> attributes = new List<Attribute>();


        public void Read(ReadOnlySpan<byte> s, ref ushort count)
        {
            this.id = BitConverter.ToInt32(s.Slice(count, s.Length - count));
            count += sizeof(int);
            this.level = BitConverter.ToInt16(s.Slice(count, s.Length - count));
            count += sizeof(short);
            this.duration = BitConverter.ToSingle(s.Slice(count, s.Length - count));
            count += sizeof(float);
            this.attributes.Clear();
            ushort attributeLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
            count += sizeof(ushort);
            for (int i = 0; i < attributeLen; i++)
            {
                Attribute attribute = new Attribute();
                attribute.Read(s, ref count);
                attributes.Add(attribute);
            }
        }

        public bool Write(Span<byte> s, ref ushort count)
        {
            bool success = true;
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), id);
            count += sizeof(int);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), level);
            count += sizeof(short);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), duration);
            count += sizeof(float);
            success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.attributes.Count);
            count += sizeof(ushort);
            foreach (Attribute attribute in attributes)
                success &= attribute.Write(s, ref count);
            return success;
        }
    }
    public List<Skill> skills = new List<Skill>();


    public void Read(ArraySegment<byte> seg)
    {
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        ushort count = 0;

        count += sizeof(ushort); // 패킷 길이
        count += sizeof(ushort); // 패킷 id
        this.testByte = (byte)seg.Array[seg.Offset + count];
        count += sizeof(byte);
        this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
        count += sizeof(long);
        ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        count += nameLen;
        this.skills.Clear();
        ushort skillLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        for (int i = 0; i < skillLen; i++)
        {
            Skill skill = new Skill();
            skill.Read(s, ref count);
            skills.Add(skill);
        }
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        count += sizeof(ushort); // size
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.PlayerInfoReq);
        count += sizeof(ushort);
        seg.Array[seg.Offset + count] = (byte)this.testByte;
        count += sizeof(byte);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), playerId);
        count += sizeof(long);
        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, seg.Array, seg.Offset + count + sizeof(ushort)); // 실제 데이터 카피를 먼저 함
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        count += sizeof(ushort);
        count += nameLen;
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)this.skills.Count);
        count += sizeof(ushort);
        foreach (Skill skill in skills)
            success &= skill.Write(s, ref count);

        success &= BitConverter.TryWriteBytes(s, count); // size를 마지막에 넣어줘야 함
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}

class Test
{
    public int testInt;

    public void Read(ArraySegment<byte> seg)
    {
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        ushort count = 0;

        count += sizeof(ushort); // 패킷 길이
        count += sizeof(ushort); // 패킷 id
        this.testInt = BitConverter.ToInt32(s.Slice(count, s.Length - count));
        count += sizeof(int);
    }

    public ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        count += sizeof(ushort); // size
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), (ushort)PacketID.Test);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), testInt);
        count += sizeof(int);

        success &= BitConverter.TryWriteBytes(s, count); // size를 마지막에 넣어줘야 함
        if (success == false)
            return null;
        return SendBufferHelper.Close(count);
    }
}



class ServerSession : Session
{
    // TryWriteBytes 이외 방법: C++ 같이 짜면 속도가 훨씬 빨라짐. 현재 사용하지 않음
    //static unsafe void ToBytes(byte[] array, int offset, ulong value)
    //{
    //    fixed (byte* ptr = &array[offset])
    //        *(ulong*)ptr = value;
    //}

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");

        PlayerInfoReq packet = new() { playerId = 1001, name = "ABCD" };
        var skill = new PlayerInfoReq.Skill() { id = 101, level = 1, duration = 3.0f };
        skill.attributes.Add(new PlayerInfoReq.Skill.Attribute() { att = 77 });
        packet.skills.Add(skill);
        packet.skills.Add(new PlayerInfoReq.Skill() { id = 201, level = 2, duration = 4.0f });
        packet.skills.Add(new PlayerInfoReq.Skill() { id = 301, level = 3, duration = 5.0f });
        packet.skills.Add(new PlayerInfoReq.Skill() { id = 401, level = 4, duration = 6.0f });

        //for (int i = 0; i < 5; i++)
        {
            // 보낸다
            ArraySegment<byte> s = packet.Write();

            if (s != null)
                Send(s);
        }
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected: {endPoint}");
    }

    public override int OnRecv(ArraySegment<byte> buffer)
    {
        string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        Console.WriteLine($"[From Server] {recvData}");
        return buffer.Count; // 임시
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Transferred Bytes: {numOfBytes}");
    }
}
