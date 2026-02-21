# Traceability UI Components - Blazor Documentation

## Overview
Comprehensive Blazor UI components for EcoCola Traceability Mesh visualization and QR scanning.

---

## Component Architecture

```
Components/Traceability/
├── QRScanner.razor              # QR code scanner with camera
├── TraceabilityChain.razor      # Hierarchical chain visualization
├── MovementTimeline.razor       # Movement history timeline
├── AnalyticsDashboard.razor     # Batch analytics dashboard
├── RealTimeMap.razor            # Real-time tracking map
└── Shared/
    ├── QRCodeDisplay.razor      # QR code display component
    ├── StatusBadge.razor        # Status indicator badge
    └── LocationPin.razor        # Location marker
```

---

## 1. QR Scanner Component

### QRScanner.razor

```razor
@using PackTrack.Services.Traceability
@inject TraceabilityMeshService TraceabilityService
@inject IJSRuntime JS

<div class="qr-scanner-container">
    <div class="scanner-header">
        <h3>Scan QR Code</h3>
        <button @onclick="ToggleCamera" class="btn btn-primary">
            @(isCameraActive ? "Stop Camera" : "Start Camera")
        </button>
    </div>
    
    @if (isCameraActive)
    {
        <div class="camera-view">
            <video id="qr-video" autoplay playsinline></video>
            <div class="scan-overlay">
                <div class="scan-frame"></div>
            </div>
        </div>
    }
    else
    {
        <div class="manual-input">
            <input type="text" @bind="manualQRCode" placeholder="Enter QR code manually" class="form-control" />
            <button @onclick="ScanManualCode" class="btn btn-success">Scan</button>
        </div>
    }
    
    @if (scanResult != null)
    {
        <div class="scan-result @(scanResult.Success ? "success" : "error")">
            @if (scanResult.Success)
            {
                <h4>✓ Scan Successful</h4>
                <p><strong>QR Code:</strong> @scanResult.QRCode</p>
                <p><strong>Entity Type:</strong> @scanResult.EntityType</p>
                <button @onclick="ViewDetails" class="btn btn-info">View Details</button>
            }
            else
            {
                <h4>✗ Scan Failed</h4>
                <p>@scanResult.ErrorMessage</p>
            }
        </div>
    }
</div>

@code {
    private bool isCameraActive = false;
    private string manualQRCode = "";
    private TraceabilityScanResult? scanResult;
    
    [Parameter]
    public EventCallback<TraceabilityScanResult> OnScanComplete { get; set; }
    
    private async Task ToggleCamera()
    {
        isCameraActive = !isCameraActive;
        
        if (isCameraActive)
        {
            await JS.InvokeVoidAsync("startQRScanner", DotNetObjectReference.Create(this));
        }
        else
        {
            await JS.InvokeVoidAsync("stopQRScanner");
        }
    }
    
    [JSInvokable]
    public async Task OnQRCodeDetected(string qrCode)
    {
        await ScanQRCode(qrCode);
    }
    
    private async Task ScanManualCode()
    {
        if (!string.IsNullOrEmpty(manualQRCode))
        {
            await ScanQRCode(manualQRCode);
        }
    }
    
    private async Task ScanQRCode(string qrCode)
    {
        var scanContext = new ScanContext
        {
            UserId = "current-user",
            Location = "Web App",
            ScanType = isCameraActive ? "camera" : "manual",
            DeviceId = "web-browser"
        };
        
        scanResult = TraceabilityService.ScanQRCode(qrCode, scanContext);
        
        if (scanResult.Success)
        {
            await OnScanComplete.InvokeAsync(scanResult);
        }
    }
    
    private async Task ViewDetails()
    {
        // Navigate to details page or show modal
    }
}
```

### JavaScript (wwwroot/js/qr-scanner.js)

