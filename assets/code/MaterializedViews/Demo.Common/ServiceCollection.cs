using Microsoft.Azure.Cosmos;

namespace Demo.Common
{
    public class ServiceCollection
    {
        public ServiceCollection(Database database)
        {
            var customerOverviewRepository = new CustomerRepository(database);
            CustomerService = new CustomerService(customerOverviewRepository);

            var warehouseOverviewRepository = new WarehouseRepository(database);
            WarehouseService = new WarehouseService(warehouseOverviewRepository);
        }
        
        public CustomerService CustomerService { get; }
        
        public WarehouseService WarehouseService { get; }
    }
}