using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(host);
        IPAddress hostAddress = hostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(hostAddress, 7777);

        // 문지기 생성
        Socket listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        
        try
        {
            // 문지기 교육
            listenSocket.Bind(endPoint);

            // 영업 시작
            // backlog: 최대 대기수
            listenSocket.Listen(10);

            while (true)
            {
                Console.WriteLine("Listening...");

                // 손님 입장
                Socket clientSocketProxy = listenSocket.Accept();

                // 받기
                byte[] recvBuffer = new byte[1024];
                int recvBytes = clientSocketProxy.Receive(recvBuffer);
                Console.WriteLine($"Received - {Encoding.UTF8.GetString(recvBuffer, 0, recvBytes)}");

                // 보내기
                clientSocketProxy.Send(Encoding.UTF8.GetBytes("Hello MMORPG man !!!"));

                // 쫓아낸다. 보낸다
                clientSocketProxy.Shutdown(SocketShutdown.Both);
                clientSocketProxy.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed - {ex.Message}");
        }
    }
}