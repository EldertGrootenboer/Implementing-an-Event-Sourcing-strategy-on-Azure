using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Demo.Common
{
    public class WarehouseService
    {
        private readonly WarehouseRepository _warehouseViewRepository;
        
        public WarehouseService(WarehouseRepository warehouseViewRepository)
        {
            _warehouseViewRepository = warehouseViewRepository;
        }
        
        public async Task HandleWarehouseView(Order order)
        {
            if (order.orderStatus != "REQUESTED")
            {
                return;
            }

            var partitionKey = $"{order.hubId}_{DateTime.Today:MM.dd.yyyy}";

            var overview = await _warehouseViewRepository.GetDocument(partitionKey);

            if (overview is null)
            {
                await CreateHubOverview(order, partitionKey);
            }
            else
            {
                await UpdateHubOverview(order, overview);
            }
        }

        private async Task UpdateHubOverview(Order order, WarehouseProjection projection)
        {
            if (projection.orderNumbers.Contains(order.orderNumber))
            {
                return;
            }

            ++projection.count;
            projection.orderNumbers.Add(order.orderNumber);

            await _warehouseViewRepository.UpdateDocument(projection);
        }

        private async Task CreateHubOverview(Order order,
            string partitionKey)
        {
            WarehouseProjection projection;
            projection = new WarehouseProjection
            {
                hubId = order.hubId,
                count = 1,
                partitionKey = partitionKey,
                orderNumbers = new List<string> {order.orderNumber}
            };

            await _warehouseViewRepository.CreateDocument(projection);
        }
    }
}