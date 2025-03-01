﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore;

public class Listener
{
    Socket _listenSocket;
    Func<Session> _sessionFactory;
        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
    {
        _sessionFactory += sessionFactory;
        // 문지기 생성
        _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        // 문지기 교육
        _listenSocket.Bind(endPoint);

        // 영업 시작
        // backlog: 최대 대기수
        _listenSocket.Listen(10);

        SocketAsyncEventArgs args = new SocketAsyncEventArgs();
        args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
        RegisterAccept(args);
    }

    void RegisterAccept(SocketAsyncEventArgs args)
    {
        args.AcceptSocket = null;

            bool isPending = _listenSocket.AcceptAsync(args);
            if (isPending == false)
        {
            OnAcceptCompleted(null, args);
        }
    }

    void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
    {
        if (args.SocketError == SocketError.Success)
        {
            Session session = _sessionFactory.Invoke();
            session.Start(args.AcceptSocket);
            session.OnConnected(args.AcceptSocket.RemoteEndPoint);
        }
        else
        {
            Console.WriteLine(args.SocketError.ToString());
        }
        
        // 다 끝난 상태에서 다음을 위해 또 등록
        RegisterAccept(args);
    }
}
