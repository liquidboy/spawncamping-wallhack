namespace ManagementClientSample
{
    using Backend.GameLogic.Scaling;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            var agent = new ScalingAgent(
                subscriptionID: "5cc34a74-cb29-467a-93d4-1ddfb450b971",
                subscriptionManagementCertificateThumbprint: "A5596EA671E1B508AF358A040A400F88A5E4BF0F");

            await agent.ScaleAsync();
        }
    }
}
