# Implementing an Event Sourcing strategy on Azure

In recent years the Event Sourcing pattern has become increasingly popular. By storing a history of events it enables us to decouple the storage of data from the implementation of the logic around it. And we can rebuild the state of our data to any point in time, giving us a wide range of opportunities around auditing and compensation.

In this demo-heavy session you will learn how we can use Azure Event Hubs to process and store these events to build our own event store based on Cosmos DB. Moreover, we will also dive into options around connecting to other Azure services and even Kafka applications to easily implement this popular pattern in our own solutions.
