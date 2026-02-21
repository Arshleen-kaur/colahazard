using PackTrack.Models;
using PackTrack.Data;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace PackTrack.Services.QRCode
{
    /// <summary>
    /// Comprehensive QR Code Generation Service for hierarchical product tracking
    /// Generates QR codes for: Batch → Pallet → Carton → Bottle
    /// </summary>
    public class QRCodeGenerationService
    {
        private readonly LiteDbContext _context;
        private readonly QRCodePrintService _printService;

        public QRCodeGenerationService(LiteDbContext context, QRCodePrintService printService)
        {
            _context = context;
            _printService = printService;
        }

        #region Batch QR Generation

        /// <summary>
        /// Generate QR code for a production batch
        /// Format: ECO-B-YYYYMMDD-XXXX
        /// </summary>
        public BatchQRResult GenerateBatchQR(int batchId)
        {
            var batch = GetBatch(batchId);
            if (batch == null)
                throw new Exception($"Batch {batchId} not found");

            // Generate unique batch code
            var batchCode = $"B-{DateTime.Now:yyyyMMdd}-{GenerateSequence(4)}";
            var qrCode = $"ECO-{batchCode}";

            // Update batch with QR code
            batch.BatchCode = batchCode;
            SaveBatch(batch);

            // Register QR in registry
            RegisterQR(qrCode, "Batch", batchId.ToString());

            // Generate QR image
            var qrImage = GenerateQRImage(qrCode, 300);

            // Save QR image to storage
            var imagePath = SaveQRImage(qrImage, qrCode, "Batch");

            return new BatchQRResult
            {
                BatchId = batchId,
                BatchCode = batchCode,
                QRCode = qrCode,
                QRImagePath = imagePath,
                QRImageBase64 = ConvertImageToBase64(qrImage),
                GeneratedAt = DateTime.Now
            };
        }

        #endregion

        #region Pallet QR Generation

        /// <summary>
        /// Generate QR codes for all pallets in a batch
        /// Format: ECO-PLT-YYYYMMDD-XXXX
        /// </summary>
        public List<PalletQRResult> GeneratePalletQRs(int batchId, int palletCount, int unitsPerPallet)
        {
            var batch = GetBatch(batchId);
            if (batch == null)
                throw new Exception($"Batch {batchId} not found");

            var results = new List<PalletQRResult>();

            for (int i = 1; i <= palletCount; i++)
            {
                // Create pallet
                var pallet = new Pallet
                {
                    BatchId = batchId,
                    PalletCode = $"PLT-{DateTime.Now:yyyyMMdd}-{GenerateSequence(4)}",
                    TotalUnits = unitsPerPallet,
                    Status = "Created",
                    DispatchReadyAt = null
                };

                // Save pallet to database
                var palletId = SavePallet(pallet);
                pallet.Id = palletId;

                // Generate QR code
                var qrCode = $"ECO-{pallet.PalletCode}";
                RegisterQR(qrCode, "Pallet", palletId.ToString());

                // Generate QR image
                var qrImage = GenerateQRImage(qrCode, 250);
                var imagePath = SaveQRImage(qrImage, qrCode, "Pallet");

                results.Add(new PalletQRResult
                {
                    PalletId = palletId,
                    PalletCode = pallet.PalletCode,
                    QRCode = qrCode,
                    QRImagePath = imagePath,
                    QRImageBase64 = ConvertImageToBase64(qrImage),
                    BatchCode = batch.BatchCode,
                    TotalUnits = unitsPerPallet,
                    SequenceNumber = i,
                    GeneratedAt = DateTime.Now
                });
            }

            return results;
        }

        /// <summary>
        /// Generate single pallet QR with label for printing
        /// </summary>
        public PalletQRResult GenerateSinglePalletQR(int batchId, int unitsPerPallet)
        {
            var results = GeneratePalletQRs(batchId, 1, unitsPerPallet);
            return results.First();
        }

        #endregion

        #region Carton QR Generation

        /// <summary>
        /// Generate QR codes for all cartons in a pallet
        /// Format: ECO-CTN-YYYYMMDD-XXXX-YY
        /// </summary>
        public List<CartonQRResult> GenerateCartonQRs(int palletId, int cartonCount, int unitsPerCarton)
        {
            var pallet = GetPallet(palletId);
            if (pallet == null)
                throw new Exception($"Pallet {palletId} not found");

            var results = new List<CartonQRResult>();

            for (int i = 1; i <= cartonCount; i++)
            {
                // Create carton
                var carton = new Carton
                {
                    PalletId = palletId,
                    BatchId = pallet.BatchId,
                    CartonCode = $"CTN-{DateTime.Now:yyyyMMdd}-{GenerateSequence(4)}-{i:D2}",
                    UnitsPerCarton = unitsPerCarton,
                    CurrentUnits = unitsPerCarton,
                    Status = "Packed"
                };

                // Save carton to database
                var cartonId = SaveCarton(carton);
                carton.Id = cartonId;

                // Generate QR code
                var qrCode = $"ECO-{carton.CartonCode}";
                RegisterQR(qrCode, "Carton", cartonId.ToString());

                // Generate QR image
                var qrImage = GenerateQRImage(qrCode, 200);
                var imagePath = SaveQRImage(qrImage, qrCode, "Carton");

                results.Add(new CartonQRResult
                {
                    CartonId = cartonId,
                    CartonCode = carton.CartonCode,
                    QRCode = qrCode,
                    QRImagePath = imagePath,
                    QRImageBase64 = ConvertImageToBase64(qrImage),
                    PalletCode = pallet.PalletCode,
                    UnitsPerCarton = unitsPerCarton,
                    SequenceNumber = i,
                    GeneratedAt = DateTime.Now
                });
            }

            // Update pallet carton count
            pallet.CartonCount = cartonCount;
            UpdatePallet(pallet);

            return results;
        }

        #endregion

        #region Bottle QR Generation

        /// <summary>
        /// Generate QR codes for all bottles in a batch
        /// Format: ECO-B-YYYYMMDD-XXXX-YY-ZZZZ
        /// </summary>
        public List<BottleQRResult> GenerateBottleQRs(int batchId, int bottleCount)
        {
            var batch = GetBatch(batchId);
            if (batch == null)
                throw new Exception($"Batch {batchId} not found");

            var results = new List<BottleQRResult>();

            for (int i = 1; i <= bottleCount; i++)
            {
                // Generate unique bottle ID
                var bottleId = $"ECO-{batch.BatchCode}-{(i / 1000):D2}-{(i % 1000):D4}";

                // Create bottle unit
                var bottle = new BottleUnit
                {
                    BottleId = bottleId,
                    BatchId = batchId,
                    BottleType = batch.BottleType,
                    ContainerType = batch.ContainerType,
                    ThicknessMicron = batch.BottleThicknessMicron,
                    CapacityML = batch.CapacityML,
                    MaterialGrade = batch.BottleMaterialGrade,
                    LiquidType = batch.LiquidType,
                    ManufactureDate = batch.ManufactureDate,
                    ExpiryDate = batch.ExpiryDate,
                    CurrentStatus = "Produced",
                    CurrentLocationType = "Plant",
                    CreatedAt = DateTime.Now
                };

                // Save bottle to database
                SaveBottle(bottle);

                // Register QR
                RegisterQR(bottleId, "Bottle", bottleId);

                // Generate QR image (smaller for bottles)
                var qrImage = GenerateQRImage(bottleId, 150);
                var imagePath = SaveQRImage(qrImage, bottleId, "Bottle");

                results.Add(new BottleQRResult
                {
                    BottleId = bottleId,
                    QRCode = bottleId,
                    QRImagePath = imagePath,
                    QRImageBase64 = ConvertImageToBase64(qrImage),
                    BatchCode = batch.BatchCode,
                    BottleType = batch.BottleType,
                    CapacityML = batch.CapacityML,
                    SequenceNumber = i,
                    GeneratedAt = DateTime.Now
                });
            }

            return results;
        }

        /// <summary>
        /// Generate bottle QRs and assign to cartons
        /// </summary>
        public List<BottleQRResult> GenerateBottleQRsWithCartonAssignment(
            int batchId, 
            List<int> cartonIds, 
            int bottlesPerCarton)
        {
            var results = new List<BottleQRResult>();
            int bottleSequence = 1;

            foreach (var cartonId in cartonIds)
            {
                var carton = GetCarton(cartonId);
                if (carton == null) continue;

                for (int i = 0; i < bottlesPerCarton; i++)
                {
                    var batch = GetBatch(batchId);
                    var bottleId = $"ECO-{batch.BatchCode}-{(bottleSequence / 1000):D2}-{(bottleSequence % 1000):D4}";

                    var bottle = new BottleUnit
                    {
                        BottleId = bottleId,
                        BatchId = batchId,
                        CartonId = cartonId,
                        PalletId = carton.PalletId,
                        BottleType = batch.BottleType,
                        ContainerType = batch.ContainerType,
                        ThicknessMicron = batch.BottleThicknessMicron,
                        CapacityML = batch.CapacityML,
                        MaterialGrade = batch.BottleMaterialGrade,
                        LiquidType = batch.LiquidType,
                        ManufactureDate = batch.ManufactureDate,
                        ExpiryDate = batch.ExpiryDate,
                        CurrentStatus = "Produced",
                        CurrentLocationType = "Plant",
                        CreatedAt = DateTime.Now
                    };

                    SaveBottle(bottle);
                    RegisterQR(bottleId, "Bottle", bottleId);

                    var qrImage = GenerateQRImage(bottleId, 150);
                    var imagePath = SaveQRImage(qrImage, bottleId, "Bottle");

                    results.Add(new BottleQRResult
                    {
                        BottleId = bottleId,
                        QRCode = bottleId,
                        QRImagePath = imagePath,
                        QRImageBase64 = ConvertImageToBase64(qrImage),
                        BatchCode = batch.BatchCode,
                        BottleType = batch.BottleType,
                        CapacityML = batch.CapacityML,
                        CartonId = cartonId,
                        PalletId = carton.PalletId,
                        SequenceNumber = bottleSequence,
                        GeneratedAt = DateTime.Now
                    });

                    bottleSequence++;
                }
            }

            return results;
        }

        #endregion

        #region Complete Hierarchy Generation

        /// <summary>
        /// Generate complete QR hierarchy for a batch
        /// Batch → Pallets → Cartons → Bottles
        /// </summary>
        public CompleteHierarchyQRResult GenerateCompleteHierarchy(CompleteHierarchyRequest request)
        {
            var result = new CompleteHierarchyQRResult
            {
                BatchId = request.BatchId,
                GeneratedAt = DateTime.Now
            };

            // 1. Generate Batch QR
            result.BatchQR = GenerateBatchQR(request.BatchId);

            // 2. Generate Pallet QRs
            result.PalletQRs = GeneratePalletQRs(
                request.BatchId, 
                request.PalletCount, 
                request.UnitsPerPallet
            );

            // 3. Generate Carton QRs for each pallet
            result.CartonQRs = new List<CartonQRResult>();
            foreach (var pallet in result.PalletQRs)
            {
                var cartonQRs = GenerateCartonQRs(
                    pallet.PalletId, 
                    request.CartonsPerPallet, 
                    request.BottlesPerCarton
                );
                result.CartonQRs.AddRange(cartonQRs);
            }

            // 4. Generate Bottle QRs and assign to cartons
            var cartonIds = result.CartonQRs.Select(c => c.CartonId).ToList();
            result.BottleQRs = GenerateBottleQRsWithCartonAssignment(
                request.BatchId, 
                cartonIds, 
                request.BottlesPerCarton
            );

            // 5. Calculate totals
            result.TotalPallets = result.PalletQRs.Count;
            result.TotalCartons = result.CartonQRs.Count;
            result.TotalBottles = result.BottleQRs.Count;

            return result;
        }

        #endregion

        #region QR Image Generation

        private byte[] GenerateQRImage(string qrText, int pixelSize)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                using (var qrCode = new QRCoder.QRCode(qrCodeData))
                {
                    using (var qrBitmap = qrCode.GetGraphic(20))
                    {
                        using (var ms = new MemoryStream())
                        {
                            qrBitmap.Save(ms, ImageFormat.Png);
                            return ms.ToArray();
                        }
                    }
                }
            }
        }

        private string ConvertImageToBase64(byte[] imageBytes)
        {
            return Convert.ToBase64String(imageBytes);
        }

        private string SaveQRImage(byte[] imageBytes, string qrCode, string entityType)
        {
            var directory = Path.Combine("wwwroot", "qrcodes", entityType, DateTime.Now.ToString("yyyy-MM-dd"));
            Directory.CreateDirectory(directory);

            var fileName = $"{qrCode.Replace("/", "-")}.png";
            var filePath = Path.Combine(directory, fileName);

            File.WriteAllBytes(filePath, imageBytes);

            return $"/qrcodes/{entityType}/{DateTime.Now:yyyy-MM-dd}/{fileName}";
        }

        #endregion

        #region Helper Methods

        private string GenerateSequence(int length)
        {
            var random = new Random();
            return random.Next(0, (int)Math.Pow(10, length)).ToString($"D{length}");
        }

        private void RegisterQR(string qrCode, string entityType, string entityId)
        {
            var registry = new QrRegistry
            {
                QrId = qrCode,
                EntityType = entityType,
                EntityId = entityId,
                CreatedAt = DateTime.Now
            };

            // Save to database
            var collection = _context.Database.GetCollection<QrRegistry>("qr_registry");
            collection.Insert(registry);
        }

        private ProductionBatch? GetBatch(int batchId)
        {
            var collection = _context.Database.GetCollection<ProductionBatch>("batches");
            return collection.FindById(batchId);
        }

        private void SaveBatch(ProductionBatch batch)
        {
            var collection = _context.Database.GetCollection<ProductionBatch>("batches");
            collection.Update(batch);
        }

        private int SavePallet(Pallet pallet)
        {
            var collection = _context.Database.GetCollection<Pallet>("pallets");
            return collection.Insert(pallet);
        }

        private Pallet? GetPallet(int palletId)
        {
            var collection = _context.Database.GetCollection<Pallet>("pallets");
            return collection.FindById(palletId);
        }

        private void UpdatePallet(Pallet pallet)
        {
            var collection = _context.Database.GetCollection<Pallet>("pallets");
            collection.Update(pallet);
        }

        private int SaveCarton(Carton carton)
        {
            var collection = _context.Database.GetCollection<Carton>("cartons");
            return collection.Insert(carton);
        }

        private Carton? GetCarton(int cartonId)
        {
            var collection = _context.Database.GetCollection<Carton>("cartons");
            return collection.FindById(cartonId);
        }

        private void SaveBottle(BottleUnit bottle)
        {
            var collection = _context.Database.GetCollection<BottleUnit>("bottles");
            collection.Insert(bottle);
        }

        #endregion
    }

    #region Result Models

    public class BatchQRResult
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string QRImagePath { get; set; } = string.Empty;
        public string QRImageBase64 { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
    }

    public class PalletQRResult
    {
        public int PalletId { get; set; }
        public string PalletCode { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string QRImagePath { get; set; } = string.Empty;
        public string QRImageBase64 { get; set; } = string.Empty;
        public string BatchCode { get; set; } = string.Empty;
        public int TotalUnits { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CartonQRResult
    {
        public int CartonId { get; set; }
        public string CartonCode { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string QRImagePath { get; set; } = string.Empty;
        public string QRImageBase64 { get; set; } = string.Empty;
        public string PalletCode { get; set; } = string.Empty;
        public int UnitsPerCarton { get; set; }
        public int SequenceNumber { get; set; }
        public int? PalletId { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class BottleQRResult
    {
        public string BottleId { get; set; } = string.Empty;
        public string QRCode { get; set; } = string.Empty;
        public string QRImagePath { get; set; } = string.Empty;
        public string QRImageBase64 { get; set; } = string.Empty;
        public string BatchCode { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public int? CartonId { get; set; }
        public int? PalletId { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CompleteHierarchyQRResult
    {
        public int BatchId { get; set; }
        public BatchQRResult BatchQR { get; set; } = new();
        public List<PalletQRResult> PalletQRs { get; set; } = new();
        public List<CartonQRResult> CartonQRs { get; set; } = new();
        public List<BottleQRResult> BottleQRs { get; set; } = new();
        public int TotalPallets { get; set; }
        public int TotalCartons { get; set; }
        public int TotalBottles { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class CompleteHierarchyRequest
    {
        public int BatchId { get; set; }
        public int PalletCount { get; set; }
        public int UnitsPerPallet { get; set; }
        public int CartonsPerPallet { get; set; }
        public int BottlesPerCarton { get; set; }
    }

    #endregion
}
