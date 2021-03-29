using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;

namespace EventHubKafkaSample
{
    public class Worker
    {
        private static Producer<Null, string> _producer;
        private static Dictionary<string, object> _config;
        private static string _topicName;

        public static async Task Producer(string brokerlist, string password, string topicname, string cacertlocation)
        {
            try
            {
                _config = new Dictionary<string, object> {
                    { "bootstrap.servers", brokerlist },
                    { "security.protocol","SASL_SSL" },
                    { "sasl.mechanism","PLAIN" },
                    { "sasl.username", "$ConnectionString"},
                    { "sasl.password", password },
                    { "ssl.ca.location",cacertlocation },
                    { "debug", "security,broker,protocol" }
                };

                _producer = new Producer<Null, string>(_config, null, new StringSerializer(Encoding.UTF8));
                _topicName = topicname;

                var tasks = new List<Task>();
                for (int i = 0; i < 100; i++)
                {
                    tasks.Add(ProcessOrder());
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception exception)
            {
                Console.WriteLine(string.Format($"Exception occurred - {exception.Message}"));
            }
        }

        private static async Task ProcessOrder()
        {
            var orderNumber = Guid.NewGuid().ToString();
            var hubId = new Random().Next(10000, 10100);
            var updateArticles = new Random().Next(0, 10) > 7;
            var orderCancelled = new Random().Next(0, 10) > 9;
            var lookupIndex = new Random().Next(0, 47);

            #region Order requested

            // Create order
            var order = new Order
            {
                orderNumber = orderNumber,
                orderStatus = "REQUESTED",
                hubId = hubId,
                customer = new Customer
                {
                    customerId = new Random().Next(50000, 80000),
                    deliveryAddress = new DeliveryAddress
                    {
                        name = Lookups.names[lookupIndex],
                        street = Lookups.streets[lookupIndex],
                        city = "Berlin",
                        zipCode = Lookups.zipCodes[lookupIndex],
                    }
                },
                articles = new List<Article>()
            };

            // Add articles
            for (int i = 0; i < new Random().Next(1, 6); i++)
            {
                var articleIndex = new Random().Next(0, 42);
                order.articles.Add(new Article
                {
                    article = Lookups.articles[articleIndex],
                    quantity = new Random().Next(1, 10),
                    price = Lookups.prices[articleIndex],
                });
            }

            await SubmitAndWait(order);

            #endregion

            #region Order accepted

            order = new Order
            {
                orderNumber = orderNumber,
                hubId = hubId,
                orderStatus = "ACCEPTED"
            };

            await SubmitAndWait(order);

            #endregion

            #region Articles updated

            if (updateArticles)
            {
                order = new Order
                {
                    orderNumber = orderNumber,
                    hubId = hubId
                };

                // Add articles
                for (int i = 0; i < new Random().Next(1, 3); i++)
                {
                    var articleIndex = new Random().Next(0, 42);
                    order.articles.Add(new Article
                    {
                        article = Lookups.articles[articleIndex],
                        quantity = new Random().Next(1, 10),
                        price = Lookups.prices[articleIndex],
                    });
                }

                await SubmitAndWait(order);
            }

            #endregion

            #region Order cancelled

            if (orderCancelled)
            {
                order = new Order
                {
                    orderNumber = orderNumber,
                    hubId = hubId,
                    orderStatus = "CANCELLED"
                };

                await SubmitAndWait(order);
                return;
            }

            #endregion

            #region Order is being picked

            order = new Order
            {
                orderNumber = orderNumber,
                hubId = hubId,
                orderStatus = "PICKING",
                delivery = new Delivery
                {
                    pickingStartTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm")
                }
            };

            await SubmitAndWait(order);

            #endregion

            #region Order out for delivery

            order = new Order
            {
                orderNumber = orderNumber,
                hubId = hubId,
                orderStatus = "DELIVERING",
                delivery = new Delivery
                {
                    pickingEndTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 5)).ToString("dd-MM-yyyy HH:mm"),
                    plannedDeliveryTime = DateTime.Now.Add(new TimeSpan(0, 5, 0)).ToString("dd-MM-yyyy HH:mm")
                }
            };

            await SubmitAndWait(order);

            #endregion

            #region Order delivered

            order = new Order
            {
                orderNumber = orderNumber,
                hubId = hubId,
                orderStatus = "DELIVERED"
            };

            await SubmitAndWait(order);

            #endregion
        }

        private static async Task SubmitAndWait(Order order)
        {
            var message = JsonConvert.SerializeObject(order,
                             Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            await _producer.ProduceAsync(_topicName, null, message);
            Console.WriteLine(string.Format($"Order {order.orderNumber} sent."));
            Thread.Sleep(new Random().Next(1000, 60000));
        }
    }
}
