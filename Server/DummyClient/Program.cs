using ServerCore;
using System.Net;

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

            Connector connector = new Connector();
            connector.Connect(endPoint, () => { return new ServerSession(); });

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