using ServerCore;
using System.Net;

namespace Server;

class Packet
{
    public ushort size;
    public ushort packetId; // packet 종류 구분
}

class ClientSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");
        //byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !!!");
        Packet packet = new() { size = 100, packetId = 10 };
        // [ 100 ][ 10 ]
        //byte[] sendBuff = new byte[4096];

        ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
        byte[] buffer = BitConverter.GetBytes(packet.size);
        byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
        Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
        Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
        ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

        // 100명
        // 1 -> 이동패킷이 100명
        // 100 -> 이동패킷이  100 * 100 = 1만
        Send(sendBuff);
        Thread.Sleep(1000);
        Disconnect();
    }

    // 딱 유효한 부분만 받음
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + 2);
        Console.WriteLine($"RecvPacketId: {id}, Size: {size}");
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnDisconnected: {endPoint}");
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine($"Transferred Bytes: {numOfBytes}");
    }
}