```javascript
let qrScanner = null;

window.startQRScanner = async (dotNetHelper) => {
    const video = document.getElementById('qr-video');
    
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ 
            video: { facingMode: 'environment' } 
        });
        video.srcObject = stream;
        
        // Use jsQR library for QR detection
        qrScanner = setInterval(() => {
            const canvas = document.createElement('canvas');
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            const ctx = canvas.getContext('2d');
            ctx.drawImage(video, 0, 0);
            
            const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
            const code = jsQR(imageData.data, imageData.width, imageData.height);
            
            if (code) {
                dotNetHelper.invokeMethodAsync('OnQRCodeDetected', code.data);
                stopQRScanner();
            }
        }, 100);
    } catch (error) {
        console.error('Camera access denied:', error);
    }
};

window.stopQRScanner = () => {
    if (qrScanner) {
        clearInterval(qrScanner);
        qrScanner = null;
    }
    
    const video = document.getElementById('qr-video');
    if (video && video.srcObject) {
        video.srcObject.getTracks().forEach(track => track.stop());
    }
};
```

---

## 2. Traceability Chain Component

### TraceabilityChain.razor

```razor
@using PackTrack.Services.Traceability

<div class="traceability-chain">
    <h3>Product Journey</h3>
    
    @if (Chain != null)
    {
        <div class="chain-visualization">
            @if (Chain.Batch != null)
            {
                <div class="chain-level batch-level">
                    <div class="level-icon">📦</div>
                    <div class="level-content">
                        <h4>Batch</h4>
                        <p><strong>Code:</strong> @Chain.Batch.BatchCode</p>
                        <p><strong>Plant:</strong> @Chain.Batch.PlantInfo?.PlantName</p>
                        <p><strong>Production Date:</strong> @Chain.Batch.ProductionInfo.ManufactureDate.ToString("yyyy-MM-dd")</p>
                        <p><strong>Expiry Date:</strong> @Chain.Batch.ProductionInfo.ExpiryDate.ToString("yyyy-MM-dd")</p>
                        <button @onclick="() => ShowBatchDetails()" class="btn btn-sm btn-link">View Details</button>
                    </div>
                </div>
                <div class="chain-connector">↓</div>
            }
            
            @if (Chain.Pallet != null)
            {
                <div class="chain-level pallet-level">
                    <div class="level-icon">🏗️</div>
                    <div class="level-content">
                        <h4>Pallet</h4>
                        <p><strong>Code:</strong> @Chain.Pallet.PalletCode</p>
                        <p><strong>Total Units:</strong> @Chain.Pallet.TotalUnits</p>
                        <p><strong>Cartons:</strong> @Chain.Pallet.CartonCount</p>
                        <p><strong>Status:</strong> <StatusBadge Status="@Chain.Pallet.Status" /></p>
                    </div>
                </div>
                <div class="chain-connector">↓</div>
            }
            
            @if (Chain.Carton != null)
            {
                <div class="chain-level carton-level">
                    <div class="level-icon">📦</div>
                    <div class="level-content">
                        <h4>Carton</h4>
                        <p><strong>Code:</strong> @Chain.Carton.CartonCode</p>
                        <p><strong>Units:</strong> @Chain.Carton.CurrentUnits / @Chain.Carton.UnitsPerCarton</p>
                        <p><strong>Status:</strong> <StatusBadge Status="@Chain.Carton.Status" /></p>
                    </div>
                </div>
                <div class="chain-connector">↓</div>
            }
            
            @if (Chain.Bottle != null)
            {
                <div class="chain-level bottle-level">
                    <div class="level-icon">🍾</div>
                    <div class="level-content">
                        <h4>Bottle</h4>
                        <p><strong>ID:</strong> @Chain.Bottle.BottleId</p>
                        <p><strong>Type:</strong> @Chain.Bottle.BottleType (@Chain.Bottle.CapacityML ml)</p>
                        <p><strong>Status:</strong> <StatusBadge Status="@Chain.Bottle.CurrentStatus" /></p>
                        <p><strong>Location:</strong> @Chain.Bottle.CurrentLocation</p>
                        <p><strong>Recycle Count:</strong> @Chain.Bottle.RecycleCycleCount</p>
                        
                        @if (Chain.Bottle.IsExpired)
                        {
                            <div class="alert alert-danger">⚠️ Expired</div>
                        }
                        else
                        {
                            <p><strong>Days to Expiry:</strong> @Chain.Bottle.DaysToExpiry</p>
                        }
                    </div>
                </div>
            }
        </div>
    }
</div>

@code {
    [Parameter]
    public TraceabilityChain? Chain { get; set; }
    
    private void ShowBatchDetails()
    {
        // Show batch details modal
    }
}
```

