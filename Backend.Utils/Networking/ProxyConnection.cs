namespace Backend.Utils.Networking
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensions;

    public class ProxyConnection
    {
        public static async Task ProxyLogic(TcpClient serverSocket, CancellationTokenSource cts, IPAddress defaultForwarderAddress)
        {
            var proxyConnection = new ProxyConnection(serverSocket, cts, defaultForwarderAddress);
         
            await proxyConnection.BridgeConnectionAsync();
        }

        public static async Task UseProxy(
            TcpClient client, CancellationToken ct,
            int serverPort, 
            IPAddress serverIP,
            Func<TcpClient, CancellationToken, Task> innerImplementation)
        {
            // if (serverPort > (1 << 15) || serverPort < 1) throw new ArgumentException("Illegal port"); 

            await client.Client.WriteAsync(serverPort);
            // await SendIPAddress(client, serverIP);

            await innerImplementation(client, ct);
        }

        const int BUFSIZE = 0x2000;

        public readonly CancellationTokenSource CancellationTokenSource;
        public readonly TcpClient ClientSocket;
        public readonly IPAddress DefaultForwarderAddress;

        public ProxyConnection(TcpClient clientSocket, CancellationTokenSource cts, IPAddress defaultForwarderAddress)
        {
            if (clientSocket == null) throw new ArgumentNullException("clientSocket");
            if (cts == null) throw new ArgumentNullException("cts");

            this.ClientSocket = clientSocket;
            this.CancellationTokenSource = cts;
            this.DefaultForwarderAddress = defaultForwarderAddress;
        }

        //private static async Task SendIPAddress(TcpClient client, IPAddress serverIP)
        //{
        //    var ipBytes = serverIP.GetAddressBytes();
        //    await client.Client.WriteAsync((byte)ipBytes.Length);
        //    await client.Client.WriteAsync(ipBytes, 0, ipBytes.Length);
        //}

        //private static async Task<IPAddress> ReadIPAddress(TcpClient client)
        //{
        //    var ipAddressLength = await client.Client.ReadByteAsync();
        //    var ipAddressBytes = new byte[ipAddressLength];
        //    var ipAddressBytesRead = await client.Client.ReadAsync(ipAddressBytes, 0, ipAddressBytes.Length);
        //    if (ipAddressBytesRead != ipAddressLength) { throw new ProtocolViolationException("Could not read IPAddress"); }
        //    var ipAddress = new IPAddress(ipAddressBytes);
        //    return ipAddress;
        //}

        public async Task BridgeConnectionAsync()
        {
            var port = await this.ClientSocket.Client.ReadInt32Async();
            // var ipAddress = await ReadIPAddress(this.ClientSocket);

            var ipAddress = this.DefaultForwarderAddress;
            var ServerSocket = new TcpClient();
            await ServerSocket.ConnectAsync(ipAddress, port);

            Task clientToServer = TransferSocketData(
                source: ClientSocket.Client, 
                dest: ServerSocket.Client, 
                dir: Direction.ClientToServer);
            
            Task serverToClient = TransferSocketData(
                source: ServerSocket.Client, 
                dest: ClientSocket.Client,
                dir: Direction.ServerToClient);

            await Task.WhenAny(clientToServer, serverToClient);

            // ... close everything
            // this.ClientSocket.Close();
            ServerSocket.Close();
        }

        private async Task TransferSocketData(Socket source, Socket dest, Direction dir)
        {
            byte[] buf = new byte[BUFSIZE];
            while (!this.CancellationTokenSource.Token.IsCancellationRequested)
            {
                int receiveLen = await source.ReadAsync(buf, 0, buf.Length);
                if (receiveLen == 0)
                {
                    break;
                }

                int sendLen = await dest.WriteAsync(buf, 0, receiveLen);
                if (sendLen != receiveLen)
                {
                    throw new NotSupportedException("sendLen != receiveLen");
                } 
                else if (sendLen == 0)
                {
                    throw new NotSupportedException("sendLen == 0");
                }
            }
        }

        //private async Task TransferSocketData_ImplementationWithSocketAwaitable(Socket source, Socket dest, Direction dir)
        //{
        //    var awaitable = SocketAwaitable.NewInstance(BUFSIZE);
        //    while (!this.CancellationTokenSource.Token.IsCancellationRequested)
        //    {
        //        awaitable.m_eventArgs.SetBuffer(0, BUFSIZE); // Allow whole internal buffer to be used for receiving
        //        await source.ReceiveAsync(awaitable);
        //        var receivedBytesCount = awaitable.m_eventArgs.BytesTransferred;
        //        if (receivedBytesCount == 0)
        //        {
        //            throw new NotSupportedException("sendLen == 0");
        //        }

        //        awaitable.m_eventArgs.SetBuffer(0, receivedBytesCount); // Only send the previously received bytes
        //        await dest.SendAsync(awaitable);
        //        var sentBytesCount = awaitable.m_eventArgs.BytesTransferred;
        //        if (receivedBytesCount != sentBytesCount)
        //        {
        //            throw new NotSupportedException("sendLen != receiveLen");
        //        }
        //    }
        //}
    }

    internal enum Direction
    {
        ClientToServer,
        ServerToClient
    }
}
