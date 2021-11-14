using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EPH.Functions
{
    public static class EventHubsProcessor
    {
        [FunctionName("EventHubsProcessor")]
        public static async Task Run(
            [EventHubTrigger("orders", Connection = "implementingeventsourcingstrategy")] EventData[] events,
            //[CosmosDB(databaseName: "warehouse", collectionName: "orders", ConnectionStringSetting = "CosmosDBConnection")]out dynamic document,
            ILogger log)
        {
            var exceptions = new List<Exception>();

            var client = new CosmosClient(
                Environment.GetEnvironmentVariable("CosmosDBConnection"),
                new CosmosClientOptions()
                {
                    SerializerOptions = new CosmosSerializationOptions()
                    {
                        IgnoreNullValues = true
                    }
                });

            var database = await client.CreateDatabaseIfNotExistsAsync(Environment.GetEnvironmentVariable("CosmosDBDatabase"));
            var container = await database.Database.CreateContainerIfNotExistsAsync(
                Environment.GetEnvironmentVariable("CosmosDBContainer"),
                Environment.GetEnvironmentVariable("CosmosDBPartitionKey"));

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");
                    var order = JsonConvert.DeserializeObject<Order>(
                        messageBody,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Ignore
                        });

                    await container.Container.UpsertItemAsync(order);
                    //document = order;
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.
            if (exceptions.Count > 1)
            {
                throw new AggregateException(exceptions);
            }

            if (exceptions.Count == 1)
            {
                throw exceptions.Single();
            }
        }
    }
}
