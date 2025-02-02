﻿using ServerCore;
using System.Net;

namespace Server;

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
        ArraySegment<byte> seg = SendBufferHelper.Open(4096);

        ushort count = 0;
        bool success = true;

        //success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), packet.size);
        count += 2;
        success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), packetId);
        count += 2;
        success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset + count, seg.Count - count), playerId);
        count += 8;
        success &= BitConverter.TryWriteBytes(new Span<byte>(seg.Array, seg.Offset, seg.Count), count); // size를 마지막에 넣어줘야 함

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

class ClientSession : PacketSession
{
    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"OnConnected: {endPoint}");
        //byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !!!");
        //Packet packet = new() { size = 100, packetId = 10 };
        // [ 100 ][ 10 ]
        //byte[] sendBuff = new byte[4096];

        //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
        //byte[] buffer = BitConverter.GetBytes(packet.size);
        //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
        //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
        //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
        //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

        //// 100명
        //// 1 -> 이동패킷이 100명
        //// 100 -> 이동패킷이  100 * 100 = 1만
        //Send(sendBuff);
        Thread.Sleep(1000);
        Disconnect();
    }

    // 딱 유효한 부분만 받음
    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        ushort count = 0;
        ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;
        ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
        count += 2;

        switch ((PacketID)id)
        {
            case PacketID.PlayerInfoReq:
                {
                    PlayerInfoReq p = new PlayerInfoReq();
                    p.Read(buffer);
                    Console.WriteLine($"PlayerInfoReq: {p.playerId}");
                }
                break;
        }

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