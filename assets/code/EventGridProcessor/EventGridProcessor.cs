// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using Azure.Messaging.EventGrid;
using Microsoft.Azure.Cosmos;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace EPH.Functions
{
    public static class EventGridProcessor
    {
        private static CosmosClient client = new CosmosClient(
             Environment.GetEnvironmentVariable("CosmosDBConnection"),
             new CosmosClientOptions()
             {
                 SerializerOptions = new CosmosSerializationOptions()
                 {
                     IgnoreNullValues = true
                 }
             });

        [FunctionName("EventGridProcessor")]
        public static void Run([EventGridTrigger] EventGridEvent eventGridEvent,
        [ServiceBus("orders", Connection = "ServiceBusConnection")] out ServiceBusMessage message,
        ILogger log)
        {
            log.LogInformation(eventGridEvent.ToString());

            // Connect to Cosmos DB
            var database = client.GetDatabase(Environment.GetEnvironmentVariable("CosmosDBDatabase"));
            var container = database.GetContainer(Environment.GetEnvironmentVariable("CosmosDBContainer"));

            // Retrieve order from Cosmos DB
            var sqlQueryText = $"SELECT * FROM c WHERE c.orderNumber = {eventGridEvent.Data.ToString()} AND c.orderStatus = 'REQUESTED'";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            using FeedIterator<Order> queryResultSetIterator = container.GetItemQueryIterator<Order>(queryDefinition);
            message = new ServiceBusMessage("test");

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Order> currentResultSet = queryResultSetIterator.ReadNextAsync().GetAwaiter().GetResult();
                foreach (Order orderUpdate in currentResultSet)
                {
                    // Send message to Service Bus
                    orderUpdate.orderStatus = "DELIVERED";
                    message = new ServiceBusMessage(JsonConvert.SerializeObject(orderUpdate));
                }
            }
        }
    }
}
