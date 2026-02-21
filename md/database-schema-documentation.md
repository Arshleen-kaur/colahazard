# PackTrack Database Schema - Complete Documentation

## Overview
Complete database schema for PackTrack EcoCola Traceability System using LiteDB (NoSQL document database).

---

## Database Collections

### 1. Plants Collection
**Collection Name**: `plants`

**Purpose**: Manufacturing plant information

**Schema**:
```csharp
{
  PlantId: int (Primary Key),
  PlantCode: string,           // QR-able identifier
  Name: string,
  Location: string,
  Country: string,             // Default: "India"
  Timezone: string,            // Default: "IST"
  IsActive: bool,
  CreatedAt: DateTime
}
```

**Indexes**:
```csharp
plants.EnsureIndex(x => x.PlantCode);
plants.EnsureIndex(x => x.IsActive);
```

**Example Document**:
```json
{
  "PlantId": 1,
  "PlantCode": "ECO-PLT-MUM-01",
  "Name": "EcoCola Mumbai Plant",
  "Location": "Andheri, Mumbai",
  "Country": "India",
  "Timezone": "IST",
  "IsActive": true,
  "CreatedAt": "2026-02-21T10:00:00Z"
}
```

---

### 2. Production Batches Collection
**Collection Name**: `batches`

**Purpose**: Production batch tracking with complete specifications

**Schema**:
```csharp
{
  Id: int (Primary Key),
  PlantId: int (Foreign Key -> plants),
  BatchCode: string,           // Format: B-YYYYMMDD-XXXX
  
  // Bottle/Container Specs
  BottleType: string,          // Thick / Thin / rPET
  BottleMaterialGrade: string,
  BottleThicknessMicron: int,
  CapacityML: int,             // 250/500/1000
  ContainerType: string,       // Bottle / Can
  
  // Liquid Specs
  LiquidType: string,          // Cola / Zero / etc
  LiquidBatchCode: string,
  BrixLevel: double,
  AcidityPH: double,
  CO2Volumes: double,
  IngredientsList: string,
  
  // Production Metadata
  ProductionLineId: string,
  MachineId: string,
  ShiftCode: string,
  OperatorId: string,
  SupervisorId: string,
  ManufactureDate: DateTime,
  ExpiryDate: DateTime,
  
  TotalPlannedUnits: int,
  TotalProducedUnits: int,
  TotalRejectedUnits: int,
  
  QualityStatus: string,       // Pending/Approved/Rejected
  Status: string,              // Produced/Packed/Shipped
  
  // Commercial
  TargetMarket: string,
  DistributorId: string,
  WholesaleRate: decimal,
  MRP: decimal,
  TaxCode: string,
  
  CreatedAt: DateTime,
  ApprovedAt: DateTime?
}
```

**Indexes**:
```csharp
batches.EnsureIndex(x => x.BatchCode);
batches.EnsureIndex(x => x.PlantId);
batches.EnsureIndex(x => x.ManufactureDate);
batches.EnsureIndex(x => x.QualityStatus);
batches.EnsureIndex(x => x.Status);
```

**Example Document**:
```json
{
  "Id": 101,
  "PlantId": 1,
  "BatchCode": "B-20260221-3130",
  "BottleType": "Thick",
  "BottleMaterialGrade": "PET-A",
  "BottleThicknessMicron": 350,
  "CapacityML": 500,
  "ContainerType": "Bottle",
  "LiquidType": "Cola Classic",
  "LiquidBatchCode": "LIQ-20260221-001",
  "BrixLevel": 11.5,
  "AcidityPH": 2.5,
  "CO2Volumes": 4.0,
  "ProductionLineId": "LINE-A1",
  "MachineId": "FILL-001",
  "ShiftCode": "SHIFT-1",
  "OperatorId": "OP-123",
  "SupervisorId": "SUP-45",
  "ManufactureDate": "2026-02-21T08:00:00Z",
  "ExpiryDate": "2026-08-21T23:59:59Z",
  "TotalPlannedUnits": 2400,
  "TotalProducedUnits": 2380,
  "TotalRejectedUnits": 20,
  "QualityStatus": "Approved",
  "Status": "Packed",
  "TargetMarket": "Mumbai Metro",
  "WholesaleRate": 15.50,
  "MRP": 20.00,
  "TaxCode": "GST-18",
  "CreatedAt": "2026-02-21T08:00:00Z",
  "ApprovedAt": "2026-02-21T10:30:00Z"
}
```

