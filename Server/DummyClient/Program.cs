using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        class GameSession : Session
        {
            public override void OnConnected(EndPoint endPoint)
            {
                Console.WriteLine($"OnConnected: {endPoint}");

                for (int i = 0; i < 5; i++)
                {
                    // 보낸다
                    byte[] sendBuffer = Encoding.UTF8.GetBytes($"Hello World! I'm client: {i}");
                    Send(sendBuffer);
                }
            }

            public override void OnDisconnected(EndPoint endPoint)
            {
                Console.WriteLine($"OnDisconnected: {endPoint}");
            }

            public override void OnRecv(ArraySegment<byte> buffer)
            {
                string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
                Console.WriteLine($"[From Server] {recvData}");
            }

            public override void OnSend(int numOfBytes)
            {
                Console.WriteLine($"Transferred Bytes: {numOfBytes}");
            }
        }

        static void Main(string[] args)
        {
            // DNS
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            IPAddress hostAddress = hostEntry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(hostAddress, 7777);

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                Thread.Sleep(100);
            }
        }
    }
}