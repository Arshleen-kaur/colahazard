using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.SmallRetailer
{
    public class InventoryManagementService
    {
        private readonly LiteDbContext _context;

        public InventoryManagementService(LiteDbContext context)
        {
            _context = context;
        }

        public InventoryResponse GetInventory(int retailerId, string? bottleType = null, int? capacityML = null)
        {
            // Get inventory from database
            var inventory = new List<InventoryItemDetail>();

            return new InventoryResponse
            {
                RetailerId = retailerId,
                LastUpdated = DateTime.Now,
                Inventory = inventory,
                Summary = new InventorySummary
                {
                    TotalItems = 6,
                    TotalUnits = 1250,
                    TotalValue = 18750.00m,
                    LowStockItems = 2,
                    OutOfStockItems = 0,
                    ExpiringItems = 1
                }
            };
        }

        public StockUpdateResponse UpdateStock(StockUpdateRequest request)
        {
            // Update stock in database
            return new StockUpdateResponse
            {
                Success = true,
                UpdatedStock = request.Quantity,
                Message = "Stock updated successfully"
            };
        }
    }

    public class InventoryResponse
    {
        public int RetailerId { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<InventoryItemDetail> Inventory { get; set; } = new();
        public InventorySummary Summary { get; set; } = new();
    }

    public class InventoryItemDetail
    {
        public string ItemId { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int CurrentStock { get; set; }
        public int MinThreshold { get; set; }
        public int MaxCapacity { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal StockValue { get; set; }
        public int AverageDailySales { get; set; }
        public int DaysOfStock { get; set; }
        public int ReorderPoint { get; set; }
        public int SuggestedReorder { get; set; }
        public List<RackInfo> Racks { get; set; } = new();
        public List<ExpiringUnit> ExpiringUnits { get; set; } = new();
    }

    public class RackInfo
    {
        public string RackId { get; set; } = string.Empty;
        public string RackCode { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentUnits { get; set; }
        public double Occupancy { get; set; }
    }

    public class ExpiringUnit
    {
        public string BatchCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysToExpiry { get; set; }
    }

    public class InventorySummary
    {
        public int TotalItems { get; set; }
        public int TotalUnits { get; set; }
        public decimal TotalValue { get; set; }
        public int LowStockItems { get; set; }
        public int OutOfStockItems { get; set; }
        public int ExpiringItems { get; set; }
    }

    public class StockUpdateRequest
    {
        public int RetailerId { get; set; }
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int Quantity { get; set; }
        public string UpdateType { get; set; } = string.Empty; // Add, Remove, Set
        public string Reason { get; set; } = string.Empty;
    }

    public class StockUpdateResponse
    {
        public bool Success { get; set; }
        public int UpdatedStock { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
