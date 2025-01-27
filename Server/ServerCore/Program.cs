using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static Listener _listener = new Listener();

    class GameSession : Session
    {
        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !!!");
            Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            Console.WriteLine($"[From Client] {recvData}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred Bytes: {numOfBytes}");
        }
    }

    static void Main(string[] args)
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(host);
        IPAddress hostAddress = hostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(hostAddress, 7777);
        
        try
        {
            _listener.Init(endPoint, () => new GameSession());

            while (true)
            {
                // 프로그램 종료 방지
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed - {ex.Message}");
        }
    }
}