---

### 3. Pallets Collection
**Collection Name**: `pallets`

**Purpose**: Pallet-level tracking for logistics

**Schema**:
```csharp
{
  Id: int (Primary Key),
  BatchId: int (Foreign Key -> batches),
  PalletCode: string,          // Format: PLT-YYYYMMDD-XXXX
  CartonCount: int,
  TotalUnits: int,
  NetWeight: double,           // kg
  GrossWeight: double,         // kg
  Status: string,              // Packed/Shipped/Delivered
  DispatchReadyAt: DateTime?
}
```

**Indexes**:
```csharp
pallets.EnsureIndex(x => x.PalletCode);
pallets.EnsureIndex(x => x.BatchId);
pallets.EnsureIndex(x => x.Status);
```

**Example Document**:
```json
{
  "Id": 1001,
  "BatchId": 101,
  "PalletCode": "PLT-20260221-0001",
  "CartonCount": 10,
  "TotalUnits": 240,
  "NetWeight": 120.0,
  "GrossWeight": 135.5,
  "Status": "Packed",
  "DispatchReadyAt": "2026-02-21T14:00:00Z"
}
```

---

### 4. Cartons Collection
**Collection Name**: `cartons`

**Purpose**: Carton-level tracking within pallets

**Schema**:
```csharp
{
  Id: int (Primary Key),
  PalletId: int (Foreign Key -> pallets),
  BatchId: int (Foreign Key -> batches),
  CartonCode: string,          // Format: CTN-YYYYMMDD-XXXX-YY
  UnitsPerCarton: int,
  CurrentUnits: int,
  Weight: double,              // kg
  Status: string               // Packed/Opened/Empty
}
```

**Indexes**:
```csharp
cartons.EnsureIndex(x => x.CartonCode);
cartons.EnsureIndex(x => x.PalletId);
cartons.EnsureIndex(x => x.BatchId);
```

**Example Document**:
```json
{
  "Id": 5001,
  "PalletId": 1001,
  "BatchId": 101,
  "CartonCode": "CTN-20260221-0001-01",
  "UnitsPerCarton": 24,
  "CurrentUnits": 24,
  "Weight": 12.0,
  "Status": "Packed"
}
```

---

### 5. Bottle Units Collection
**Collection Name**: `bottles`

**Purpose**: Individual bottle tracking with complete lifecycle

**Schema**:
```csharp
{
  BottleId: string (Primary Key),  // Format: ECO-B-YYYYMMDD-XXXX-YY-ZZZZ
  BatchId: int (Foreign Key -> batches),
  PalletId: int? (Foreign Key -> pallets),
  CartonId: int? (Foreign Key -> cartons),
  
  // Snapshot Specs (copied from batch)
  BottleType: string,
  ContainerType: string,
  ThicknessMicron: int,
  CapacityML: int,
  MaterialGrade: string,
  LiquidType: string,
  ManufactureDate: DateTime,
  ExpiryDate: DateTime,
  
  // Telemetry (captured at production)
  FillTemperature: double,     // °C
  PressureAtSeal: double,      // PSI
  CarbonationLevel: double,    // volumes
  pHValue: double,
  BrixLevel: double,
  CO2Volumes: double,
  
  CapType: string,
  LabelVersion: string,
  
  // Current State
  CurrentStatus: string,       // Produced/InTransit/Retail/Sold/Returned/Recycled
  CurrentLocationType: string, // Plant/Truck/Retail/Consumer/Recovery
  CurrentLocationId: string,
  RecycleCycleCount: int,
  CreatedAt: DateTime
}
```

**Indexes**:
```csharp
bottles.EnsureIndex(x => x.BottleId);
bottles.EnsureIndex(x => x.BatchId);
bottles.EnsureIndex(x => x.PalletId);
bottles.EnsureIndex(x => x.CartonId);
bottles.EnsureIndex(x => x.CurrentStatus);
bottles.EnsureIndex(x => x.CurrentLocationType);
bottles.EnsureIndex(x => x.ExpiryDate);
```

