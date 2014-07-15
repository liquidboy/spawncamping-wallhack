namespace Backend.ProxyServer
{
    using Backend.Utils.Networking;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {


        static void Main(string[] args)
        {
            Console.Title = "Backend.ProxyServer";

            if (args.Length != 2)
            {
                Console.Error.WriteLine("Need ipaddress and port number for proxy.");
                return;
            }

            var argIpAddress = args[0];
            var argPortNumber = args[1];

            IPAddress defaultForwarderAddress = IPAddress.Parse(argIpAddress);
            int portNumber = int.Parse(argPortNumber);

            IPEndPoint proxyEndpoint = new IPEndPoint(defaultForwarderAddress, portNumber);

            CancellationTokenSource cts = new CancellationTokenSource();
            AsyncServerHost proxyServerHost = new AsyncServerHost(proxyEndpoint);
            Task proxyTask = proxyServerHost.StartFunctional(async (client, ct) => await ProxyConnection.ProxyLogic(client, cts, defaultForwarderAddress), cts.Token);
            Task.Factory.StartNew(async () =>
            {
                var ct = cts.Token;

                Trace.TraceInformation("Cloud.GameServerHost.WorkerRole entry point called", "Information");

                while (!ct.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));

                    Trace.TraceInformation("Working", "Information");
                }
            }).Unwrap().Wait();
        }
    }
}