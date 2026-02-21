# PackTrack File System Structure - Complete Documentation

## Overview
Complete file system organization for QR codes, labels, and related assets in the PackTrack system.

---

## Root Directory Structure

```
D:\PackTrack\
├── wwwroot/
│   ├── qrcodes/              # QR code images
│   ├── labels/               # Generated label PDFs
│   ├── assets/               # Static assets (logos, icons)
│   └── temp/                 # Temporary files
├── backups/                  # Database backups
├── logs/                     # Application logs
├── exports/                  # Data exports
└── uploads/                  # User uploads
```

---

## QR Codes Directory Structure

### Organization Pattern

```
wwwroot/qrcodes/
├── Batch/
│   ├── 2026-02-21/
│   │   ├── ECO-B-20260221-3130.png
│   │   ├── ECO-B-20260221-3131.png
│   │   └── ...
│   ├── 2026-02-22/
│   └── ...
├── Pallet/
│   ├── 2026-02-21/
│   │   ├── ECO-PLT-20260221-0001.png
│   │   ├── ECO-PLT-20260221-0002.png
│   │   └── ...
│   ├── 2026-02-22/
│   └── ...
├── Carton/
│   ├── 2026-02-21/
│   │   ├── ECO-CTN-20260221-0001-01.png
│   │   ├── ECO-CTN-20260221-0001-02.png
│   │   └── ...
│   ├── 2026-02-22/
│   └── ...
└── Bottle/
    ├── 2026-02-21/
    │   ├── ECO-B-20260221-3130-01-0001.png
    │   ├── ECO-B-20260221-3130-01-0002.png
    │   └── ...
    ├── 2026-02-22/
    └── ...
```

### Directory Naming Convention

**Pattern**: `{EntityType}/{YYYY-MM-DD}/`

**Examples**:
- `Batch/2026-02-21/`
- `Pallet/2026-02-21/`
- `Carton/2026-02-21/`
- `Bottle/2026-02-21/`

### File Naming Convention

**Pattern**: `{QR-Code}.png`

**Examples**:
- `ECO-B-20260221-3130.png`
- `ECO-PLT-20260221-0001.png`
- `ECO-CTN-20260221-0001-01.png`
- `ECO-B-20260221-3130-01-0005.png`

### File Specifications

| Entity Type | Image Size | File Size | Format |
|-------------|------------|-----------|--------|
| Batch       | 300×300 px | ~8-10 KB  | PNG    |
| Pallet      | 250×250 px | ~6-8 KB   | PNG    |
| Carton      | 200×200 px | ~5-7 KB   | PNG    |
| Bottle      | 150×150 px | ~4-6 KB   | PNG    |

---

## Labels Directory Structure

### Organization Pattern

```
wwwroot/labels/
├── Pallet/
│   ├── 2026-02-21/
│   │   ├── batch-101-pallet-labels.pdf
│   │   ├── pallet-1001-label.pdf
│   │   └── ...
│   ├── 2026-02-22/
│   └── ...
├── Carton/
│   ├── 2026-02-21/
│   │   ├── batch-101-carton-sheet-1.pdf
│   │   ├── batch-101-carton-sheet-2.pdf
│   │   └── ...
│   ├── 2026-02-22/
│   └── ...
└── Bottle/
    ├── 2026-02-21/
    │   ├── batch-101-bottle-sheet-1.pdf
    │   ├── batch-101-bottle-sheet-2.pdf
    │   └── ...
    ├── 2026-02-22/
    └── ...
```

### File Naming Convention

**Pallet Labels**:
- Single: `pallet-{palletId}-label.pdf`
- Batch: `batch-{batchId}-pallet-labels.pdf`

**Carton Labels**:
- Sheet: `batch-{batchId}-carton-sheet-{sheetNumber}.pdf`
- Single: `carton-{cartonId}-label.pdf`

**Bottle Labels**:
- Sheet: `batch-{batchId}-bottle-sheet-{sheetNumber}.pdf`

### File Specifications

| Label Type | Page Size | File Size | Labels per Page |
|------------|-----------|-----------|-----------------|
| Pallet     | A4        | ~50-100 KB| 1               |
| Carton     | A4        | ~200-300 KB| 6              |
| Bottle     | A4        | ~500-800 KB| 24             |

---

## Assets Directory Structure

```
wwwroot/assets/
├── logos/
│   ├── ecocola-logo.png
│   ├── ecocola-logo-white.png
│   └── plant-logos/
│       ├── mumbai-plant.png
│       └── delhi-plant.png
├── icons/
│   ├── qr-icon.svg
│   ├── bottle-icon.svg
│   └── recycle-icon.svg
├── templates/
│   ├── pallet-label-template.pdf
│   ├── carton-label-template.pdf
│   └── bottle-label-template.pdf
└── fonts/
    ├── Arial.ttf
    ├── Arial-Bold.ttf
    └── Barcode.ttf
```