---

## 3. Movement Timeline Component

### MovementTimeline.razor

```razor
@using PackTrack.Services.Traceability

<div class="movement-timeline">
    <h3>Movement History</h3>
    
    @if (Movements != null && Movements.Any())
    {
        <div class="timeline">
            @foreach (var movement in Movements)
            {
                <div class="timeline-item">
                    <div class="timeline-marker">
                        <div class="marker-dot"></div>
                    </div>
                    <div class="timeline-content">
                        <div class="timeline-header">
                            <h5>@movement.EventType</h5>
                            <span class="timeline-date">@movement.Timestamp.ToString("yyyy-MM-dd HH:mm")</span>
                        </div>
                        <div class="timeline-body">
                            <p>
                                <strong>From:</strong> @movement.FromLocation.LocationType 
                                @if (!string.IsNullOrEmpty(movement.FromLocation.LocationId))
                                {
                                    <span>(ID: @movement.FromLocation.LocationId)</span>
                                }
                            </p>
                            <p>
                                <strong>To:</strong> @movement.ToLocation.LocationType 
                                @if (!string.IsNullOrEmpty(movement.ToLocation.LocationId))
                                {
                                    <span>(ID: @movement.ToLocation.LocationId)</span>
                                }
                            </p>
                            @if (movement.DurationAtLocation.HasValue)
                            {
                                <p><strong>Duration:</strong> @FormatDuration(movement.DurationAtLocation.Value)</p>
                            }
                            <p><strong>User:</strong> @movement.UserId</p>
                        </div>
                    </div>
                </div>
            }
        </div>
    }
    else
    {
        <p class="text-muted">No movement history available</p>
    }
</div>

@code {
    [Parameter]
    public List<MovementRecord>? Movements { get; set; }
    
    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalDays >= 1)
            return $"{(int)duration.TotalDays} days, {duration.Hours} hours";
        else if (duration.TotalHours >= 1)
            return $"{(int)duration.TotalHours} hours, {duration.Minutes} minutes";
        else
            return $"{duration.Minutes} minutes";
    }
}
```

---

## 4. Analytics Dashboard Component

### AnalyticsDashboard.razor

