namespace Cloud.LobbyService.WorkerRole
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Orleans;
    using Orleans.Host.Azure.Client;

    using Backend.GrainImplementations;
    using Backend.GrainInterfaces;
    using Backend.GameLogic.Models;

    public static class OrleansSampleClient
    {
        public static Task LaunchOrleansClients()
        {
            return Task.Factory.StartNew(() =>
            {
                var infinitelyRunningTasks = new string[] 
                    { 
                        "00000000-acf8-4f39-9cf1-14a84bb82980",
                        "11000000-95f1-42e4-b69f-90eab2ac8ec1",
                        "22000000-e9f0-4a37-8d6b-24de340d6b68",
                        "33000000-7968-4d94-9859-4cd869319cb8",

                        "44000000-53f9-4a96-acb6-e96f5d8085cf",
                        "55000000-4880-43ee-95eb-3523d8888608",
                        "66000000-e614-455b-8d8d-25d899ee9acd",
                        "77000000-6fc4-47a6-ad5b-57df7c324848",
                        "88000000-3742-4846-97e3-82f2e506652d",
                        "99000000-7379-47fe-85ae-2b4e284ace05",
                        "aa000000-8be5-4701-b4e9-f4c608637493",
                        "bb000000-0117-4741-8e58-7bb81e403947",
                        "cc000000-c961-4dbc-9ba0-614a93aa1bc9",
                        "dd000000-c328-498f-a653-7aecccf3ba54",
                        "ee000000-feeb-4f56-ac73-783578198438",
                        "ff000000-d10c-4d79-b0f8-8ed67ac33ca7",
                        "1e159e6d-cc31-4028-ac23-64ee318f973f",
                        "789eeea4-ef13-4d81-9ceb-ca30bfae50cb",
                        "ba8ded1c-9831-4f44-a9b9-bfee869007d3",
                        "0d64a5d8-60c8-4ea2-87cc-376dac108cfc",
                        "b94b40c2-c3da-4106-99cf-d9fb81e0111b",
                        "7b13b66b-accc-434c-ad82-e5cd0eb01aee"
                    }
                        .Select(_ => new ClientID { ID = Guid.Parse(_) })
                        .Select(gamerId => Task.Factory.StartNew(async () =>
                        {
                            GameServerStartParams gameServerInfo = null;
                            var gamer = await Gamer.CreateAsync(gamerId, server =>
                            {
                                gameServerInfo = server;
                                Trace.WriteLine(string.Format("Player {0} joins server {1}", gamerId, server.GameServerID));
                            });

                            Trace.TraceInformation(string.Format("Start gamer ID {0}", gamerId));

                            while (true)
                            {
                                await Task.Delay(TimeSpan.FromSeconds(2));

                                if (gameServerInfo == null)
                                {
                                    Trace.TraceInformation(string.Format("Game for player {0} not yet started", gamer.Id));
                                }
                                else
                                {
                                    Trace.TraceInformation(string.Format("Game for player {0} started: gameServerGrain {1}",
                                        gamer.Id, gameServerInfo.GameServerID));
                                }
                            }
                        }).Unwrap())
                        .ToArray();

                try
                {
                    Task.WaitAll(infinitelyRunningTasks);
                }
                catch (Exception)
                {
                    foreach (var t in infinitelyRunningTasks)
                    {
                        if (t.Exception != null)
                        {
                            foreach (var ie in t.Exception.InnerExceptions)
                            {
                                for (var e = ie; e != null; e = e.InnerException)
                                {
                                    Trace.TraceError(e.Message);
                                    Trace.TraceError(string.Format(e.Message));
                                }
                            }
                        }
                    }
                }
            });
        }
    }
}