using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Common
{
    public class QRScanningService
    {
        private readonly LiteDbContext _context;

        public QRScanningService(LiteDbContext context)
        {
            _context = context;
        }

        public QRScanResponse ScanQR(QRScanRequest request)
        {
            // Parse QR code and get details
            var qrDetails = ParseQRCode(request.QrCode);

            // Log movement if applicable
            if (request.ScanType != "Verification")
            {
                LogMovement(request);
            }

            return new QRScanResponse
            {
                Success = true,
                QrDetails = qrDetails,
                MovementLogged = request.ScanType != "Verification",
                MovementId = $"MOV-{DateTime.Now:yyyyMMdd}-{new Random().Next(10000, 99999)}",
                Message = "QR scanned successfully"
            };
        }

        private QRDetails ParseQRCode(string qrCode)
        {
            // Parse QR code format: ECO-B-20260221-3130-01-0005
            // or ECO-PLT-20260221-001
            // or ECO-CTN-20260221-001-01

            return new QRDetails
            {
                QrCode = qrCode,
                EntityType = "Bottle",
                EntityId = qrCode,
                BatchCode = "B-20260221-3130",
                BottleType = "Thick",
                CapacityML = 500,
                ManufactureDate = DateTime.Parse("2026-02-21"),
                ExpiryDate = DateTime.Parse("2026-08-21"),
                CurrentStatus = "InTransit",
                LastLocation = "Plant-Mumbai-01"
            };
        }

        private void LogMovement(QRScanRequest request)
        {
            var movement = new BottleMovement
            {
                BottleId = request.QrCode,
                FromLocationType = "Unknown",
                FromLocationId = "Unknown",
                ToLocationType = request.ScanType,
                ToLocationId = request.Location?.ToString() ?? "Unknown",
                EventType = request.ScanType,
                Timestamp = request.Timestamp,
                UserId = request.UserId ?? "Unknown"
            };

            // Save to database
            // var collection = _context.Database.GetCollection<BottleMovement>("movements");
            // collection.Insert(movement);
        }

        public QRGenerationResponse GenerateQR(QRGenerationRequest request)
        {
            var qrCode = $"ECO-{request.EntityType}-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            // Save to QR registry
            var registry = new QrRegistry
            {
                QrId = qrCode,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                CreatedAt = DateTime.Now
            };

            return new QRGenerationResponse
            {
                Success = true,
                QrCode = qrCode,
                QrImageBase64 = GenerateQRImage(qrCode),
                Message = "QR code generated successfully"
            };
        }

        private string GenerateQRImage(string qrCode)
        {
            // Generate QR code image using a library like QRCoder
            // Return base64 encoded image
            return "base64_encoded_qr_image";
        }
    }

    public class QRScanRequest
    {
        public string QrCode { get; set; } = string.Empty;
        public string ScanType { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public object? Location { get; set; }
        public DateTime Timestamp { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class QRScanResponse
    {
        public bool Success { get; set; }
        public QRDetails QrDetails { get; set; } = new();
        public bool MovementLogged { get; set; }
        public string MovementId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class QRDetails
    {
        public string QrCode { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string BatchCode { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int DaysToExpiry => (ExpiryDate - DateTime.Now).Days;
        public string CurrentStatus { get; set; } = string.Empty;
        public string LastLocation { get; set; } = string.Empty;
    }

    public class QRGenerationRequest
    {
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class QRGenerationResponse
    {
        public bool Success { get; set; }
        public string QrCode { get; set; } = string.Empty;
        public string QrImageBase64 { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
