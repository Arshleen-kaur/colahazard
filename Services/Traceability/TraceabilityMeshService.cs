using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Traceability
{
    /// <summary>
    /// EcoCola Traceability Mesh Service
    /// Complete product journey tracking from production to recycling
    /// </summary>
    public class TraceabilityMeshService
    {
        private readonly LiteDbContext _context;

        public TraceabilityMeshService(LiteDbContext context)
        {
            _context = context;
        }

        #region QR Scanning & Validation

        /// <summary>
        /// Scan and validate QR code with complete traceability information
        /// </summary>
        public TraceabilityScanResult ScanQRCode(string qrCode, ScanContext scanContext)
        {
            // 1. Validate QR format
            if (!ValidateQRFormat(qrCode))
            {
                return new TraceabilityScanResult
                {
                    Success = false,
                    ErrorMessage = "Invalid QR code format"
                };
            }

            // 2. Lookup QR in registry
            var qrRegistry = GetQRRegistry(qrCode);
            if (qrRegistry == null)
            {
                return new TraceabilityScanResult
                {
                    Success = false,
                    ErrorMessage = "QR code not found in system"
                };
            }

            // 3. Get entity details based on type
            var entityDetails = GetEntityDetails(qrRegistry.EntityType, qrRegistry.EntityId);

            // 4. Get complete traceability chain
            var traceabilityChain = BuildTraceabilityChain(qrRegistry.EntityType, qrRegistry.EntityId);

            // 5. Get movement history
            var movementHistory = GetMovementHistory(qrCode);

            // 6. Log scan event
            LogScanEvent(qrCode, scanContext);

            // 7. Build result
            return new TraceabilityScanResult
            {
                Success = true,
                QRCode = qrCode,
                EntityType = qrRegistry.EntityType,
                EntityId = qrRegistry.EntityId,
                EntityDetails = entityDetails,
                TraceabilityChain = traceabilityChain,
                MovementHistory = movementHistory,
                ScannedAt = DateTime.Now,
                ScannedBy = scanContext.UserId,
                ScanLocation = scanContext.Location
            };
        }

        /// <summary>
        /// Validate QR code format
        /// </summary>
        private bool ValidateQRFormat(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode)) return false;
            if (!qrCode.StartsWith("ECO-")) return false;

            var parts = qrCode.Split('-');
            if (parts.Length < 3) return false;

            var validTypes = new[] { "B", "PLT", "CTN" };
            return validTypes.Contains(parts[1]) || (parts[1] == "B" && parts.Length >= 5);
        }

        #endregion

        #region Traceability Chain Building

        /// <summary>
        /// Build complete traceability chain from bottle to batch
        /// </summary>
        private TraceabilityChain BuildTraceabilityChain(string entityType, string entityId)
        {
            var chain = new TraceabilityChain();

            switch (entityType)
            {
                case "Bottle":
                    chain = BuildBottleChain(entityId);
                    break;
                case "Carton":
                    chain = BuildCartonChain(int.Parse(entityId));
                    break;
                case "Pallet":
                    chain = BuildPalletChain(int.Parse(entityId));
                    break;
                case "Batch":
                    chain = BuildBatchChain(int.Parse(entityId));
                    break;
            }

            return chain;
        }

        private TraceabilityChain BuildBottleChain(string bottleId)
        {
            var bottle = GetBottle(bottleId);
            if (bottle == null) return new TraceabilityChain();

            var chain = new TraceabilityChain
            {
                Bottle = new BottleTraceInfo
                {
                    BottleId = bottle.BottleId,
                    BottleType = bottle.BottleType,
                    CapacityML = bottle.CapacityML,
                    MaterialGrade = bottle.MaterialGrade,
                    ManufactureDate = bottle.ManufactureDate,
                    ExpiryDate = bottle.ExpiryDate,
                    CurrentStatus = bottle.CurrentStatus,
                    CurrentLocation = bottle.CurrentLocationType,
                    RecycleCycleCount = bottle.RecycleCycleCount,
                    IsExpired = bottle.IsExpired,
                    DaysToExpiry = (bottle.ExpiryDate - DateTime.Now).Days,
                    LiquidSpecs = new LiquidSpecs
                    {
                        LiquidType = bottle.LiquidType,
                        PHValue = bottle.pHValue,
                        CarbonationLevel = bottle.CarbonationLevel,
                        BrixLevel = bottle.BrixLevel,
                        FillTemperature = bottle.FillTemperature
                    },
                    PackagingSpecs = new PackagingSpecs
                    {
                        ThicknessMicron = bottle.ThicknessMicron,
                        CapType = bottle.CapType,
                        LabelVersion = bottle.LabelVersion,
                        PressureAtSeal = bottle.PressureAtSeal
                    }
                }
            };

            // Get carton if assigned
            if (bottle.CartonId.HasValue)
            {
                var carton = GetCarton(bottle.CartonId.Value);
                if (carton != null)
                {
                    chain.Carton = new CartonTraceInfo
                    {
                        CartonId = carton.Id,
                        CartonCode = carton.CartonCode,
                        UnitsPerCarton = carton.UnitsPerCarton,
                        CurrentUnits = carton.CurrentUnits,
                        Status = carton.Status
                    };
                }
            }

            // Get pallet if assigned
            if (bottle.PalletId.HasValue)
            {
                var pallet = GetPallet(bottle.PalletId.Value);
                if (pallet != null)
                {
                    chain.Pallet = new PalletTraceInfo
                    {
                        PalletId = pallet.Id,
                        PalletCode = pallet.PalletCode,
                        TotalUnits = pallet.TotalUnits,
                        CartonCount = pallet.CartonCount,
                        GrossWeight = pallet.GrossWeight,
                        NetWeight = pallet.NetWeight,
                        Status = pallet.Status,
                        DispatchReadyAt = pallet.DispatchReadyAt
                    };
                }
            }

            // Get batch
            var batch = GetBatch(bottle.BatchId);
            if (batch != null)
            {
                chain.Batch = BuildBatchTraceInfo(batch);
            }

            return chain;
        }

        private TraceabilityChain BuildCartonChain(int cartonId)
        {
            var carton = GetCarton(cartonId);
            if (carton == null) return new TraceabilityChain();

            var chain = new TraceabilityChain
            {
                Carton = new CartonTraceInfo
                {
                    CartonId = carton.Id,
                    CartonCode = carton.CartonCode,
                    UnitsPerCarton = carton.UnitsPerCarton,
                    CurrentUnits = carton.CurrentUnits,
                    Status = carton.Status
                }
            };

            // Get pallet
            var pallet = GetPallet(carton.PalletId);
            if (pallet != null)
            {
                chain.Pallet = new PalletTraceInfo
                {
                    PalletId = pallet.Id,
                    PalletCode = pallet.PalletCode,
                    TotalUnits = pallet.TotalUnits,
                    CartonCount = pallet.CartonCount,
                    GrossWeight = pallet.GrossWeight,
                    Status = pallet.Status
                };
            }

            // Get batch
            var batch = GetBatch(carton.BatchId);
            if (batch != null)
            {
                chain.Batch = BuildBatchTraceInfo(batch);
            }

            return chain;
        }

        private TraceabilityChain BuildPalletChain(int palletId)
        {
            var pallet = GetPallet(palletId);
            if (pallet == null) return new TraceabilityChain();

            var chain = new TraceabilityChain
            {
                Pallet = new PalletTraceInfo
                {
                    PalletId = pallet.Id,
                    PalletCode = pallet.PalletCode,
                    TotalUnits = pallet.TotalUnits,
                    CartonCount = pallet.CartonCount,
                    GrossWeight = pallet.GrossWeight,
                    Status = pallet.Status
                }
            };

            // Get batch
            var batch = GetBatch(pallet.BatchId);
            if (batch != null)
            {
                chain.Batch = BuildBatchTraceInfo(batch);
            }

            return chain;
        }

        private TraceabilityChain BuildBatchChain(int batchId)
        {
            var batch = GetBatch(batchId);
            if (batch == null) return new TraceabilityChain();

            return new TraceabilityChain
            {
                Batch = BuildBatchTraceInfo(batch)
            };
        }

        private BatchTraceInfo BuildBatchTraceInfo(ProductionBatch batch)
        {
            var plant = GetPlant(batch.PlantId);

            return new BatchTraceInfo
            {
                BatchId = batch.Id,
                BatchCode = batch.BatchCode,
                PlantInfo = plant != null ? new PlantInfo
                {
                    PlantId = plant.PlantId,
                    PlantCode = plant.PlantCode,
                    PlantName = plant.Name,
                    Location = plant.Location,
                    Country = plant.Country
                } : null,
                ProductionInfo = new ProductionInfo
                {
                    ProductionLineId = batch.ProductionLineId,
                    MachineId = batch.MachineId,
                    ShiftCode = batch.ShiftCode,
                    OperatorId = batch.OperatorId,
                    SupervisorId = batch.SupervisorId,
                    ManufactureDate = batch.ManufactureDate,
                    ExpiryDate = batch.ExpiryDate
                },
                ProductSpecs = new ProductSpecs
                {
                    BottleType = batch.BottleType,
                    CapacityML = batch.CapacityML,
                    ContainerType = batch.ContainerType,
                    MaterialGrade = batch.BottleMaterialGrade,
                    ThicknessMicron = batch.BottleThicknessMicron,
                    LiquidType = batch.LiquidType,
                    LiquidBatchCode = batch.LiquidBatchCode
                },
                QualityInfo = new QualityInfo
                {
                    QualityStatus = batch.QualityStatus,
                    TotalPlannedUnits = batch.TotalPlannedUnits,
                    TotalProducedUnits = batch.TotalProducedUnits,
                    TotalRejectedUnits = batch.TotalRejectedUnits,
                    YieldPercentage = batch.TotalPlannedUnits > 0 
                        ? (double)batch.TotalProducedUnits / batch.TotalPlannedUnits * 100 
                        : 0,
                    ApprovedAt = batch.ApprovedAt
                },
                CommercialInfo = new CommercialInfo
                {
                    TargetMarket = batch.TargetMarket,
                    DistributorId = batch.DistributorId,
                    WholesaleRate = batch.WholesaleRate,
                    MRP = batch.MRP,
                    TaxCode = batch.TaxCode
                }
            };
        }

        #endregion

        #region Movement History

        /// <summary>
        /// Get complete movement history for an entity
        /// </summary>
        private List<MovementRecord> GetMovementHistory(string qrCode)
        {
            var collection = _context.Database.GetCollection<BottleMovement>("movements");
            var movements = collection.Find(m => m.BottleId == qrCode).OrderByDescending(m => m.Timestamp).ToList();

            return movements.Select(m => new MovementRecord
            {
                MovementId = m.Id,
                EventType = m.EventType,
                FromLocation = new LocationInfo
                {
                    LocationType = m.FromLocationType,
                    LocationId = m.FromLocationId
                },
                ToLocation = new LocationInfo
                {
                    LocationType = m.ToLocationType,
                    LocationId = m.ToLocationId
                },
                Timestamp = m.Timestamp,
                UserId = m.UserId,
                DurationAtLocation = CalculateDurationAtLocation(movements, m)
            }).ToList();
        }

        private TimeSpan? CalculateDurationAtLocation(List<BottleMovement> movements, BottleMovement currentMovement)
        {
            var nextMovement = movements
                .Where(m => m.Timestamp > currentMovement.Timestamp)
                .OrderBy(m => m.Timestamp)
                .FirstOrDefault();

            if (nextMovement != null)
            {
                return nextMovement.Timestamp - currentMovement.Timestamp;
            }

            return null;
        }

        #endregion

        #region Entity Details

        private object GetEntityDetails(string entityType, string entityId)
        {
            return entityType switch
            {
                "Bottle" => GetBottle(entityId),
                "Carton" => GetCarton(int.Parse(entityId)),
                "Pallet" => GetPallet(int.Parse(entityId)),
                "Batch" => GetBatch(int.Parse(entityId)),
                _ => null
            };
        }

        #endregion

        #region Scan Event Logging

        private void LogScanEvent(string qrCode, ScanContext context)
        {
            var scanLog = new ScanLog
            {
                QRCode = qrCode,
                ScannedBy = context.UserId,
                ScanLocation = context.Location,
                ScanType = context.ScanType,
                DeviceId = context.DeviceId,
                Timestamp = DateTime.Now,
                Latitude = context.Latitude,
                Longitude = context.Longitude
            };

            var collection = _context.Database.GetCollection<ScanLog>("scan_logs");
            collection.Insert(scanLog);
        }

        #endregion

        #region Analytics & Insights

        /// <summary>
        /// Get traceability analytics for a batch
        /// </summary>
        public TraceabilityAnalytics GetBatchAnalytics(int batchId)
        {
            var batch = GetBatch(batchId);
            if (batch == null) return new TraceabilityAnalytics();

            var bottles = GetBottlesByBatch(batchId);
            var movements = GetBatchMovements(batchId);
            var scans = GetBatchScans(batchId);

            return new TraceabilityAnalytics
            {
                BatchId = batchId,
                BatchCode = batch.BatchCode,
                TotalUnits = batch.TotalProducedUnits,
                TrackedUnits = bottles.Count,
                TraceabilityPercentage = batch.TotalProducedUnits > 0 
                    ? (double)bottles.Count / batch.TotalProducedUnits * 100 
                    : 0,
                StatusDistribution = bottles.GroupBy(b => b.CurrentStatus)
                    .ToDictionary(g => g.Key, g => g.Count()),
                LocationDistribution = bottles.GroupBy(b => b.CurrentLocationType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TotalMovements = movements.Count,
                TotalScans = scans.Count,
                AverageScansPerUnit = bottles.Count > 0 ? (double)scans.Count / bottles.Count : 0,
                RecycledUnits = bottles.Count(b => b.CurrentStatus == "Recycled"),
                RecyclingRate = bottles.Count > 0 
                    ? (double)bottles.Count(b => b.CurrentStatus == "Recycled") / bottles.Count * 100 
                    : 0
            };
        }

        #endregion

        #region Helper Methods

        private QrRegistry? GetQRRegistry(string qrCode)
        {
            var collection = _context.Database.GetCollection<QrRegistry>("qr_registry");
            return collection.FindOne(q => q.QrId == qrCode);
        }

        private BottleUnit? GetBottle(string bottleId)
        {
            var collection = _context.Database.GetCollection<BottleUnit>("bottles");
            return collection.FindById(bottleId);
        }

        private Carton? GetCarton(int cartonId)
        {
            var collection = _context.Database.GetCollection<Carton>("cartons");
            return collection.FindById(cartonId);
        }

        private Pallet? GetPallet(int palletId)
        {
            var collection = _context.Database.GetCollection<Pallet>("pallets");
            return collection.FindById(palletId);
        }

        private ProductionBatch? GetBatch(int batchId)
        {
            var collection = _context.Database.GetCollection<ProductionBatch>("batches");
            return collection.FindById(batchId);
        }

        private Plant? GetPlant(int plantId)
        {
            var collection = _context.Database.GetCollection<Plant>("plants");
            return collection.FindById(plantId);
        }

        private List<BottleUnit> GetBottlesByBatch(int batchId)
        {
            var collection = _context.Database.GetCollection<BottleUnit>("bottles");
            return collection.Find(b => b.BatchId == batchId).ToList();
        }

        private List<BottleMovement> GetBatchMovements(int batchId)
        {
            var bottles = GetBottlesByBatch(batchId);
            var bottleIds = bottles.Select(b => b.BottleId).ToList();

            var collection = _context.Database.GetCollection<BottleMovement>("movements");
            return collection.Find(m => bottleIds.Contains(m.BottleId)).ToList();
        }

        private List<ScanLog> GetBatchScans(int batchId)
        {
            var bottles = GetBottlesByBatch(batchId);
            var qrCodes = bottles.Select(b => b.BottleId).ToList();

            var collection = _context.Database.GetCollection<ScanLog>("scan_logs");
            return collection.Find(s => qrCodes.Contains(s.QRCode)).ToList();
        }

        #endregion
    }

    #region Models

    public class ScanContext
    {
        public string UserId { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string ScanType { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class TraceabilityScanResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public object? EntityDetails { get; set; }
        public TraceabilityChain TraceabilityChain { get; set; } = new();
        public List<MovementRecord> MovementHistory { get; set; } = new();
        public DateTime ScannedAt { get; set; }
        public string ScannedBy { get; set; } = string.Empty;
        public string ScanLocation { get; set; } = string.Empty;
    }

    public class TraceabilityChain
    {
        public BottleTraceInfo? Bottle { get; set; }
        public CartonTraceInfo? Carton { get; set; }
        public PalletTraceInfo? Pallet { get; set; }
        public BatchTraceInfo? Batch { get; set; }
    }

    public class BottleTraceInfo
    {
        public string BottleId { get; set; } = string.Empty;
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public string MaterialGrade { get; set; } = string.Empty;
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string CurrentLocation { get; set; } = string.Empty;
        public int RecycleCycleCount { get; set; }
        public bool IsExpired { get; set; }
        public int DaysToExpiry { get; set; }
        public LiquidSpecs LiquidSpecs { get; set; } = new();
        public PackagingSpecs PackagingSpecs { get; set; } = new();
    }

    public class LiquidSpecs
    {
        public string LiquidType { get; set; } = string.Empty;
        public double PHValue { get; set; }
        public double CarbonationLevel { get; set; }
        public double BrixLevel { get; set; }
        public double FillTemperature { get; set; }
    }

    public class PackagingSpecs
    {
        public int ThicknessMicron { get; set; }
        public string CapType { get; set; } = string.Empty;
        public string LabelVersion { get; set; } = string.Empty;
        public double PressureAtSeal { get; set; }
    }

    public class CartonTraceInfo
    {
        public int CartonId { get; set; }
        public string CartonCode { get; set; } = string.Empty;
        public int UnitsPerCarton { get; set; }
        public int CurrentUnits { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class PalletTraceInfo
    {
        public int PalletId { get; set; }
        public string PalletCode { get; set; } = string.Empty;
        public int TotalUnits { get; set; }
        public int CartonCount { get; set; }
        public double GrossWeight { get; set; }
        public double NetWeight { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? DispatchReadyAt { get; set; }
    }

    public class BatchTraceInfo
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public PlantInfo? PlantInfo { get; set; }
        public ProductionInfo ProductionInfo { get; set; } = new();
        public ProductSpecs ProductSpecs { get; set; } = new();
        public QualityInfo QualityInfo { get; set; } = new();
        public CommercialInfo CommercialInfo { get; set; } = new();
    }

    public class PlantInfo
    {
        public int PlantId { get; set; }
        public string PlantCode { get; set; } = string.Empty;
        public string PlantName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }

    public class ProductionInfo
    {
        public string ProductionLineId { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string ShiftCode { get; set; } = string.Empty;
        public string OperatorId { get; set; } = string.Empty;
        public string SupervisorId { get; set; } = string.Empty;
        public DateTime ManufactureDate { get; set; }
        public DateTime ExpiryDate { get; set; }
    }

    public class ProductSpecs
    {
        public string BottleType { get; set; } = string.Empty;
        public int CapacityML { get; set; }
        public string ContainerType { get; set; } = string.Empty;
        public string MaterialGrade { get; set; } = string.Empty;
        public int ThicknessMicron { get; set; }
        public string LiquidType { get; set; } = string.Empty;
        public string LiquidBatchCode { get; set; } = string.Empty;
    }

    public class QualityInfo
    {
        public string QualityStatus { get; set; } = string.Empty;
        public int TotalPlannedUnits { get; set; }
        public int TotalProducedUnits { get; set; }
        public int TotalRejectedUnits { get; set; }
        public double YieldPercentage { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public class CommercialInfo
    {
        public string TargetMarket { get; set; } = string.Empty;
        public string DistributorId { get; set; } = string.Empty;
        public decimal WholesaleRate { get; set; }
        public decimal MRP { get; set; }
        public string TaxCode { get; set; } = string.Empty;
    }

    public class MovementRecord
    {
        public int MovementId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public LocationInfo FromLocation { get; set; } = new();
        public LocationInfo ToLocation { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string UserId { get; set; } = string.Empty;
        public TimeSpan? DurationAtLocation { get; set; }
    }

    public class LocationInfo
    {
        public string LocationType { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
    }

    public class ScanLog
    {
        public int Id { get; set; }
        public string QRCode { get; set; } = string.Empty;
        public string ScannedBy { get; set; } = string.Empty;
        public string ScanLocation { get; set; } = string.Empty;
        public string ScanType { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class TraceabilityAnalytics
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public int TotalUnits { get; set; }
        public int TrackedUnits { get; set; }
        public double TraceabilityPercentage { get; set; }
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public Dictionary<string, int> LocationDistribution { get; set; } = new();
        public int TotalMovements { get; set; }
        public int TotalScans { get; set; }
        public double AverageScansPerUnit { get; set; }
        public int RecycledUnits { get; set; }
        public double RecyclingRate { get; set; }
    }

    #endregion
}
