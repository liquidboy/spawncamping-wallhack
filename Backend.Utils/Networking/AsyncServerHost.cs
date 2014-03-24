namespace Backend.Utils.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class AsyncServerHost
    {
        private readonly IPAddress _ipAddress;
        private readonly int _port;

        public AsyncServerHost(IPAddress ipAddress, int port)
        {
            _ipAddress = ipAddress;
            _port = port;
        }

        public async Task Start(Func<TcpClient, CancellationToken, Task> serverLogic, CancellationToken ct)
        {
            TcpListener listener = new TcpListener(_ipAddress, _port);
            listener.Start(
                // backlog: 1000
                );

            // var tasks = new List<Task>();
            while (!ct.IsCancellationRequested)
            {
                var tcpClient = await listener.AcceptTcpClientAsync();
                var task = Task.Factory.StartNew(async (state) =>
                {
                    var client = (TcpClient)state;
                    try
                    {
                        await serverLogic(client, ct);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                    finally
                    {
                        client.Close();
                    }
                }, tcpClient).Unwrap();

                //tasks.Add(task);
                //tasks.Where(_ => _.isFinished()).ToList().ForEach(_ => tasks.Remove(_));
            }
        }
    }
}
