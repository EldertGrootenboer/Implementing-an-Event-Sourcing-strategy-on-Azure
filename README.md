# Implementing an Event Sourcing strategy on Azure

In recent years the Event Sourcing pattern has become increasingly popular. By storing a history of events it enables us to decouple the storage of data from the implementation of the logic around it. And we can rebuild the state of our data to any point in time, giving us a wide range of opportunities around auditing and compensation.

In this demo-heavy session you will learn how we can use Azure Event Hubs to process and store these events to build our own event store based on Cosmos DB. Moreover, we will also dive into options around connecting to other Azure services and even Kafka applications to easily implement this popular pattern in our own solutions.

## Orders

In the demo we will show how we can use order events to fill our data store. Below is an example of such an event.

### Order event example

```json
{
    "orderNumber": "1b8211e0-f1b6-4261-8487-58a065ab1613",
    "orderStatus": "ACCEPTED",
    "hubId": 258,
    "customer": {
        "customerId": 321,
        "deliveryAddress": {
            "name": "John Smith",
            "street": "Pariser Platz 1",
            "city": "Berlin",
            "zipCode": "10117"
        }
    },
    "delivery": {
        "plannedDeliveryTime": "13-04-2021 19:00",
        "pickingStartTime": "13-04-2021 17:00",
        "pickingEndTime": null,
        "pickingStatus": "INPROGRESS"
    },
    "articles": [
        {
            "articleId": 369852,
            "quantity": 5,
            "price": 19.95
        },
        {
            "articleId": 145875,
            "quantity": 1,
            "price": 99.95
        }
    ]
}
```
