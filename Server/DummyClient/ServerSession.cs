using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient;

public abstract class Packet
{
    public ushort size;
    public ushort packetId; // packet 종류 구분

    public abstract ArraySegment<byte> Write();
    public abstract void Read(ArraySegment<byte> s);
}

class PlayerInfoReq : Packet
{
    public long playerId;
    public string name;

    public PlayerInfoReq()
    {
        packetId = (ushort)PacketID.PlayerInfoReq;
        playerId = 1001;
    }

    // ClientSession의 OnRecvPacket 코드 가져옴. 왜?
    public override void Read(ArraySegment<byte> seg)
    {
        ReadOnlySpan<byte> s = new ReadOnlySpan<byte>(seg.Array, seg.Offset, seg.Count);
        ushort count = 0;
        
        count += sizeof(ushort);
        count += sizeof(ushort);
        this.playerId = BitConverter.ToInt64(s.Slice(count, s.Length - count));
        count += sizeof(long);

        // string
        ushort nameLen = BitConverter.ToUInt16(s.Slice(count, s.Length - count));
        count += sizeof(ushort);
        this.name = Encoding.Unicode.GetString(s.Slice(count, nameLen));
        count += nameLen;
    }

    public override ArraySegment<byte> Write()
    {
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        Span<byte> s = new Span<byte>(seg.Array, seg.Offset, seg.Count);

        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), packetId);
        count += sizeof(ushort);
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), playerId);
        count += sizeof(long);

        // string
        ushort nameLen = (ushort)Encoding.Unicode.GetBytes(this.name, 0, this.name.Length, seg.Array, seg.Offset + count + sizeof(ushort)); // 실제 데이터 카피를 먼저 함
        success &= BitConverter.TryWriteBytes(s.Slice(count, s.Length - count), nameLen);
        count += sizeof(ushort);
        count += nameLen;

        success &= BitConverter.TryWriteBytes(s, count); // size를 마지막에 넣어줘야 함

        if (success == false)
            return null;

        return SendBufferHelper.Close(count);
    }
}

public enum PacketID
{ 
    PlayerInfoReq = 1,
    PlayerInfoOk = 2,
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
