using ServerCore;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static Listener _listener = new Listener();

    static void OnAcceptHandler(Socket clientSocketProxy)
    {
        try
        {
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
        catch (Exception ex)
        {
            Console.WriteLine(ex);
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
            _listener.Init(endPoint, OnAcceptHandler);

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