using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.Wholesaler
{
    public class BulkOrderManagementService
    {
        private readonly LiteDbContext _context;

        public BulkOrderManagementService(LiteDbContext context)
        {
            _context = context;
        }

        public BulkOrderResponse CreateBulkOrder(BulkOrderRequest request)
        {
            var orderId = $"WHL-ORD-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            // Save order to database

            return new BulkOrderResponse
            {
                Success = true,
                OrderId = orderId,
                OrderNumber = orderId,
                Status = "Pending",
                TotalAmount = request.TotalAmount,
                EstimatedDelivery = request.DeliveryDate,
                SupplierConfirmation = "Pending",
                Message = "Bulk order created successfully"
            };
        }

        public List<PendingOrder> GetPendingOrders(int wholesalerId, string? status = null)
        {
            // Get pending orders from database
            return new List<PendingOrder>();
        }
    }

    public class BulkOrderRequest
    {
        public int WholesalerId { get; set; }
        public int SupplierId { get; set; }
        public string OrderType { get; set; } = string.Empty;
        public List<BulkOrderItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class BulkOrderItem
    {
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }

    public class BulkOrderResponse
    {
        public bool Success { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime EstimatedDelivery { get; set; }
        public string SupplierConfirmation { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class PendingOrder
    {
        public string OrderId { get; set; } = string.Empty;
        public int RetailerId { get; set; }
        public string RetailerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime RequestedDeliveryDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public string FulfillmentStatus { get; set; } = string.Empty;
        public int PickedUnits { get; set; }
        public int RemainingUnits { get; set; }
        public string AssignedWarehouse { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class OrderItem
    {
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