```razor
@using PackTrack.Services.Traceability
@inject TraceabilityMeshService TraceabilityService

<div class="analytics-dashboard">
    <h3>Batch Analytics</h3>
    
    @if (Analytics != null)
    {
        <div class="analytics-grid">
            <div class="analytics-card">
                <div class="card-icon">📊</div>
                <div class="card-content">
                    <h4>@Analytics.TotalUnits</h4>
                    <p>Total Units</p>
                </div>
            </div>
            
            <div class="analytics-card">
                <div class="card-icon">🔍</div>
                <div class="card-content">
                    <h4>@Analytics.TrackedUnits</h4>
                    <p>Tracked Units</p>
                </div>
            </div>
            
            <div class="analytics-card">
                <div class="card-icon">📈</div>
                <div class="card-content">
                    <h4>@Analytics.TraceabilityPercentage.ToString("F1")%</h4>
                    <p>Traceability</p>
                </div>
            </div>
            
            <div class="analytics-card">
                <div class="card-icon">♻️</div>
                <div class="card-content">
                    <h4>@Analytics.RecycledUnits</h4>
                    <p>Recycled</p>
                </div>
            </div>
        </div>
        
        <div class="charts-section">
            <div class="chart-container">
                <h4>Status Distribution</h4>
                <div class="status-chart">
                    @foreach (var status in Analytics.StatusDistribution)
                    {
                        <div class="status-bar">
                            <span class="status-label">@status.Key</span>
                            <div class="bar-container">
                                <div class="bar-fill" style="width: @GetPercentage(status.Value, Analytics.TotalUnits)%"></div>
                                <span class="bar-value">@status.Value</span>
                            </div>
                        </div>
                    }
                </div>
            </div>
            
            <div class="chart-container">
                <h4>Location Distribution</h4>
                <div class="location-chart">
                    @foreach (var location in Analytics.LocationDistribution)
                    {
                        <div class="location-item">
                            <span class="location-icon">📍</span>
                            <span class="location-name">@location.Key</span>
                            <span class="location-count">@location.Value</span>
                        </div>
                    }
                </div>
            </div>
        </div>
        
        <div class="stats-section">
            <div class="stat-item">
                <strong>Total Movements:</strong> @Analytics.TotalMovements
            </div>
            <div class="stat-item">
                <strong>Total Scans:</strong> @Analytics.TotalScans
            </div>
            <div class="stat-item">
                <strong>Avg Scans per Unit:</strong> @Analytics.AverageScansPerUnit.ToString("F2")
            </div>
            <div class="stat-item">
                <strong>Recycling Rate:</strong> @Analytics.RecyclingRate.ToString("F1")%
            </div>
        </div>
    }
    else
    {
        <p>Loading analytics...</p>
    }
</div>

@code {
    [Parameter]
    public int BatchId { get; set; }
    
    private TraceabilityAnalytics? Analytics;
    
    protected override void OnParametersSet()
    {
        LoadAnalytics();
    }
    
    private void LoadAnalytics()
    {
        Analytics = TraceabilityService.GetBatchAnalytics(BatchId);
    }
    
    private double GetPercentage(int value, int total)
    {
        return total > 0 ? (double)value / total * 100 : 0;
    }
}
```

---

## 5. Complete Traceability Page

### TraceabilityPage.razor

```razor
@page "/traceability"
@using PackTrack.Services.Traceability
@inject TraceabilityMeshService TraceabilityService

<div class="traceability-page">
    <div class="page-header">
        <h2>🔍 EcoCola Traceability Mesh</h2>
        <p>Track your product journey from production to recycling</p>
    </div>
    
    <div class="page-content">
        <div class="scanner-section">
            <QRScanner OnScanComplete="HandleScanComplete" />
        </div>
        
        @if (currentScanResult != null && currentScanResult.Success)
        {
            <div class="results-section">
                <div class="result-tabs">
                    <button class="tab-button @(activeTab == "chain" ? "active" : "")" 
                            @onclick="() => activeTab = \"chain\"">
                        Traceability Chain
                    </button>
                    <button class="tab-button @(activeTab == "timeline" ? "active" : "")" 
                            @onclick="() => activeTab = \"timeline\"">
                        Movement History
                    </button>
                    <button class="tab-button @(activeTab == "analytics" ? "active" : "")" 
                            @onclick="() => activeTab = \"analytics\"">
                        Analytics
                    </button>
                </div>
                
                <div class="tab-content">
                    @if (activeTab == "chain")
                    {
                        <TraceabilityChain Chain="currentScanResult.TraceabilityChain" />
                    }
                    else if (activeTab == "timeline")
                    {
                        <MovementTimeline Movements="currentScanResult.MovementHistory" />
                    }
                    else if (activeTab == "analytics" && currentScanResult.TraceabilityChain.Batch != null)
                    {
                        <AnalyticsDashboard BatchId="currentScanResult.TraceabilityChain.Batch.BatchId" />
                    }
                </div>
            </div>
        }
    </div>
</div>

@code {
    private TraceabilityScanResult? currentScanResult;
    private string activeTab = "chain";
    
    private void HandleScanComplete(TraceabilityScanResult result)
    {
        currentScanResult = result;
        activeTab = "chain";
        StateHasChanged();
    }
}
```

---

## CSS Styles

### traceability.css

