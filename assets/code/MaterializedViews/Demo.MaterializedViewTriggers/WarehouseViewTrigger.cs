using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Document = Microsoft.Azure.Documents.Document;

namespace Demo.CustomerViewTrigger
{
    public static class WarehouseViewTrigger
    {
        private static readonly string DatabaseName = Environment.GetEnvironmentVariable("CosmosDBDatabase");
        
        [FunctionName("WarehouseViewTrigger")]
        public static async Task RunAsync([CosmosDBTrigger(
                "%CosmosDBDatabase%","%CosmosDBContainer%",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "orders-leases-container",
                LeaseCollectionPrefix = "warehouse")]
            IReadOnlyList<Document> input,
            [CosmosDB("%CosmosDBDatabase%", "%CosmosDBContainer%",
                ConnectionStringSetting = "CosmosDBConnection")]
            CosmosClient client, ILogger log)
        {
            var customerService = new WarehouseService(new WarehouseRepository(client.GetDatabase(DatabaseName)));

            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);

                foreach (var document in input)
                {
                    var orderInformation = JsonConvert.DeserializeObject<Order>(document.ToString());

                    await customerService.HandleWarehouseView(orderInformation);
                }
            }
        }
    }
}