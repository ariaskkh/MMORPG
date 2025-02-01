using System.Net;
using ServerCore;

namespace Server;

class Program
{

    static Listener _listener = new Listener();

    static void Main(string[] args)
    {
        // DNS
        string host = Dns.GetHostName();
        IPHostEntry hostEntry = Dns.GetHostEntry(host);
        IPAddress hostAddress = hostEntry.AddressList[0];
        IPEndPoint endPoint = new IPEndPoint(hostAddress, 7777);

        try
        {
            _listener.Init(endPoint, () => new ClientSession());

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