```css
/* QR Scanner */
.qr-scanner-container {
    background: white;
    border-radius: 8px;
    padding: 20px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.scanner-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
}

.camera-view {
    position: relative;
    width: 100%;
    max-width: 500px;
    margin: 0 auto;
}

#qr-video {
    width: 100%;
    border-radius: 8px;
}

.scan-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    display: flex;
    align-items: center;
    justify-content: center;
}

.scan-frame {
    width: 250px;
    height: 250px;
    border: 3px solid #4CAF50;
    border-radius: 8px;
    box-shadow: 0 0 0 9999px rgba(0,0,0,0.5);
}

.manual-input {
    display: flex;
    gap: 10px;
    margin: 20px 0;
}

.scan-result {
    margin-top: 20px;
    padding: 15px;
    border-radius: 8px;
}

.scan-result.success {
    background: #d4edda;
    border: 1px solid #c3e6cb;
}

.scan-result.error {
    background: #f8d7da;
    border: 1px solid #f5c6cb;
}

/* Traceability Chain */
.traceability-chain {
    background: white;
    border-radius: 8px;
    padding: 20px;
}

.chain-visualization {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
}

.chain-level {
    display: flex;
    align-items: center;
    gap: 15px;
    padding: 15px;
    border-radius: 8px;
    width: 100%;
    max-width: 600px;
}

.batch-level { background: #e3f2fd; border-left: 4px solid #2196F3; }
.pallet-level { background: #f3e5f5; border-left: 4px solid #9C27B0; }
.carton-level { background: #fff3e0; border-left: 4px solid #FF9800; }
.bottle-level { background: #e8f5e9; border-left: 4px solid #4CAF50; }

.level-icon {
    font-size: 32px;
}

.level-content {
    flex: 1;
}

.level-content h4 {
    margin: 0 0 10px 0;
    font-size: 18px;
}

.level-content p {
    margin: 5px 0;
    font-size: 14px;
}

.chain-connector {
    font-size: 24px;
    color: #666;
}

/* Movement Timeline */
.movement-timeline {
    background: white;
    border-radius: 8px;
    padding: 20px;
}

.timeline {
    position: relative;
    padding-left: 40px;
}

.timeline::before {
    content: '';
    position: absolute;
    left: 15px;
    top: 0;
    bottom: 0;
    width: 2px;
    background: #e0e0e0;
}

.timeline-item {
    position: relative;
    margin-bottom: 30px;
}

.timeline-marker {
    position: absolute;
    left: -40px;
    top: 0;
}

.marker-dot {
    width: 12px;
    height: 12px;
    border-radius: 50%;
    background: #2196F3;
    border: 3px solid white;
    box-shadow: 0 0 0 2px #2196F3;
}

.timeline-content {
    background: #f5f5f5;
    border-radius: 8px;
    padding: 15px;
}

.timeline-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 10px;
}

.timeline-header h5 {
    margin: 0;
    font-size: 16px;
    color: #2196F3;
}

.timeline-date {
    font-size: 12px;
    color: #666;
}

.timeline-body p {
    margin: 5px 0;
    font-size: 14px;
}

/* Analytics Dashboard */
.analytics-dashboard {
    background: white;
    border-radius: 8px;
    padding: 20px;
}

.analytics-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 20px;
    margin-bottom: 30px;
}

.analytics-card {
    display: flex;
    align-items: center;
    gap: 15px;
    padding: 20px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border-radius: 8px;
}

.card-icon {
    font-size: 36px;
}

.card-content h4 {
    margin: 0;
    font-size: 28px;
}

.card-content p {
    margin: 5px 0 0 0;
    font-size: 14px;
    opacity: 0.9;
}

.charts-section {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 20px;
    margin-bottom: 20px;
}

.chart-container {
    background: #f5f5f5;
    border-radius: 8px;
    padding: 20px;
}

.chart-container h4 {
    margin: 0 0 15px 0;
}

.status-bar {
    margin-bottom: 15px;
}

.status-label {
    display: block;
    font-size: 14px;
    margin-bottom: 5px;
    font-weight: 500;
}

.bar-container {
    position: relative;
    background: #e0e0e0;
    border-radius: 4px;
    height: 30px;
    display: flex;
    align-items: center;
}

.bar-fill {
    background: linear-gradient(90deg, #4CAF50, #8BC34A);
    height: 100%;
    border-radius: 4px;
    transition: width 0.3s ease;
}

.bar-value {
    position: absolute;
    right: 10px;
    font-size: 12px;
    font-weight: bold;
}

.location-item {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px;
    background: white;
    border-radius: 4px;
    margin-bottom: 10px;
}

.location-icon {
    font-size: 20px;
}

.location-name {
    flex: 1;
    font-size: 14px;
}

.location-count {
    font-weight: bold;
    color: #2196F3;
}

.stats-section {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 15px;
    padding: 20px;
    background: #f5f5f5;
    border-radius: 8px;
}

.stat-item {
    font-size: 14px;
}

/* Page Layout */
.traceability-page {
    max-width: 1200px;
    margin: 0 auto;
    padding: 20px;
}

.page-header {
    text-align: center;
    margin-bottom: 30px;
}

.page-header h2 {
    margin: 0 0 10px 0;
    font-size: 32px;
}

.page-header p {
    margin: 0;
    color: #666;
}

.scanner-section {
    margin-bottom: 30px;
}

.result-tabs {
    display: flex;
    gap: 10px;
    margin-bottom: 20px;
    border-bottom: 2px solid #e0e0e0;
}

.tab-button {
    padding: 10px 20px;
    background: none;
    border: none;
    border-bottom: 3px solid transparent;
    cursor: pointer;
    font-size: 16px;
    transition: all 0.3s;
}

.tab-button:hover {
    background: #f5f5f5;
}

.tab-button.active {
    border-bottom-color: #2196F3;
    color: #2196F3;
    font-weight: bold;
}

/* Responsive */
@media (max-width: 768px) {
    .analytics-grid {
        grid-template-columns: 1fr;
    }
    
    .charts-section {
        grid-template-columns: 1fr;
    }
    
    .stats-section {
        grid-template-columns: 1fr;
    }
}
```