**Example Document**:
```json
{
  "BottleId": "ECO-B-20260221-3130-01-0005",
  "BatchId": 101,
  "PalletId": 1001,
  "CartonId": 5001,
  "BottleType": "Thick",
  "ContainerType": "Bottle",
  "ThicknessMicron": 350,
  "CapacityML": 500,
  "MaterialGrade": "PET-A",
  "LiquidType": "Cola Classic",
  "ManufactureDate": "2026-02-21T08:00:00Z",
  "ExpiryDate": "2026-08-21T23:59:59Z",
  "FillTemperature": 4.5,
  "PressureAtSeal": 65.0,
  "CarbonationLevel": 4.0,
  "pHValue": 2.5,
  "BrixLevel": 11.5,
  "CO2Volumes": 4.0,
  "CapType": "Standard",
  "LabelVersion": "v1.0",
  "CurrentStatus": "Produced",
  "CurrentLocationType": "Plant",
  "CurrentLocationId": "1",
  "RecycleCycleCount": 0,
  "CreatedAt": "2026-02-21T08:15:30Z"
}
```

---

### 6. QR Registry Collection
**Collection Name**: `qr_registry`

**Purpose**: Central registry for all QR codes in the system

**Schema**:
```csharp
{
  QrId: string (Primary Key),     // The QR code itself
  EntityType: string,              // Batch/Pallet/Carton/Bottle
  EntityId: string,                // ID of the entity
  CreatedAt: DateTime
}
```

**Indexes**:
```csharp
qr_registry.EnsureIndex(x => x.QrId);
qr_registry.EnsureIndex(x => x.EntityType);
qr_registry.EnsureIndex(x => x.EntityId);
```

**Example Documents**:
```json
[
  {
    "QrId": "ECO-B-20260221-3130",
    "EntityType": "Batch",
    "EntityId": "101",
    "CreatedAt": "2026-02-21T08:00:00Z"
  },
  {
    "QrId": "ECO-PLT-20260221-0001",
    "EntityType": "Pallet",
    "EntityId": "1001",
    "CreatedAt": "2026-02-21T09:00:00Z"
  },
  {
    "QrId": "ECO-CTN-20260221-0001-01",
    "EntityType": "Carton",
    "EntityId": "5001",
    "CreatedAt": "2026-02-21T09:15:00Z"
  },
  {
    "QrId": "ECO-B-20260221-3130-01-0005",
    "EntityType": "Bottle",
    "EntityId": "ECO-B-20260221-3130-01-0005",
    "CreatedAt": "2026-02-21T09:30:00Z"
  }
]
```

---

### 7. Bottle Movements Collection
**Collection Name**: `movements`

**Purpose**: Track all movements of bottles through the supply chain

**Schema**:
```csharp
{
  Id: int (Primary Key),
  BottleId: string (Foreign Key -> bottles),
  FromLocationType: string,
  FromLocationId: string,
  ToLocationType: string,
  ToLocationId: string,
  EventType: string,           // Produced/Shipped/Retail/Sold/Returned/Recycled
  Timestamp: DateTime,
  UserId: string
}
```

**Indexes**:
```csharp
movements.EnsureIndex(x => x.BottleId);
movements.EnsureIndex(x => x.EventType);
movements.EnsureIndex(x => x.Timestamp);
```

**Example Documents**:
```json
[
  {
    "Id": 1,
    "BottleId": "ECO-B-20260221-3130-01-0005",
    "FromLocationType": "None",
    "FromLocationId": "",
    "ToLocationType": "Plant",
    "ToLocationId": "1",
    "EventType": "Produced",
    "Timestamp": "2026-02-21T08:15:30Z",
    "UserId": "OP-123"
  },
  {
    "Id": 2,
    "BottleId": "ECO-B-20260221-3130-01-0005",
    "FromLocationType": "Plant",
    "FromLocationId": "1",
    "ToLocationType": "Truck",
    "ToLocationId": "TRK-001",
    "EventType": "Shipped",
    "Timestamp": "2026-02-21T14:00:00Z",
    "UserId": "DRIVER-45"
  },
  {
    "Id": 3,
    "BottleId": "ECO-B-20260221-3130-01-0005",
    "FromLocationType": "Truck",
    "FromLocationId": "TRK-001",
    "ToLocationType": "Retail",
    "ToLocationId": "RET-123",
    "EventType": "Retail",
    "Timestamp": "2026-02-22T10:30:00Z",
    "UserId": "DRIVER-45"
  }
]
```

