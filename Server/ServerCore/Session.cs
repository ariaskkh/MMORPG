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
        int _disconnected = 0;
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        Queue<byte[]> _sendQueue = new Queue<byte[]>();
        bool _sendProcessing = false;
        object _lockObj = new object();

        public void Start(Socket socket)
        {
            _socket = socket;

            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
            recvArgs.SetBuffer(new byte[1024], 0, 1024);
            
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            RegisterRecv(recvArgs);

        }

        public void Send(byte[] sendBuff)
        {
            lock (_lockObj)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_sendProcessing == false)
                {
                    RegisterSend();
                }
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)
                return;

            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        void RegisterSend()
        {
            _sendProcessing = true;
            byte[] buff = _sendQueue.Dequeue();
            _sendArgs.SetBuffer(buff, 0, buff.Length);

            var isPending = _socket.SendAsync(_sendArgs);
            if (isPending == false)
            {
                OnSendCompleted(null, _sendArgs);
            }
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            lock (_lockObj)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
                {
                    try
                    {
                        if (_sendQueue.Count > 0)
                        {
                            RegisterSend();
                        }
                        else
                        {
                            _sendProcessing = false;
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"OnSendCompleted failed: {ex}");
                    }
                }
                else
                {
                    Disconnect();
                }
            }
        }

        #region 네트워크 통신
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool isPending = _socket.ReceiveAsync(args);
            if (isPending == false)
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
                Disconnect();
            }
        }
    }
    #endregion
}
