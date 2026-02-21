# QR Code Generation & Printing System - Complete Documentation

## Overview
Comprehensive hierarchical QR code generation system for PackTrack with automatic generation, printing, and tracking capabilities.

## System Architecture

### Hierarchy Structure
```
Batch (1)
  └── Pallets (N)
        └── Cartons (N)
              └── Bottles (N)
```

### QR Code Format Standards

#### 1. Batch QR Code
- **Format**: `ECO-B-YYYYMMDD-XXXX`
- **Example**: `ECO-B-20260221-3130`
- **Components**:
  - `ECO`: Company prefix
  - `B`: Batch identifier
  - `YYYYMMDD`: Production date
  - `XXXX`: 4-digit sequence number

#### 2. Pallet QR Code
- **Format**: `ECO-PLT-YYYYMMDD-XXXX`
- **Example**: `ECO-PLT-20260221-0001`
- **Components**:
  - `ECO`: Company prefix
  - `PLT`: Pallet identifier
  - `YYYYMMDD`: Creation date
  - `XXXX`: 4-digit sequence number

#### 3. Carton QR Code
- **Format**: `ECO-CTN-YYYYMMDD-XXXX-YY`
- **Example**: `ECO-CTN-20260221-0001-01`
- **Components**:
  - `ECO`: Company prefix
  - `CTN`: Carton identifier
  - `YYYYMMDD`: Creation date
  - `XXXX`: 4-digit sequence number
  - `YY`: Carton number within pallet

#### 4. Bottle QR Code
- **Format**: `ECO-B-YYYYMMDD-XXXX-YY-ZZZZ`
- **Example**: `ECO-B-20260221-3130-01-0005`
- **Components**:
  - `ECO`: Company prefix
  - `B`: Batch code
  - `YYYYMMDD`: Production date
  - `XXXX`: Batch sequence
  - `YY`: Group number (per 1000 bottles)
  - `ZZZZ`: Bottle number within group

---

## Service Implementation

### 1. QRCodeGenerationService.cs

Located: `Services/QRCode/QRCodeGenerationService.cs`

#### Key Methods

##### Generate Batch QR
```csharp
public BatchQRResult GenerateBatchQR(int batchId)
```
- Generates unique QR code for production batch
- Updates batch record with QR code
- Registers QR in central registry
- Returns QR image (PNG) and Base64 string

##### Generate Pallet QRs
```csharp
public List<PalletQRResult> GeneratePalletQRs(
    int batchId, 
    int palletCount, 
    int unitsPerPallet
)
```
- Creates multiple pallets for a batch
- Generates unique QR for each pallet
- Links pallets to parent batch
- Returns list of pallet QR results

##### Generate Carton QRs
```csharp
public List<CartonQRResult> GenerateCartonQRs(
    int palletId, 
    int cartonCount, 
    int unitsPerCarton
)
```
- Creates cartons within a pallet
- Generates sequential QR codes
- Links cartons to parent pallet
- Updates pallet carton count

##### Generate Bottle QRs
```csharp
public List<BottleQRResult> GenerateBottleQRs(
    int batchId, 
    int bottleCount
)
```
- Creates individual bottle records
- Generates unique QR for each bottle
- Copies batch specifications to bottle
- Returns list of bottle QR results

##### Generate Complete Hierarchy
```csharp
public CompleteHierarchyQRResult GenerateCompleteHierarchy(
    CompleteHierarchyRequest request
)
```
- **One-stop method** for complete QR generation
- Generates all levels: Batch → Pallets → Cartons → Bottles
- Automatically assigns bottles to cartons
- Returns complete hierarchy with all QR codes

**Request Parameters:**
```csharp
public class CompleteHierarchyRequest
{
    public int BatchId { get; set; }
    public int PalletCount { get; set; }
    public int UnitsPerPallet { get; set; }
    public int CartonsPerPallet { get; set; }
    public int BottlesPerCarton { get; set; }
}
```

**Example Usage:**
```csharp
var request = new CompleteHierarchyRequest
{
    BatchId = 101,
    PalletCount = 10,           // 10 pallets
    UnitsPerPallet = 240,       // 240 bottles per pallet
    CartonsPerPallet = 10,      // 10 cartons per pallet
    BottlesPerCarton = 24       // 24 bottles per carton
};

var result = qrService.GenerateCompleteHierarchy(request);

// Result contains:
// - 1 Batch QR
// - 10 Pallet QRs
// - 100 Carton QRs (10 pallets × 10 cartons)
// - 2,400 Bottle QRs (10 pallets × 240 bottles)
```