---

### 8. Scan Logs Collection
**Collection Name**: `scan_logs`

**Purpose**: Log all QR code scans for analytics and tracking

**Schema**:
```csharp
{
  Id: int (Primary Key),
  QRCode: string,
  ScannedBy: string,
  ScanLocation: string,
  ScanType: string,            // manual/auto/api
  DeviceId: string,
  Timestamp: DateTime,
  Latitude: double?,
  Longitude: double?
}
```

**Indexes**:
```csharp
scan_logs.EnsureIndex(x => x.QRCode);
scan_logs.EnsureIndex(x => x.Timestamp);
scan_logs.EnsureIndex(x => x.ScannedBy);
```

**Example Document**:
```json
{
  "Id": 1,
  "QRCode": "ECO-B-20260221-3130-01-0005",
  "ScannedBy": "USER-789",
  "ScanLocation": "Retail Store",
  "ScanType": "manual",
  "DeviceId": "MOBILE-123",
  "Timestamp": "2026-02-22T15:45:00Z",
  "Latitude": 19.0760,
  "Longitude": 72.8777
}
```

---

### 9. Shipments Collection
**Collection Name**: `shipments`

**Purpose**: Track shipments from plant to retail

**Schema**:
```csharp
{
  Id: int (Primary Key),
  BatchId: int (Foreign Key -> batches),
  TruckId: string (Foreign Key -> trucks),
  DriverId: string,
  PickupDate: DateTime,
  DispatchTime: DateTime?,
  ExpectedDelivery: DateTime,
  ActualDelivery: DateTime?,
  RouteCode: string,
  Status: string,              // InTransit/Delivered
  
  // IoT Logs
  TemperatureLogId: string,
  HumidityLogId: string,
  ShockLogId: string
}
```

**Indexes**:
```csharp
shipments.EnsureIndex(x => x.BatchId);
shipments.EnsureIndex(x => x.TruckId);
shipments.EnsureIndex(x => x.Status);
```

---

### 10. Retail Locations Collection
**Collection Name**: `retail_locations`

**Purpose**: Store retail location information

**Schema**:
```csharp
{
  Id: int (Primary Key),
  Name: string,
  Address: string,
  City: string,
  Region: string,
  Country: string,
  ContactPerson: string,
  Status: string               // Active/Inactive
}
```

**Indexes**:
```csharp
retail_locations.EnsureIndex(x => x.City);
retail_locations.EnsureIndex(x => x.Status);
```

---

## Database Relationships

### Entity Relationship Diagram

```
Plants (1) ──────┐
                 │
                 ├──> Batches (N)
                 │       │
                 │       ├──> Pallets (N)
                 │       │       │
                 │       │       ├──> Cartons (N)
                 │       │       │       │
                 │       │       │       └──> Bottles (N)
                 │       │       │
                 │       │       └──> Bottles (N)
                 │       │
                 │       └──> Bottles (N)
                 │
                 └──> QR Registry (N)
                         │
                         └──> Scan Logs (N)

Bottles (1) ──> Movements (N)
Bottles (1) ──> Recycling Events (N)

Batches (1) ──> Shipments (N)
Trucks (1) ──> Shipments (N)
```

---

## Performance Optimization

### Recommended Indexes

```csharp
// High-frequency queries
bottles.EnsureIndex(x => x.BottleId);
bottles.EnsureIndex(x => x.BatchId);
bottles.EnsureIndex(x => x.CurrentStatus);

qr_registry.EnsureIndex(x => x.QrId);

movements.EnsureIndex(x => x.BottleId);
movements.EnsureIndex(x => x.Timestamp);

scan_logs.EnsureIndex(x => x.QRCode);
scan_logs.EnsureIndex(x => x.Timestamp);

// Composite indexes for complex queries
batches.EnsureIndex("idx_batch_status_date", "$.Status, $.ManufactureDate");
bottles.EnsureIndex("idx_bottle_status_location", "$.CurrentStatus, $.CurrentLocationType");
```

### Query Optimization Tips