---

## Temporary Files Directory

```
wwwroot/temp/
├── qr-generation/
│   ├── batch-101-temp/
│   │   ├── temp-qr-1.png
│   │   └── ...
│   └── ...
├── label-generation/
│   ├── batch-101-temp/
│   │   ├── temp-label-1.pdf
│   │   └── ...
│   └── ...
└── uploads/
    └── ...
```

### Cleanup Policy

- **Retention**: 24 hours
- **Cleanup Schedule**: Daily at 2:00 AM
- **Cleanup Script**:

```csharp
public void CleanupTempFiles()
{
    var tempDir = Path.Combine("wwwroot", "temp");
    var cutoffDate = DateTime.Now.AddHours(-24);
    
    foreach (var file in Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories))
    {
        var fileInfo = new FileInfo(file);
        if (fileInfo.CreationTime < cutoffDate)
        {
            File.Delete(file);
        }
    }
}
```

---

## Backups Directory Structure

```
backups/
├── database/
│   ├── daily/
│   │   ├── packtrack_20260221.db
│   │   ├── packtrack_20260222.db
│   │   └── ...
│   ├── weekly/
│   │   ├── packtrack_week08_2026.db
│   │   └── ...
│   └── monthly/
│       ├── packtrack_202602.db
│       └── ...
├── qrcodes/
│   ├── 2026-02/
│   │   ├── qrcodes_20260221.zip
│   │   └── ...
│   └── ...
└── labels/
    ├── 2026-02/
    │   ├── labels_20260221.zip
    │   └── ...
    └── ...
```

### Backup Schedule

| Type     | Frequency | Retention | Location                    |
|----------|-----------|-----------|----------------------------|
| Database | Daily     | 30 days   | backups/database/daily/    |
| Database | Weekly    | 12 weeks  | backups/database/weekly/   |
| Database | Monthly   | 24 months | backups/database/monthly/  |
| QR Codes | Weekly    | 12 weeks  | backups/qrcodes/           |
| Labels   | Monthly   | 12 months | backups/labels/            |

### Backup Script

```csharp
public class BackupService
{
    public void PerformDailyBackup()
    {
        var date = DateTime.Now.ToString("yyyyMMdd");
        var backupPath = Path.Combine("backups", "database", "daily", $"packtrack_{date}.db");
        
        File.Copy("packtrack.db", backupPath, overwrite: true);
        
        // Cleanup old daily backups (> 30 days)
        CleanupOldBackups("backups/database/daily", 30);
    }
    
    public void PerformWeeklyBackup()
    {
        var week = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
            DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        var year = DateTime.Now.Year;
        var backupPath = Path.Combine("backups", "database", "weekly", $"packtrack_week{week:D2}_{year}.db");
        
        File.Copy("packtrack.db", backupPath, overwrite: true);
        
        // Cleanup old weekly backups (> 12 weeks)
        CleanupOldBackups("backups/database/weekly", 84);
    }
    
    public void PerformMonthlyBackup()
    {
        var month = DateTime.Now.ToString("yyyyMM");
        var backupPath = Path.Combine("backups", "database", "monthly", $"packtrack_{month}.db");
        
        File.Copy("packtrack.db", backupPath, overwrite: true);
        
        // Archive QR codes and labels
        ArchiveQRCodes(month);
        ArchiveLabels(month);
    }
    
    private void CleanupOldBackups(string directory, int retentionDays)
    {
        var cutoffDate = DateTime.Now.AddDays(-retentionDays);
        
        foreach (var file in Directory.GetFiles(directory))
        {
            var fileInfo = new FileInfo(file);
            if (fileInfo.CreationTime < cutoffDate)
            {
                File.Delete(file);
            }
        }
    }
    
    private void ArchiveQRCodes(string month)
    {
        var sourceDir = Path.Combine("wwwroot", "qrcodes");
        var archivePath = Path.Combine("backups", "qrcodes", month.Substring(0, 7), $"qrcodes_{month}.zip");
        
        // Create zip archive
        ZipFile.CreateFromDirectory(sourceDir, archivePath);
    }
    
    private void ArchiveLabels(string month)
    {
        var sourceDir = Path.Combine("wwwroot", "labels");
        var archivePath = Path.Combine("backups", "labels", month.Substring(0, 7), $"labels_{month}.zip");
        
        // Create zip archive
        ZipFile.CreateFromDirectory(sourceDir, archivePath);
    }
}
```

---

## Logs Directory Structure