---

### 2. QRCodePrintService.cs

Located: `Services/QRCode/QRCodePrintService.cs`

#### Label Types

##### Pallet Label (A4 Size)
- **Size**: 210mm × 297mm (A4)
- **Content**:
  - Large QR code (300×300 pixels)
  - Batch information table
  - Pallet details
  - Barcode
  - Company logo
  - Manufacture/Expiry dates
  - Weight information

**Method:**
```csharp
public byte[] GeneratePalletLabel(PalletLabelData data)
```

**Batch Printing:**
```csharp
public byte[] GenerateBatchPalletLabels(List<PalletLabelData> pallets)
```

##### Carton Label (10cm × 15cm)
- **Size**: 100mm × 150mm
- **Content**:
  - Medium QR code (150×150 pixels)
  - Carton code
  - Pallet reference
  - Unit count
  - Product type

**Method:**
```csharp
public byte[] GenerateCartonLabel(CartonLabelData data)
```

**Sheet Printing (6 per page):**
```csharp
public byte[] GenerateCartonLabelsSheet(List<CartonLabelData> cartons)
```
- 2 columns × 3 rows per A4 page
- Optimized for label printers

##### Bottle Label (Small Sticker)
- **Size**: ~50mm × 50mm
- **Content**:
  - Small QR code (40×40 pixels)
  - Bottle ID (last 8 digits)
  - Capacity
  - Minimal design for small bottles

**Sheet Printing (24 per page):**
```csharp
public byte[] GenerateBottleLabelsSheet(List<BottleLabelData> bottles)
```
- 4 columns × 6 rows per A4 page
- Suitable for sticker sheets

#### Printer Integration

**Get Available Printers:**
```csharp
public List<string> GetAvailablePrinters()
```

**Send to Printer:**
```csharp
public async Task<bool> SendToPrinter(byte[] pdfData, string printerName)
```

---

## Usage Examples

### Example 1: Create Complete Production Run

```csharp
// 1. Create batch
var batch = new ProductionBatch
{
    PlantId = 1,
    BottleType = "Thick",
    CapacityML = 500,
    LiquidType = "Cola Classic",
    TotalPlannedUnits = 2400,
    ManufactureDate = DateTime.Now,
    ExpiryDate = DateTime.Now.AddMonths(6)
};
var batchId = SaveBatch(batch);

// 2. Generate complete QR hierarchy
var qrService = new QRCodeGenerationService(context, printService);
var request = new CompleteHierarchyRequest
{
    BatchId = batchId,
    PalletCount = 10,
    UnitsPerPallet = 240,
    CartonsPerPallet = 10,
    BottlesPerCarton = 24
};

var result = qrService.GenerateCompleteHierarchy(request);

// 3. Print pallet labels
var printService = new QRCodePrintService();
var palletLabels = result.PalletQRs.Select(p => new PalletLabelData
{
    PalletCode = p.PalletCode,
    QRCode = p.QRCode,
    QRCodeImage = Convert.FromBase64String(p.QRImageBase64),
    BatchCode = result.BatchQR.BatchCode,
    BottleType = batch.BottleType,
    CapacityML = batch.CapacityML,
    TotalUnits = p.TotalUnits,
    ManufactureDate = batch.ManufactureDate,
    ExpiryDate = batch.ExpiryDate
}).ToList();

var palletPdf = printService.GenerateBatchPalletLabels(palletLabels);
await File.WriteAllBytesAsync("pallet_labels.pdf", palletPdf);

// 4. Print carton labels
var cartonLabels = result.CartonQRs.Select(c => new CartonLabelData
{
    CartonCode = c.CartonCode,
    QRCode = c.QRCode,
    QRCodeImage = Convert.FromBase64String(c.QRImageBase64),
    PalletCode = c.PalletCode,
    BottleType = batch.BottleType,
    CapacityML = batch.CapacityML,
    UnitsPerCarton = c.UnitsPerCarton
}).ToList();

var cartonPdf = printService.GenerateCartonLabelsSheet(cartonLabels);
await File.WriteAllBytesAsync("carton_labels.pdf", cartonPdf);

// 5. Print bottle labels
var bottleLabels = result.BottleQRs.Select(b => new BottleLabelData
{
    BottleId = b.BottleId,
    QRCodeImage = Convert.FromBase64String(b.QRImageBase64),
    BottleType = b.BottleType,
    CapacityML = b.CapacityML
}).ToList();

var bottlePdf = printService.GenerateBottleLabelsSheet(bottleLabels);
await File.WriteAllBytesAsync("bottle_labels.pdf", bottlePdf);

Console.WriteLine($"Generated {result.TotalPallets} pallets, {result.TotalCartons} cartons, {result.TotalBottles} bottles");
```

