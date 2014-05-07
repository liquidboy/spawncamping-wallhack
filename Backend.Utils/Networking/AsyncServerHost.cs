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
        private readonly IPEndPoint _endpoint;

        public AsyncServerHost(IPEndPoint endpoint)
        {
            _endpoint = endpoint;
        }

        public async Task Start(ITcpServerHandler handler, CancellationToken ct)
        {
            var task = this.StartFunctional((c, t) => handler.HandleRequest(c, t), ct);
            await task;
        }

        public async Task StartFunctional(Func<TcpClient, CancellationToken, Task> serverLogic, CancellationToken ct)
        {
            TcpListener listener = new TcpListener(_endpoint);
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
