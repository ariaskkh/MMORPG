using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
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

    public PlayerInfoReq()
    {
        packetId = (ushort)PacketID.PlayerInfoReq;
        playerId = 1001;
    }

    // ClientSession의 OnRecvPacket 코드 가져옴. 왜?
    public override void Read(ArraySegment<byte> s)
    {
        ushort count = 0;
        
        //ushort size = BitConverter.ToUInt16(s.Array, s.Offset + count);
        count += 2;
        //ushort id = BitConverter.ToUInt16(s.Array, s.Offset + count);
        count += 2;
        this.playerId = BitConverter.ToInt64(new ReadOnlySpan<byte>(s.Array, s.Offset + count, s.Count - count));
        count += 8;
    }

    public override ArraySegment<byte> Write()
    {
        ArraySegment<byte> s = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        //success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), packet.size);
        count += 2;
        success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), packetId);
        count += 2;
        success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset + count, s.Count - count), playerId);
        count += 8;
        success &= BitConverter.TryWriteBytes(new Span<byte>(s.Array, s.Offset, s.Count), count); // size를 마지막에 넣어줘야 함

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

        PlayerInfoReq packet = new() { playerId = 1001 };

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
