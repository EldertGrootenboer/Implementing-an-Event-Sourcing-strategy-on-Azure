using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Common;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Database = Microsoft.Azure.Cosmos.Database;
using ServiceCollection = Demo.Common.ServiceCollection;

namespace ChangeFeedProcessor
{
    public static class ChangeFeedTrigger
    {
        private static readonly CosmosClient Client  = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBConnection"));
        private static readonly Database Database = Client.GetDatabase(Environment.GetEnvironmentVariable("CosmosDBDatabase"));
        private static readonly ServiceCollection Services = new ServiceCollection(Database);
        
        [FunctionName("ChangeFeedTrigger")]
        public static async Task RunAsync([CosmosDBTrigger(
                databaseName: "%CosmosDBDatabase%",
                collectionName: "%CosmosDBContainer%",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "orders-leases-container",
                CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);

                foreach (var document in input)
                {
                    var orderInformation = JsonConvert.DeserializeObject<Order>(document.ToString());

                    await Services.CustomerService.HandleCustomerView(orderInformation);
                    await Services.WarehouseService.HandleWarehouseView(orderInformation);
                }
            }
        }
    }
}