### Example 2: Generate Only Pallet QRs

```csharp
var qrService = new QRCodeGenerationService(context, printService);

// Generate 5 pallets with 240 units each
var palletResults = qrService.GeneratePalletQRs(
    batchId: 101,
    palletCount: 5,
    unitsPerPallet: 240
);

foreach (var pallet in palletResults)
{
    Console.WriteLine($"Pallet {pallet.SequenceNumber}: {pallet.QRCode}");
    Console.WriteLine($"  Image: {pallet.QRImagePath}");
}
```

### Example 3: Generate Single Pallet with Cartons

```csharp
var qrService = new QRCodeGenerationService(context, printService);

// 1. Generate single pallet
var palletResult = qrService.GenerateSinglePalletQR(
    batchId: 101,
    unitsPerPallet: 240
);

// 2. Generate cartons for this pallet
var cartonResults = qrService.GenerateCartonQRs(
    palletId: palletResult.PalletId,
    cartonCount: 10,
    unitsPerCarton: 24
);

// 3. Generate bottles and assign to cartons
var cartonIds = cartonResults.Select(c => c.CartonId).ToList();
var bottleResults = qrService.GenerateBottleQRsWithCartonAssignment(
    batchId: 101,
    cartonIds: cartonIds,
    bottlesPerCarton: 24
);

Console.WriteLine($"Created 1 pallet with {cartonResults.Count} cartons and {bottleResults.Count} bottles");
```

---

## API Endpoints

### Generate Complete Hierarchy
```http
POST /api/qr/generate-hierarchy
Content-Type: application/json

{
  "batchId": 101,
  "palletCount": 10,
  "unitsPerPallet": 240,
  "cartonsPerPallet": 10,
  "bottlesPerCarton": 24
}
```

**Response:**
```json
{
  "batchId": 101,
  "batchQR": {
    "batchCode": "B-20260221-3130",
    "qrCode": "ECO-B-20260221-3130",
    "qrImagePath": "/qrcodes/Batch/2026-02-21/ECO-B-20260221-3130.png"
  },
  "totalPallets": 10,
  "totalCartons": 100,
  "totalBottles": 2400,
  "generatedAt": "2026-02-21T14:30:00Z"
}
```

### Print Pallet Labels
```http
POST /api/qr/print-pallet-labels
Content-Type: application/json

{
  "palletIds": [1, 2, 3, 4, 5],
  "printerName": "Zebra-ZT230",
  "autoprint": true
}
```

### Print Carton Labels Sheet
```http
POST /api/qr/print-carton-labels
Content-Type: application/json

{
  "cartonIds": [1, 2, 3, 4, 5, 6],
  "format": "sheet",
  "printerName": "HP-LaserJet"
}
```

---

## Database Schema

### QR Registry Table
```sql
CREATE TABLE qr_registry (
    qr_id VARCHAR(100) PRIMARY KEY,
    entity_type VARCHAR(50) NOT NULL,
    entity_id VARCHAR(100) NOT NULL,
    created_at DATETIME NOT NULL,
    INDEX idx_entity (entity_type, entity_id)
);
```

### Batch Table (Extended)
```sql
ALTER TABLE batches ADD COLUMN batch_code VARCHAR(50);
ALTER TABLE batches ADD COLUMN qr_generated BOOLEAN DEFAULT FALSE;
ALTER TABLE batches ADD COLUMN qr_generated_at DATETIME;
```

### Pallet Table (Extended)
```sql
ALTER TABLE pallets ADD COLUMN pallet_code VARCHAR(50);
ALTER TABLE pallets ADD COLUMN qr_code VARCHAR(100);
ALTER TABLE pallets ADD COLUMN label_printed BOOLEAN DEFAULT FALSE;
ALTER TABLE pallets ADD COLUMN label_printed_at DATETIME;
```

### Carton Table (Extended)
```sql
ALTER TABLE cartons ADD COLUMN carton_code VARCHAR(50);
ALTER TABLE cartons ADD COLUMN qr_code VARCHAR(100);
ALTER TABLE cartons ADD COLUMN label_printed BOOLEAN DEFAULT FALSE;
```

---

## Label Specifications

### Pallet Label Dimensions
- **Paper Size**: A4 (210mm × 297mm)
- **QR Code Size**: 300×300 pixels
- **Margins**: 20mm all sides
- **Font**: Arial, Bold for headers
- **Barcode**: Code 128, 400×60 pixels

