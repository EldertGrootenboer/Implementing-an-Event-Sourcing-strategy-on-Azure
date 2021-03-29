using System.Collections.Generic;

public class Order
{
    public string orderNumber { get; set; }
    public string orderStatus { get; set; }
    public int hubId { get; set; }
    public Customer customer { get; set; }
    public Delivery delivery { get; set; }
    public List<Article> articles { get; set; }
}