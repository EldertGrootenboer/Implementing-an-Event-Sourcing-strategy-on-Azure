using System;
using System.Collections.Generic;

public class Order
{
    public Guid? id { get; set; }
    public DateTime? eventTime { get; set; }
    public string orderNumber { get; set; }
    public string orderStatus { get; set; }
    public int hubId { get; set; }
    public Customer customer { get; set; }
    public Delivery delivery { get; set; }
    public List<Article> articles { get; set; }

    public Order()
    {
        id = Guid.NewGuid();
        eventTime = DateTime.Now;
    }
}