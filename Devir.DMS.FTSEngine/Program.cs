using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Web;

namespace Devir.DMS.FulltextSearchEngine
{
    public class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            Uri baseAddress = new Uri("http://localhost:4784");
#else
            Uri baseAddress = new Uri("http://192.168.64.9:4784");
#endif
            //Для Тестов


            // Create the ServiceHost.
            using (ServiceHost host = new ServiceHost(typeof(ServiceFTS), baseAddress))
            {
                // Enable metadata publishing.
                ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
                smb.HttpGetEnabled = true;
                smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
                host.Description.Behaviors.Add(smb);

                // Open the ServiceHost to start listening for messages. Since
                // no endpoints are explicitly configured, the runtime will create
                // one endpoint per base address for each service contract implemented
                // by the service.
                host.Open();

                Console.WriteLine("Сервис полнотекстной индексации и поиска запущен на {0}", baseAddress);
                Console.WriteLine("Для выхода нажмите q и ENTER.");
                var resultKey = "";
                while (resultKey != "q")
                {
                    
                    resultKey = Console.ReadLine();
                }

                // Close the ServiceHost.
                host.Close();
                Environment.Exit(0);
            }
        }
    }
}