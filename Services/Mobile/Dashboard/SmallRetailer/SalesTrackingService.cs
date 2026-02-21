using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.SmallRetailer
{
    public class SalesTrackingService
    {
        private readonly LiteDbContext _context;

        public SalesTrackingService(LiteDbContext context)
        {
            _context = context;
        }

        public SaleRecordResponse RecordSale(SaleRecordRequest request)
        {
            var saleId = $"SALE-{DateTime.Now:yyyyMMdd}-{new Random().Next(10000, 99999)}";
            var invoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{new Random().Next(10000, 99999)}";

            // Save sale to database
            // Update inventory

            return new SaleRecordResponse
            {
                Success = true,
                SaleId = saleId,
                InvoiceNumber = invoiceNumber,
                InvoiceUrl = $"https://storage.packtrack.com/invoices/{DateTime.Now:yyyy/MM/dd}/{invoiceNumber}.pdf",
                TotalAmount = request.TotalAmount,
                PaymentStatus = "Paid",
                InventoryUpdated = true,
                UpdatedStock = request.Items.Select(i => new StockUpdate
                {
                    BottleType = i.BottleType,
                    CapacityML = i.CapacityML,
                    PreviousStock = 450,
                    CurrentStock = 450 - i.Quantity
                }).ToList(),
                Message = "Sale recorded successfully"
            };
        }

        public SalesHistoryResponse GetSalesHistory(int retailerId, DateTime startDate, DateTime endDate)
        {
            // Get sales history from database
            return new SalesHistoryResponse
            {
                RetailerId = retailerId,
                Period = new DatePeriod { StartDate = startDate, EndDate = endDate },
                Sales = new List<DailySales>(),
                Summary = new SalesSummary()
            };
        }
    }

    public class SaleRecordRequest
    {
        public int RetailerId { get; set; }
        public string SaleType { get; set; } = string.Empty;
        public List<SaleItem> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public CustomerInfo CustomerInfo { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class SaleItem
    {
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
    }

    public class CustomerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string LoyaltyId { get; set; } = string.Empty;
    }

    public class SaleRecordResponse
    {
        public bool Success { get; set; }
        public string SaleId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceUrl { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool InventoryUpdated { get; set; }
        public List<StockUpdate> UpdatedStock { get; set; } = new();
        public LoyaltyPoints LoyaltyPoints { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class StockUpdate
    {
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int PreviousStock { get; set; }
        public int CurrentStock { get; set; }
    }

    public class LoyaltyPoints
    {
        public int Earned { get; set; }
        public int TotalPoints { get; set; }
    }

    public class SalesHistoryResponse
    {
        public int RetailerId { get; set; }
        public DatePeriod Period { get; set; } = new();
        public List<DailySales> Sales { get; set; } = new();
        public SalesSummary Summary { get; set; } = new();
    }

    public class DatePeriod
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class DailySales
    {
        public DateTime Date { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public decimal AverageTransaction { get; set; }
        public string TopProduct { get; set; } = string.Empty;
    }

    public class SalesSummary
    {
        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public double AverageDailySales { get; set; }
        public decimal AverageDailyRevenue { get; set; }
        public double ProfitMargin { get; set; }
    }
}