```
logs/
├── application/
│   ├── 2026-02-21.log
│   ├── 2026-02-22.log
│   └── ...
├── qr-generation/
│   ├── 2026-02-21.log
│   └── ...
├── label-printing/
│   ├── 2026-02-21.log
│   └── ...
├── api/
│   ├── 2026-02-21.log
│   └── ...
└── errors/
    ├── 2026-02-21.log
    └── ...
```

### Log File Format

**Filename**: `{YYYY-MM-DD}.log`

**Content Format**:
```
[2026-02-21 14:30:15] [INFO] QR generation started for batch 101
[2026-02-21 14:30:18] [INFO] Generated 2400 bottle QR codes
[2026-02-21 14:30:20] [INFO] QR generation completed successfully
[2026-02-21 14:35:10] [ERROR] Failed to print pallet label: Printer not found
```

### Log Retention

- **Application Logs**: 90 days
- **QR Generation Logs**: 60 days
- **Label Printing Logs**: 60 days
- **API Logs**: 30 days
- **Error Logs**: 180 days

---

## Exports Directory Structure

```
exports/
├── batches/
│   ├── batch-101-export-20260221.csv
│   └── ...
├── bottles/
│   ├── bottles-export-20260221.csv
│   └── ...
├── movements/
│   ├── movements-export-20260221.csv
│   └── ...
└── analytics/
    ├── traceability-report-20260221.pdf
    └── ...
```

### Export File Formats

| Export Type | Format | Naming Convention |
|-------------|--------|-------------------|
| Batch Data  | CSV    | batch-{batchId}-export-{date}.csv |
| Bottle Data | CSV    | bottles-export-{date}.csv |
| Movements   | CSV    | movements-export-{date}.csv |
| Analytics   | PDF    | traceability-report-{date}.pdf |

---

## Storage Management

### Disk Space Monitoring

```csharp
public class StorageMonitor
{
    public StorageStats GetStorageStats()
    {
        var stats = new StorageStats();
        
        // QR codes
        stats.QRCodesSize = GetDirectorySize("wwwroot/qrcodes");
        stats.QRCodesCount = GetFileCount("wwwroot/qrcodes");
        
        // Labels
        stats.LabelsSize = GetDirectorySize("wwwroot/labels");
        stats.LabelsCount = GetFileCount("wwwroot/labels");
        
        // Backups
        stats.BackupsSize = GetDirectorySize("backups");
        
        // Logs
        stats.LogsSize = GetDirectorySize("logs");
        
        // Total
        stats.TotalSize = stats.QRCodesSize + stats.LabelsSize + stats.BackupsSize + stats.LogsSize;
        
        return stats;
    }
    
    private long GetDirectorySize(string path)
    {
        var dirInfo = new DirectoryInfo(path);
        return dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
    }
    
    private int GetFileCount(string path)
    {
        return Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
    }
}

public class StorageStats
{
    public long QRCodesSize { get; set; }
    public int QRCodesCount { get; set; }
    public long LabelsSize { get; set; }
    public int LabelsCount { get; set; }
    public long BackupsSize { get; set; }
    public long LogsSize { get; set; }
    public long TotalSize { get; set; }
    
    public string QRCodesSizeFormatted => FormatBytes(QRCodesSize);
    public string LabelsSizeFormatted => FormatBytes(LabelsSize);
    public string BackupsSizeFormatted => FormatBytes(BackupsSize);
    public string LogsSizeFormatted => FormatBytes(LogsSize);
    public string TotalSizeFormatted => FormatBytes(TotalSize);
    
    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
```

### Storage Estimates

#### Per Production Run (2,400 bottles)

| Category    | Size      | Count |
|-------------|-----------|-------|
| QR Codes    | ~15 MB    | 2,511 |
| Labels      | ~2 MB     | ~15   |
| Total       | ~17 MB    | -     |

#### Annual Estimates (1,000 batches)

| Category    | Size      | Count       |
|-------------|-----------|-------------|
| QR Codes    | ~15 GB    | 2,511,000   |
| Labels      | ~2 GB     | ~15,000     |
| Backups     | ~10 GB    | -           |
| Logs        | ~1 GB     | -           |
| Total       | ~28 GB    | -           |

### Cleanup Strategies

#### 1. Archive Old QR Codes

```csharp
public void ArchiveOldQRCodes(int monthsOld)
{
    var cutoffDate = DateTime.Now.AddMonths(-monthsOld);
    var qrCodesDir = Path.Combine("wwwroot", "qrcodes");
    
    foreach (var entityDir in Directory.GetDirectories(qrCodesDir))
    {
        foreach (var dateDir in Directory.GetDirectories(entityDir))
        {
            var dirName = Path.GetFileName(dateDir);
            if (DateTime.TryParse(dirName, out var dirDate) && dirDate < cutoffDate)
            {
                // Archive to zip
                var archivePath = Path.Combine("backups", "qrcodes", $"{dirName}.zip");
                ZipFile.CreateFromDirectory(dateDir, archivePath);
                
                // Delete original
                Directory.Delete(dateDir, recursive: true);
            }
        }
    }
}
```

