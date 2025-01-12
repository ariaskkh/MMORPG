using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DummyClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // DNS
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            IPAddress hostAddress = hostEntry.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(hostAddress, 7777);

            // 휴대폰 설정 (대리인의 휴대폰)
            Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                // 문지기 입장 문의 - 연결
                socket.Connect(endPoint);
                Console.WriteLine($"[Client] Connected to {socket.RemoteEndPoint.ToString()}");

                // 보낸다
                byte[] sendBuffer = Encoding.UTF8.GetBytes("Hello World! I'm client");
                socket.Send(sendBuffer);

                // 받는다
                byte[] recvBuffer = new byte[1024];
                int recvBytes = socket.Receive(recvBuffer);
                Console.WriteLine($"[Client] received - {Encoding.UTF8.GetString(recvBuffer, 0, recvBytes)}");

                // 종료
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}