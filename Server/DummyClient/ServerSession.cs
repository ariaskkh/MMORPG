using ServerCore;
using System.Net;
using System.Text;

namespace DummyClient;

class Packet
{
    public ushort size;
    public ushort packetId; // packet 종류 구분
}

class PlayerInfoReq : Packet
{
    public long playerId;
}

class PlayerInfoOk : Packet
{
    public int hp;
    public int attack;
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

        PlayerInfoReq packet = new() { packetId = (ushort)PacketID.PlayerInfoReq, playerId = 1001 };

        //for (int i = 0; i < 5; i++)
        {
            // 보낸다
            ArraySegment<byte> seg = SendBufferHelper.Open(4096);
            
            ushort count = 0;
            bool success = true;

            //success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), packet.size);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.packetId);
            count += 2;
            success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packet.playerId);
            count += 8;
            success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), count); // size를 마지막에 넣어줘야 함
            
            ArraySegment<byte> sendBuff = SendBufferHelper.Close(count);

            if (success)
                Send(sendBuff);
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
