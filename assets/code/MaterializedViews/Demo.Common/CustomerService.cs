using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Demo.Common
{
    public class CustomerService
    {
        private readonly CustomerRepository _customerViewRepository;
        
        public CustomerService(CustomerRepository customerViewRepository)
        {
            _customerViewRepository = customerViewRepository;
        }
        
        public async Task HandleCustomerView(Order order)
        {
            string customerId = order.customer.customerId.ToString();

            var overview = await _customerViewRepository.GetDocument(customerId);

            if (overview is null)
            {
                await CreateCustomerOverview(order, customerId);
            }
            else
            {
                await UpdateCustomerOverview(order, overview);
            }
        }
        
        private async Task UpdateCustomerOverview(Order order, CustomerProjection projection)
        {
            var existingOrder = projection.orders.FirstOrDefault(x => x.orderNumber == order.orderNumber);

            if (existingOrder is null)
            {
                ++projection.count;
                projection.orders.Add(new OrdersDetails
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

            await _customerViewRepository.UpdateDocument(projection);
        }

        private async Task CreateCustomerOverview(Order order, string customerId)
        {
            var overview = new CustomerProjection
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

            await _customerViewRepository.CreateDocument(overview);
        }
    }
}