1. **Use indexes for frequent queries**
2. **Limit result sets with pagination**
3. **Use projection to fetch only required fields**
4. **Cache frequently accessed data**
5. **Use batch operations for bulk inserts**

---

## Data Integrity Rules

### Constraints

1. **Batch → Pallet**: A pallet must belong to a valid batch
2. **Pallet → Carton**: A carton must belong to a valid pallet
3. **Carton → Bottle**: A bottle can optionally belong to a carton
4. **QR Registry**: Every QR code must have a unique entry
5. **Movement Log**: Every bottle movement must have valid from/to locations

### Validation Rules

```csharp
// Batch validation
- TotalProducedUnits <= TotalPlannedUnits
- ExpiryDate > ManufactureDate
- QualityStatus in ["Pending", "Approved", "Rejected"]

// Pallet validation
- TotalUnits = CartonCount × UnitsPerCarton
- GrossWeight > NetWeight

// Bottle validation
- ExpiryDate > ManufactureDate
- RecycleCycleCount >= 0
- CurrentStatus in valid status list
```

---

## Backup and Recovery

### Backup Strategy

```csharp
// Daily backup
var backupPath = $"backups/packtrack_{DateTime.Now:yyyyMMdd}.db";
File.Copy("packtrack.db", backupPath);

// Weekly full backup
// Monthly archive
```

### Recovery Procedure

```csharp
// Restore from backup
File.Copy("backups/packtrack_20260221.db", "packtrack.db", overwrite: true);
```

---

## Migration Scripts

### Initial Setup

```csharp
public void InitializeDatabase(LiteDatabase db)
{
    // Create collections
    var plants = db.GetCollection<Plant>("plants");
    var batches = db.GetCollection<ProductionBatch>("batches");
    var pallets = db.GetCollection<Pallet>("pallets");
    var cartons = db.GetCollection<Carton>("cartons");
    var bottles = db.GetCollection<BottleUnit>("bottles");
    var qrRegistry = db.GetCollection<QrRegistry>("qr_registry");
    var movements = db.GetCollection<BottleMovement>("movements");
    var scanLogs = db.GetCollection<ScanLog>("scan_logs");
    
    // Create indexes
    plants.EnsureIndex(x => x.PlantCode);
    batches.EnsureIndex(x => x.BatchCode);
    pallets.EnsureIndex(x => x.PalletCode);
    cartons.EnsureIndex(x => x.CartonCode);
    bottles.EnsureIndex(x => x.BottleId);
    qrRegistry.EnsureIndex(x => x.QrId);
    movements.EnsureIndex(x => x.BottleId);
    scanLogs.EnsureIndex(x => x.QRCode);
}
```

---

## Storage Estimates

### Per Entity Storage

- **Plant**: ~500 bytes
- **Batch**: ~1 KB
- **Pallet**: ~300 bytes
- **Carton**: ~250 bytes
- **Bottle**: ~800 bytes
- **QR Registry**: ~150 bytes
- **Movement**: ~200 bytes
- **Scan Log**: ~250 bytes

### Example Production Run

For a batch of 2,400 bottles:
- 1 Batch: 1 KB
- 10 Pallets: 3 KB
- 100 Cartons: 25 KB
- 2,400 Bottles: 1.9 MB
- 2,400 QR Registry: 360 KB
- Total: ~2.3 MB per batch

### Annual Estimates

- 1,000 batches/year
- 2.4 million bottles/year
- Database size: ~2.3 GB/year
- With movements and scans: ~5-10 GB/year

---

## Security Considerations

### Access Control

```csharp
// Role-based access
- Admin: Full access
- Factory: Read/Write batches, pallets, cartons, bottles
- Transport: Read batches, Update shipments
- Retail: Read bottles, Update movements
- Consumer: Read-only scan access
```

### Data Encryption

```csharp
// Encrypt sensitive fields
- OperatorId
- SupervisorId
- DriverId
- GPS coordinates
```

---

## Monitoring and Maintenance

### Health Checks

```csharp
public class DatabaseHealthCheck
{
    public bool CheckDatabaseHealth()
    {
        // Check database size
        // Check index integrity
        // Check orphaned records
        // Check data consistency
        return true;
    }
}
```

### Cleanup Tasks

```csharp
// Remove old scan logs (> 1 year)
// Archive completed batches (> 2 years)
// Compress movement history
```
