namespace Backend.GameLogic.Scaling
{
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Management.Compute;
    using Microsoft.WindowsAzure.Management.Compute.Models;

    using Configuration;

    public class ScalingAgent
    {
        private ComputeManagementClient computeManagementClient;
        private string subscriptionID;
        private string subscriptionManagementCertificateThumbprint;
        private string servicename;

        public ScalingAgent(string subscriptionID, string subscriptionManagementCertificateThumbprint, string servicename)
        {
            this.subscriptionID = subscriptionID;
            this.subscriptionManagementCertificateThumbprint = subscriptionManagementCertificateThumbprint;
            this.servicename = servicename;
            this.computeManagementClient = GetComputeManagementClient();
        }

        public async Task ScaleAsync()
        {
            var detailed = await computeManagementClient.HostedServices.GetDetailedAsync(servicename);
            var deployment = detailed.Deployments.First(_ => _.DeploymentSlot == DeploymentSlot.Production);
            var doc = XDocument.Parse(deployment.Configuration);

            setInstanceCount(doc, Names.GameRole.Name, 5);

            var operationResponse = await computeManagementClient.Deployments.BeginChangingConfigurationBySlotAsync(
                    serviceName: detailed.ServiceName,
                    deploymentSlot: deployment.DeploymentSlot,
                    cancellationToken: CancellationToken.None,
                    parameters: new DeploymentChangeConfigurationParameters()
                    {
                        Configuration = doc.ToString()
                    });

            if (operationResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Trace.TraceError(string.Format(
                            "Problem scaling: HTTPStatus: {0} RequestID {1}",
                            operationResponse.StatusCode.ToString(),
                            operationResponse.RequestId));
            }

            Trace.TraceInformation(string.Format(
                        "Scaling done: HTTPStatus: {0} RequestID {1}",
                        operationResponse.StatusCode.ToString(),
                        operationResponse.RequestId));
        }

        private static XName n(string name)
        {
            return XName.Get(name,
                "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration");
        }

        private static int getInstanceCount(XDocument doc, string rolename)
        {
            var role = doc.Root.Elements(n("Role")).FirstOrDefault(_ => _.Attribute("name").Value == rolename);
            if (role == null) return -1;
            var v = role.Element(n("Instances")).Attribute("count").Value;
            return int.Parse(v);
        }

        private static void setInstanceCount(XDocument doc, string rolename, int newInstanceCount)
        {
            if (newInstanceCount < 1)
            {
                newInstanceCount = 1;
            }

            var role = doc.Root.Elements(n("Role")).FirstOrDefault(_ => _.Attribute("name").Value == rolename);
            role.Element(n("Instances")).Attribute("count").Value = newInstanceCount.ToString();
        }

        private static void changeInstanceCount(XDocument doc, string rolename, int deltaCount)
        {
            int oldCount = getInstanceCount(doc, rolename);
            var newCount = oldCount + deltaCount;
            setInstanceCount(doc, rolename, newCount);
        }

        private static X509Certificate2 FindX509Certificate(string managementCertThumbprint)
        {
            X509Store store = null;

            try
            {
                store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                X509Certificate2Collection certs = store.Certificates.Find(
                    findType: X509FindType.FindByThumbprint,
                    findValue: managementCertThumbprint,
                    validOnly: false);
                if (certs.Count == 0)
                {
                    return null;
                }

                return certs[0];
            }
            finally
            {
                if (store != null) store.Close();
            }
        }

        private X509Certificate2 GetManagementCert()
        {
            var managementCertThumbprint = this.subscriptionManagementCertificateThumbprint;
            X509Certificate2 managementCert = FindX509Certificate(managementCertThumbprint);
            return managementCert;
        }

        private ComputeManagementClient GetComputeManagementClient()
        {
            string subscriptionId = this.subscriptionID;
            X509Certificate2 managementCert = GetManagementCert();
            SubscriptionCloudCredentials creds = new CertificateCloudCredentials(subscriptionId, managementCert);
            ComputeManagementClient computeManagementClient = CloudContext.Clients.CreateComputeManagementClient(creds);
            return computeManagementClient;
        }
    }
}