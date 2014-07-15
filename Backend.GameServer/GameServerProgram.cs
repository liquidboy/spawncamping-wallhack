namespace Backend.GameServer
{
    using Backend.GameLogic;
    using Backend.Utils.Networking;
    using Frontend.Library.Models;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class GameServerProgram
    {
        static void Main(string[] args)
        {
            Console.Title = "Backend.GameServer";

            if (args.Length != 4)
            {
                Console.Error.WriteLine("Provide ip address (such as 127.0.0.1), TCP port and secret key (base64) as command line args");
                return;
            }

            var argIpAddress = args[0];
            var argPortNumber = args[1];
            var argSecretKey = args[2];
            var argGameServerId = args[3];


            IPAddress address;
            if (!IPAddress.TryParse(argIpAddress, out address)) {
                Console.Error.WriteLine("\"{0}\" is not a valid IP address", args[0]);
                return;
            }
            int port;
            if (!int.TryParse(argPortNumber, out port))
            {
                Console.Error.WriteLine("\"{0}\" is not a valid TCP port", args[1]);
                return;
            }
            var ipEndPoint = new IPEndPoint(address, port);
            byte[] secretKey = Convert.FromBase64String(argSecretKey);
            var gameServerID = new GameServerID { ID = Guid.Parse(argGameServerId) };

            Console.WriteLine("Listen on {0}", ipEndPoint);

            var cts = new CancellationTokenSource();
            var server = new AsyncServerHost(ipEndPoint);

            var gameServerImpl = new GameServerImpl(gameServerID, secretKey);
            Task t = server.Start(gameServerImpl, cts.Token);

            Console.WriteLine("Launched game server process on {0}", ipEndPoint);
            t.Wait();
        }
    }
}