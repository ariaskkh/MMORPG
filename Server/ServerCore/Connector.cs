using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

public class Connector
{
    Func<Session> _sessionFactory;
    public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _sessionFactory = sessionFactory;
        SocketAsyncEventArgs args = new();
        args.Completed += OnRegisterConnected;
        args.RemoteEndPoint = endPoint;
        args.UserToken = socket; // object타입이라 option 같은 느낌으로 사용

        RegisterConnect(args);
    }

    private void RegisterConnect(SocketAsyncEventArgs args)
    {
        Socket? socket = args.UserToken as Socket;
        if (socket == null)
            return;

        bool isPending = socket.ConnectAsync(args);
        if (isPending == false)
            OnRegisterConnected(null, args);
    }

    // Listener와 대칭적으로 만듦
    private void OnRegisterConnected(object? sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory.Invoke();
            session.Start(args.ConnectSocket);
            session.OnConnected(args.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
        }
    }
}
