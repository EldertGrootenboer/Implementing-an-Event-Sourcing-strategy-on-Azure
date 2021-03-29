using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;
using System.Net.Http;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;

namespace EventHubKafkaSample
{
    class Worker
    {
        public static async Task Producer(string brokerlist, string password, string topicname, string cacertlocation)
        {
            try
            {
                var config = new Dictionary<string, object> {
                    { "bootstrap.servers", brokerlist },
                    { "security.protocol","SASL_SSL" },
                    { "sasl.mechanism","PLAIN" },
                    { "sasl.username", "$ConnectionString"},
                    { "sasl.password", password },
                    { "ssl.ca.location",cacertlocation },

                    { "debug", "security,broker,protocol" }
                };

                using (var producer = new Producer<Null, string>(config, null, new StringSerializer(Encoding.UTF8)))
                {
                    Console.WriteLine("Initiating Execution");
                    for (int x = 0; x < 10; x++)
                    {
                        var msg = string.Format("This is a sample message - msg # {0} at {1}", x, DateTime.Now.ToString("yyyMMdd_HHmmSSfff"));
                        var deliveryReport = await producer.ProduceAsync(topicname, null, msg);
                        Console.WriteLine(string.Format("Message {0} sent.", x));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Exception Ocurred - {0}", e.Message));
            }
        }

        public static void Consumer(string brokerlist, string password, string consumergroup, string topicname, string cacertlocation)
        {
            

            var config = new Dictionary<string, object> {
                    { "bootstrap.servers", brokerlist },
                    { "security.protocol","SASL_SSL" },
                    { "sasl.mechanism","PLAIN" },
                    { "sasl.username", "$ConnectionString"},
                    { "sasl.password", password },
                    { "ssl.ca.location",cacertlocation },
                    { "debug", "security,broker,protocol" },
                    { "group.id", consumergroup },
                    { "auto.commit.interval.ms", 5000 },
                    { "auto.offset.reset", "earliest" },
                    { "broker.version.fallback","0.10.0.0" },
                    { "api.version.fallback.ms","0" }
                };

            using (var consumer = new Consumer<Null, string>(config, null, new StringDeserializer(Encoding.UTF8)))
            {
                consumer.OnMessage += (_, msg)
                  => Console.WriteLine($"Read '{msg.Value}' from: {msg.TopicPartitionOffset}");

                consumer.OnError += (_, error)
                  => Console.WriteLine($"Error: {error}");

                consumer.OnConsumeError += (_, msg)
                  => Console.WriteLine($"Consume error ({msg.TopicPartitionOffset}): {msg.Error}");

                consumer.Subscribe(topicname);

                while (true)
                {
                    consumer.Poll(TimeSpan.FromMilliseconds(100));
                }
            }

        }
    }
}
