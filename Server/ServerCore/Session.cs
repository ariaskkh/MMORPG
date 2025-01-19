using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    class Session
    {
        Socket _socket;

        public void Init(Socket socket)
        {
            _socket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);
            

            RegisterRecv(recvArgs);
        }

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool isPending = _socket.ReceiveAsync(args);
            if (isPending)
            {
                OnRecvCompleted(null, args);
            }
        }

        void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                try
                {
                    var recvBuffer = args.Buffer;
                    // TODO
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);
                    Console.WriteLine($"[From Client] {recvData}");

                    RegisterRecv(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"onRecvCompleted Failed {ex}");
                }
                
            }
            else
            {
                // TODO Disconnect
            }
        }
    }
}
