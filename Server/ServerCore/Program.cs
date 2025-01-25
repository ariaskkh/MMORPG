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
            Session session = new Session();
            session.Start(clientSocketProxy);
            
            byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server !!!");
            session.Send(sendBuff);

            Thread.Sleep(1000);

            session.Disconnect();
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