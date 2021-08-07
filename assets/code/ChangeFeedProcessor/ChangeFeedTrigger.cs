using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Database = Microsoft.Azure.Cosmos.Database;

namespace ChangeFeedProcessor
{
    public static class ChangeFeedTrigger
    {
        private static readonly CosmosClient Client  = new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBConnection"));
        private static readonly Database Database = Client.GetDatabase(Environment.GetEnvironmentVariable("CosmosDBDatabase"));

        [FunctionName("ChangeFeedTrigger")]
        public static async Task RunAsync([CosmosDBTrigger(
                databaseName: "%CosmosDBDatabase%",
                collectionName: "%CosmosDBContainer%",
                ConnectionStringSetting = "CosmosDBConnection",
                LeaseCollectionName = "orders-leases-container",
                CreateLeaseCollectionIfNotExists = true)]
            IReadOnlyList<Document> input, ILogger log)
        {
            var customerOverviewRepository = new CustomerOverviewRepository(Database);
            var hubOverviewRepository = new HubOverviewRepository(Database);

            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);

                foreach (var document in input)
                {
                    var orderInformation = JsonConvert.DeserializeObject<Order>(document.ToString());

                    await MaterializeCustomersOverview(customerOverviewRepository, orderInformation);
                    await MaterializeHubsOverview(hubOverviewRepository, orderInformation);
                }
            }
        }

        private static async Task MaterializeCustomersOverview(CustomerOverviewRepository customerViewRepository, Order order)
        {
            string customerId = order.customer.customerId.ToString();

            var overview = await customerViewRepository.GetDocument(customerId);

            if (overview is null)
            {
                await CreateCustomerOverview(customerViewRepository, order, customerId);
            }
            else
            {
                await UpdateCustomerOverview(customerViewRepository, order, overview);
            }
        }

        private static async Task MaterializeHubsOverview(HubOverviewRepository hubOverviewRepository, Order order)
        {
            if (order.orderStatus != "REQUESTED")
            {
                return;
            }

            var partitionKey = $"{order.hubId}_{DateTime.Today:MM.dd.yyyy}";

            var overview = await hubOverviewRepository.GetDocument(partitionKey);

            if (overview is null)
            {
                await CreateHubOverview(hubOverviewRepository, order, partitionKey);
            }
            else
            {
                await UpdateHubOverview(hubOverviewRepository, order, overview);
            }
        }

        private static async Task UpdateHubOverview(HubOverviewRepository hubOverviewRepository, Order order,
            HubOverview overview)
        {
            if (overview.orderNumbers.Contains(order.orderNumber))
            {
                return;
            }

            ++overview.count;
            overview.orderNumbers.Add(order.orderNumber);

            await hubOverviewRepository.UpdateDocument(overview);
        }

        private static async Task CreateHubOverview(HubOverviewRepository hubOverviewRepository, Order order,
            string partitionKey)
        {
            HubOverview overview;
            overview = new HubOverview
            {
                hubId = order.hubId,
                count = 1,
                partitionKey = partitionKey,
                orderNumbers = new List<string> {order.orderNumber}
            };

            await hubOverviewRepository.CreateDocument(overview);
        }

        private static async Task UpdateCustomerOverview(CustomerOverviewRepository customerViewRepository, Order order,
            CustomerOverview overview)
        {
            var existingOrder = overview.orders.FirstOrDefault(x => x.orderNumber == order.orderNumber);

            if (existingOrder is null)
            {
                ++overview.count;
                overview.orders.Add(new OrdersDetails
                {
                    articles = order.articles,
                    orderNumber = order.orderNumber,
                    orderStatus = order.orderStatus
                });
            }
            else
            {
                existingOrder.orderStatus = order.orderStatus;

                if (order.articles != null)
                {
                    existingOrder.articles = order.articles;
                }
            }

            await customerViewRepository.UpdateDocument(overview);
        }

        private static async Task CreateCustomerOverview(CustomerOverviewRepository customerViewRepository, Order order,
            string customerId)
        {
            CustomerOverview overview;
            overview = new CustomerOverview
            {
                count = 1,
                customerId = customerId,
                orders = new List<OrdersDetails>
                {
                    new OrdersDetails
                    {
                        articles = order.articles,
                        orderNumber = order.orderNumber,
                        orderStatus = order.orderStatus
                    }
                }
            };

            await customerViewRepository.CreateDocument(overview);
        }
    }
}