#### 2. Delete Old Labels

```csharp
public void DeleteOldLabels(int monthsOld)
{
    var cutoffDate = DateTime.Now.AddMonths(-monthsOld);
    var labelsDir = Path.Combine("wwwroot", "labels");
    
    foreach (var entityDir in Directory.GetDirectories(labelsDir))
    {
        foreach (var dateDir in Directory.GetDirectories(entityDir))
        {
            var dirName = Path.GetFileName(dateDir);
            if (DateTime.TryParse(dirName, out var dirDate) && dirDate < cutoffDate)
            {
                Directory.Delete(dateDir, recursive: true);
            }
        }
    }
}
```

#### 3. Rotate Logs

```csharp
public void RotateLogs(int daysOld)
{
    var cutoffDate = DateTime.Now.AddDays(-daysOld);
    var logsDir = "logs";
    
    foreach (var categoryDir in Directory.GetDirectories(logsDir))
    {
        foreach (var logFile in Directory.GetFiles(categoryDir, "*.log"))
        {
            var fileInfo = new FileInfo(logFile);
            if (fileInfo.CreationTime < cutoffDate)
            {
                File.Delete(logFile);
            }
        }
    }
}
```

---

## Access Permissions

### Directory Permissions

| Directory          | Read | Write | Execute |
|-------------------|------|-------|---------|
| wwwroot/qrcodes   | All  | App   | -       |
| wwwroot/labels    | All  | App   | -       |
| wwwroot/assets    | All  | Admin | -       |
| wwwroot/temp      | App  | App   | -       |
| backups           | Admin| App   | -       |
| logs              | Admin| App   | -       |
| exports           | Admin| App   | -       |

### File Permissions

- **QR Code Images**: Read-only for web access
- **Label PDFs**: Read-only for web access
- **Database Backups**: Admin access only
- **Log Files**: Admin access only

---

## CDN Integration (Optional)

### For Production Deployment

```csharp
public class CDNService
{
    private readonly string _cdnBaseUrl = "https://cdn.ecocola.com";
    
    public string GetQRCodeUrl(string qrCode, string entityType)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        return $"{_cdnBaseUrl}/qrcodes/{entityType}/{date}/{qrCode}.png";
    }
    
    public string GetLabelUrl(string labelFileName)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        return $"{_cdnBaseUrl}/labels/{date}/{labelFileName}";
    }
    
    public async Task UploadToCDN(string localPath, string cdnPath)
    {
        // Upload file to CDN
        // Implementation depends on CDN provider (AWS S3, Azure Blob, etc.)
    }
}
```

---

## Monitoring and Alerts

### Storage Alerts

```csharp
public class StorageAlertService
{
    public void CheckStorageThresholds()
    {
        var stats = new StorageMonitor().GetStorageStats();
        
        // Alert if QR codes exceed 50 GB
        if (stats.QRCodesSize > 50L * 1024 * 1024 * 1024)
        {
            SendAlert("QR codes storage exceeds 50 GB");
        }
        
        // Alert if total storage exceeds 100 GB
        if (stats.TotalSize > 100L * 1024 * 1024 * 1024)
        {
            SendAlert("Total storage exceeds 100 GB");
        }
    }
    
    private void SendAlert(string message)
    {
        // Send email/SMS alert
        Console.WriteLine($"ALERT: {message}");
    }
}
```

---

## Best Practices

### 1. Directory Organization
- Use date-based subdirectories for easy cleanup
- Separate by entity type for better organization
- Keep consistent naming conventions

### 2. File Management
- Compress old files to save space
- Archive instead of delete for compliance
- Use meaningful file names

### 3. Backup Strategy
- Multiple backup frequencies (daily, weekly, monthly)
- Store backups in separate location
- Test restore procedures regularly

### 4. Performance
- Use CDN for static assets in production
- Implement caching for frequently accessed files
- Optimize image sizes

### 5. Security
- Restrict write access to application only
- Use HTTPS for file access
- Implement access logging

---

## Maintenance Schedule

| Task                    | Frequency | Time      |
|-------------------------|-----------|-----------|
| Cleanup temp files      | Daily     | 2:00 AM   |
| Daily database backup   | Daily     | 3:00 AM   |
| Weekly QR archive       | Weekly    | Sunday 4:00 AM |
| Monthly label archive   | Monthly   | 1st, 5:00 AM |
| Log rotation            | Daily     | 2:30 AM   |
| Storage monitoring      | Daily     | 6:00 AM   |
| Backup verification     | Weekly    | Sunday 10:00 AM |