---

## API Integration

### TraceabilityApiService.cs

```csharp
public class TraceabilityApiService
{
    private readonly HttpClient _httpClient;
    
    public TraceabilityApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<TraceabilityScanResult> ScanQRCodeAsync(string qrCode, ScanContext context)
    {
        var request = new
        {
            QRCode = qrCode,
            UserId = context.UserId,
            Location = context.Location,
            ScanType = context.ScanType,
            DeviceId = context.DeviceId,
            Latitude = context.Latitude,
            Longitude = context.Longitude
        };
        
        var response = await _httpClient.PostAsJsonAsync("/api/traceability/scan", request);
        return await response.Content.ReadFromJsonAsync<TraceabilityScanResult>();
    }
    
    public async Task<TraceabilityChain> GetTraceabilityChainAsync(string qrCode)
    {
        return await _httpClient.GetFromJsonAsync<TraceabilityChain>($"/api/traceability/chain/{qrCode}");
    }
    
    public async Task<List<MovementRecord>> GetMovementHistoryAsync(string qrCode)
    {
        return await _httpClient.GetFromJsonAsync<List<MovementRecord>>($"/api/traceability/movement-history/{qrCode}");
    }
    
    public async Task<TraceabilityAnalytics> GetBatchAnalyticsAsync(int batchId)
    {
        return await _httpClient.GetFromJsonAsync<TraceabilityAnalytics>($"/api/traceability/analytics/batch/{batchId}");
    }
}
```

---

## Usage Example

### Program.cs Registration

```csharp
builder.Services.AddScoped<TraceabilityMeshService>();
builder.Services.AddScoped<TraceabilityApiService>();
```

### Navigation Menu

```razor
<NavLink class="nav-link" href="traceability">
    <span class="oi oi-magnifying-glass" aria-hidden="true"></span> Traceability
</NavLink>
```

This completes the comprehensive UI components for the traceability system!
