using System;
using System.Collections.Generic;

namespace ChangeFeedProcessor
{
    public class Order
    {
        public string id { get; }
        public DateTime eventTime { get; }
        public string orderNumber { get; set; }
        public string orderStatus { get; set; }
        public int hubId { get; set; }
        public Customer customer { get; set; }
        public Delivery delivery { get; set; }
        public List<Article> articles { get; set; }
    }

    public class Delivery
    {
        public string plannedDeliveryTime { get; set; }
        public string pickingStartTime { get; set; }
        public object pickingEndTime { get; set; }
        public string pickingStatus { get; set; }
    }

    public class Customer
    {
        public int customerId { get; set; }
        public DeliveryAddress deliveryAddress { get; set; }
    }

    public class DeliveryAddress
    {
        public string name { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string zipCode { get; set; }
    }

    public class Article
    {
        public string article { get; set; }
        public int quantity { get; set; }
        public double price { get; set; }
    }

    public class CustomerOverview : BaseOverview
    {
        public override string id => customerId;
        public string customerId { get; set; }
        public int count { get; set; }

        public List<OrdersDetails> orders { get; set; }
    }

    public class HubOverview : BaseOverview
    {
        public override string id => partitionKey;
        public string partitionKey { get; set; }
        public int hubId { get; set; }
        public int count { get; set; }
    }

    public class OrdersDetails
    {
        public string orderStatus { get; set; }
        public string orderNumber { get; set; }

        public List<Article> articles { get; set; }
    }

    public abstract class BaseOverview
    {
        public abstract string id { get; }
    }
}