### Carton Label Dimensions
- **Label Size**: 100mm × 150mm
- **QR Code Size**: 150×150 pixels
- **Margins**: 10mm all sides
- **Font**: Arial, 9-10pt
- **Layout**: Portrait orientation

### Bottle Label Dimensions
- **Label Size**: 50mm × 50mm
- **QR Code Size**: 40×40 pixels
- **Margins**: 3mm all sides
- **Font**: Arial, 6-8pt
- **Layout**: Minimal design

---

## Printer Recommendations

### For Pallet Labels
- **Zebra ZT230** - Industrial thermal printer
- **Brother QL-1110NWB** - Wide format label printer
- **Dymo LabelWriter 4XL** - Large format labels

### For Carton Labels
- **Zebra GK420d** - Desktop thermal printer
- **Rollo Label Printer** - Commercial grade
- **MUNBYN Label Printer** - Cost-effective option

### For Bottle Labels
- **Zebra ZD410** - Compact desktop printer
- **TSC TTP-225** - High-speed label printer
- **Godex G500** - Industrial label printer

---

## Performance Considerations

### Generation Speed
- **Batch QR**: < 100ms
- **Pallet QR (10 units)**: < 500ms
- **Carton QR (100 units)**: < 2 seconds
- **Bottle QR (2400 units)**: < 10 seconds
- **Complete Hierarchy**: < 15 seconds

### Storage Requirements
- **QR Image (PNG)**: ~5-10 KB each
- **Pallet Label (PDF)**: ~50-100 KB
- **Carton Sheet (PDF)**: ~200-300 KB
- **Bottle Sheet (PDF)**: ~500-800 KB

### Optimization Tips
1. Generate QR codes in batches
2. Use async/await for large operations
3. Cache QR images in memory
4. Use background jobs for printing
5. Compress PDF files for storage

---

## Error Handling

### Common Errors

**Duplicate QR Code:**
```csharp
try {
    var result = qrService.GenerateBatchQR(batchId);
} catch (DuplicateQRException ex) {
    // Handle duplicate QR code
    Console.WriteLine($"QR already exists: {ex.QRCode}");
}
```

**Printer Not Found:**
```csharp
var printers = printService.GetAvailablePrinters();
if (!printers.Contains(printerName)) {
    throw new PrinterNotFoundException($"Printer {printerName} not found");
}
```

**Invalid Hierarchy:**
```csharp
if (request.BottlesPerCarton * request.CartonsPerPallet != request.UnitsPerPallet) {
    throw new InvalidHierarchyException("Units mismatch in hierarchy");
}
```

---

## Testing

### Unit Tests
```csharp
[Fact]
public void GenerateBatchQR_ShouldCreateUniqueCode()
{
    var service = new QRCodeGenerationService(context, printService);
    var result = service.GenerateBatchQR(101);
    
    Assert.NotNull(result.QRCode);
    Assert.StartsWith("ECO-B-", result.QRCode);
}

[Fact]
public void GenerateCompleteHierarchy_ShouldCreateCorrectCounts()
{
    var request = new CompleteHierarchyRequest
    {
        BatchId = 101,
        PalletCount = 2,
        CartonsPerPallet = 5,
        BottlesPerCarton = 10
    };
    
    var result = service.GenerateCompleteHierarchy(request);
    
    Assert.Equal(2, result.TotalPallets);
    Assert.Equal(10, result.TotalCartons);
    Assert.Equal(100, result.TotalBottles);
}
```

---

## Future Enhancements

1. **RFID Integration** - Add RFID tags alongside QR codes
2. **NFC Support** - Enable NFC reading for mobile devices
3. **Blockchain** - Store QR registry on blockchain
4. **AI Verification** - Use AI to verify QR code quality
5. **Cloud Printing** - Support cloud-based printing services
6. **Multi-language** - Support labels in multiple languages
7. **Custom Templates** - Allow custom label designs
8. **Batch Tracking** - Real-time tracking dashboard

---

## Dependencies

### NuGet Packages Required
```xml
<PackageReference Include="QRCoder" Version="1.4.3" />
<PackageReference Include="QuestPDF" Version="2023.12.0" />
<PackageReference Include="System.Drawing.Common" Version="8.0.0" />
<PackageReference Include="LiteDB" Version="5.0.17" />
```

### Installation
```bash
dotnet add package QRCoder
dotnet add package QuestPDF
dotnet add package System.Drawing.Common
```
