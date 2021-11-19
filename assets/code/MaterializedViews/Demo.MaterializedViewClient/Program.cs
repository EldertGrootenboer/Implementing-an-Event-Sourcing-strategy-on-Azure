using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Demo.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Demo.MaterializedViewClient
{
    static class Program
    {
        private static readonly IConfigurationRoot Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        private static readonly string Connection = Configuration["CosmosDBConnection"];

        private static readonly CosmosClient Client = new(Connection, new CosmosClientOptions { AllowBulkExecution = true });
        private static readonly Database Database = Client.GetDatabase("warehouse");
        private static readonly ServiceCollection Services = new(Database);

        public static async Task Main(string[] args)
        {
            var leaseContainerProperties = new ContainerProperties("orders-leases-container", "/id");
            var leaseContainer = await Database
                .CreateContainerIfNotExistsAsync(leaseContainerProperties, throughput: 400);
            
            var processor = Database
                .GetContainer("orders")
                .GetChangeFeedProcessorBuilder<Order>("MovementProcessor", HandleMaterializedViews)
                .WithInstanceName("ChangeFeedOrdersProjections")
                .WithLeaseContainer(leaseContainer)
                .Build();
            
            Console.WriteLine("Starting Change Feed Processor...");
            await processor.StartAsync();
            Console.WriteLine("Change Feed Processor started.");

            Console.WriteLine("Press any key to stop the processor...");
            Console.ReadKey();

            Console.WriteLine("Stopping Change Feed Processor");

            await processor.StopAsync();
        }

        private static async Task HandleMaterializedViews(IReadOnlyCollection<Order> changes, CancellationToken cancellationToken)
        {
            Console.WriteLine(changes.Count + " Change(s) Received");

            if (changes.Count > 0)
            {
                Console.WriteLine("Documents modified " + changes.Count);

                foreach (var document in changes)
                {
                    var orderInformation = JsonConvert.DeserializeObject<Order>(document.ToString());

                    await Services.CustomerService.HandleCustomerView(orderInformation);
                    await Services.WarehouseService.HandleWarehouseView(orderInformation);
                }
            }
        }
    }
}