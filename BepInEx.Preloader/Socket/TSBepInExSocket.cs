using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BepInEx.Preloader.Socket
{
    public class TSBepInExSocket
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _receiveThread;
        private volatile bool _isRunning;
        public bool isConnect { get; private set; }
        private string _serverIpAddress;
        private int _serverPort;

        public Queue<Package> ReceivedPackages { get; private set; }

        public TSBepInExSocket(string serverIpAddress, int serverPort)
        {
            _serverIpAddress = serverIpAddress;
            _serverPort = serverPort;
            ReceivedPackages = new Queue<Package>();
        }
        public void Connect()
        {
            _client = new TcpClient();
            _client.Connect(_serverIpAddress, _serverPort);
            _stream = _client.GetStream();
            isConnect = true;
            _isRunning = true;
            _receiveThread = new Thread(ReceiveThread);
            _receiveThread.Start();
        }
        public void SendPackage(byte key, string context)
        {
            SendPackage(key, Encoding.UTF8.GetBytes(context));
        }
        public void SendPackage(byte key, byte[] body)
        {
            byte[] bodyLengthBytes = BitConverter.GetBytes((ushort)body.Length);

            byte[] packageBytes = new byte[1 + 2 + body.Length];
            packageBytes[0] = key;
            Buffer.BlockCopy(bodyLengthBytes, 0, packageBytes, 1, 2);
            Buffer.BlockCopy(body, 0, packageBytes, 3, body.Length);

            _stream.Write(packageBytes, 0, packageBytes.Length);
        }

        // 在单独的线程中处理接收数据包的逻辑
        private void ReceiveThread()
        {
            try
            {
                // 定义一个缓冲区，用于存储接收到的数据
                byte[] buffer = new byte[4096];
                int bufferOffset = 0;

                while (_isRunning)
                {
                    int bytesRead = _stream.Read(buffer, bufferOffset, buffer.Length - bufferOffset);
                    if (bytesRead == 0)
                    {
                        // 客户端断开连接
                        break;
                    }

                    bufferOffset += bytesRead;

                    // 检查是否收到至少一个完整的数据包
                    while (bufferOffset >= 3)
                    {
                        if (!_isRunning) break;
                        ushort bodyLength = BitConverter.ToUInt16(buffer, 1);

                        // 检查缓冲区中是否有足够的数据组成完整的数据包
                        if (bufferOffset >= 3 + bodyLength)
                        {
                            byte key = buffer[0];
                            byte[] body = new byte[bodyLength];
                            Buffer.BlockCopy(buffer, 3, body, 0, bodyLength);

                            Package receivedPackage = new Package(key, body);

                            // 将接收到的数据包加入队列
                            ReceivedPackages.Enqueue(receivedPackage);

                            // 移动缓冲区中剩余的数据
                            Buffer.BlockCopy(buffer, 3 + bodyLength, buffer, 0, bufferOffset - (3 + bodyLength));
                            bufferOffset -= (3 + bodyLength);
                        }
                        else
                        {
                            // 缓冲区中的数据不足以组成一个完整的数据包，跳出循环，继续接收更多数据
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Debug.Error(string.Format("An error occurred while receiving packages: {0}", ex.Message));
            }
            Clearup();
        }

        void Clearup()
        {
            isConnect = false;
            _isRunning = false;
            _stream.Close();
            _client.Close();
        }

        public void Disconnect()
        {
            try
            {
                Clearup();
                _receiveThread.Abort();
            }catch(Exception) { }
        }
    }
}
