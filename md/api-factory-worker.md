# Factory Worker APIs - Detailed Specification

## Role Overview
Factory workers use the mobile app for production tracking, batch creation, quality control checks, QR generation, defect reporting, and shift management.

## Dashboard Overview

### Key Metrics Displayed
- Today's production count
- Shift targets vs actual
- Quality check pass rate
- Defects reported
- Active batches
- Machine status
- Safety compliance score

---

## API Specifications

### 1. Get Factory Worker Dashboard
**Endpoint:** `GET /api/factory/dashboard`

**Headers:**
```json
{
  "Authorization": "Bearer {jwt_token}",
  "Worker-Id": "{worker_id}"
}
```

**Response:**
```json
{
  "workerId": "WKR-001",
  "workerName": "Priya Sharma",
  "plantId": 1,
  "plantName": "EcoCola Plant Mumbai",
  "currentShift": {
    "shiftId": "SHIFT-2024-050",
    "shiftCode": "Morning-A",
    "startTime": "2026-02-21T06:00:00Z",
    "endTime": "2026-02-21T14:00:00Z",
    "productionLineId": "LINE-01",
    "supervisor": "Rajesh Kumar"
  },
  "todayStats": {
    "unitsProduced": 2450,
    "targetUnits": 3000,
    "completionPercentage": 81.7,
    "qualityChecksPassed": 48,
    "qualityChecksFailed": 2,
    "defectsReported": 3,
    "batchesCreated": 5,
    "hoursWorked": 6.5
  },
  "activeBatches": [
    {
      "batchId": 101,
      "batchCode": "B-20260221-3130",
      "bottleType": "Thick",
      "capacityML": 500,
      "status": "InProduction",
      "producedUnits": 450,
      "targetUnits": 600,
      "startedAt": "2026-02-21T12:00:00Z"
    }
  ],
  "pendingTasks": [
    {
      "taskId": "TASK-QC-001",
      "taskType": "QualityCheck",
      "batchCode": "B-20260221-3130",
      "priority": "High",
      "dueTime": "2026-02-21T13:00:00Z"
    }
  ],
  "machineStatus": [
    {
      "machineId": "MACH-01",
      "machineName": "Filling Machine A",
      "status": "Running",
      "efficiency": 95.5,
      "lastMaintenance": "2026-02-15"
    }
  ],
  "alerts": [
    {
      "type": "QualityCheck",
      "message": "Quality check due for Batch B-20260221-3130",
      "severity": "Medium"
    }
  ]
}
```

---

### 2. Create Production Batch
**Endpoint:** `POST /api/factory/create-batch`

**Request:**
```json
{
  "plantId": 1,
  "productionLineId": "LINE-01",
  "machineId": "MACH-01",
  "shiftCode": "Morning-A",
  "operatorId": "WKR-001",
  "supervisorId": "SUP-001",
  "bottleType": "Thick",
  "bottleMaterialGrade": "A+",
  "bottleThicknessMicron": 250,
  "capacityML": 500,
  "containerType": "Bottle",
  "liquidType": "Cola Classic",
  "liquidBatchCode": "LIQ-2026-021-001",
  "brixLevel": 11.5,
  "acidityPH": 2.5,
  "co2Volumes": 3.8,
  "ingredientsList": "Water, Sugar, CO2, Caramel, Phosphoric Acid, Natural Flavors",
  "totalPlannedUnits": 600,
  "targetMarket": "Mumbai Metro",
  "wholesaleRate": 15.00,
  "mrp": 20.00,
  "taxCode": "GST-18",
  "manufactureDate": "2026-02-21",
  "expiryDate": "2026-08-21"
}
```

**Response:**
```json
{
  "success": true,
  "batchId": 101,
  "batchCode": "B-20260221-3130",
  "qrCode": "ECO-B-20260221-3130",
  "status": "Created",
  "createdAt": "2026-02-21T12:00:00Z",
  "estimatedCompletionTime": "2026-02-21T14:30:00Z",
  "message": "Production batch created successfully. Start production."
}
```

---

### 3. Scan QR Code
**Endpoint:** `POST /api/factory/scan-qr`

**Request:**
```json
{
  "qrCode": "ECO-B-20260221-3130-01-0005",
  "scanType": "Production" | "QualityCheck" | "Packaging" | "Verification",
  "workerId": "WKR-001",
  "location": "Production Line 01",
  "timestamp": "2026-02-21T12:30:00Z",
  "additionalData": {
    "machineId": "MACH-01",
    "temperature": 25.5,
    "pressure": 2.5
  }
}
```

**Response:**
```json
{
  "success": true,
  "qrDetails": {
    "bottleId": "ECO-B-20260221-3130-01-0005",
    "batchCode": "B-20260221-3130",
    "bottleType": "Thick",
    "capacityML": 500,
    "manufactureDate": "2026-02-21",
    "expiryDate": "2026-08-21",
    "currentStatus": "Produced",
    "qualityStatus": "Pending"
  },
  "actionRequired": "QualityCheck",
  "message": "Bottle scanned. Proceed with quality check."
}
```

---

### 4. Perform Quality Check
**Endpoint:** `POST /api/factory/quality-check`

**Request:**
```json
{
  "batchId": 101,
  "batchCode": "B-20260221-3130",
  "workerId": "WKR-001",
  "checkType": "Visual" | "Weight" | "Seal" | "Label" | "Carbonation" | "pH" | "Comprehensive",
  "sampleSize": 10,
  "checkParameters": {
    "visualInspection": {
      "passed": true,
      "notes": "No visible defects"
    },
    "weightCheck": {
      "passed": true,
      "averageWeight": 525.5,
      "targetWeight": 525.0,
      "tolerance": 5.0
    },
    "sealIntegrity": {
      "passed": true,
      "pressure": 2.5,
      "leakTest": "Pass"
    },
    "labelQuality": {
      "passed": true,
      "alignment": "Good",
      "printQuality": "Excellent"
    },
    "carbonationLevel": {
      "passed": true,
      "measured": 3.8,
      "target": 3.8,
      "tolerance": 0.2
    },
    "phLevel": {
      "passed": true,
      "measured": 2.5,
      "target": 2.5,
      "tolerance": 0.1
    }
  },
  "overallResult": "Pass",
  "defectsFound": 0,
  "timestamp": "2026-02-21T13:00:00Z",
  "notes": "All parameters within acceptable range",
  "photos": []
}
```

**Response:**
```json
{
  "success": true,
  "checkId": "QC-2024-5001",
  "batchStatus": "Approved",
  "qualityScore": 98.5,
  "certificateGenerated": true,
  "certificateUrl": "https://storage.packtrack.com/qc/2024/02/21/QC-2024-5001.pdf",
  "nextAction": "ProceedToPackaging",
  "message": "Quality check passed. Batch approved for packaging."
}
```

---

### 5. Report Defect
**Endpoint:** `POST /api/factory/report-defect`

**Request:**
```json
{
  "batchId": 101,
  "batchCode": "B-20260221-3130",
  "bottleId": "ECO-B-20260221-3130-01-0005",
  "workerId": "WKR-001",
  "defectType": "Seal" | "Label" | "Weight" | "Carbonation" | "Contamination" | "Damage" | "Other",
  "severity": "Low" | "Medium" | "High" | "Critical",
  "description": "Improper seal detected. Cap not fully tightened.",
  "location": "Production Line 01 - Capping Station",
  "machineId": "MACH-01",
  "timestamp": "2026-02-21T13:15:00Z",
  "photos": [
    "base64_encoded_photo_1",
    "base64_encoded_photo_2"
  ],
  "affectedUnits": 5,
  "rootCause": "Machine calibration issue",
  "correctiveAction": "Machine recalibrated. Production resumed.",
  "requiresMaintenance": true
}
```

**Response:**
```json
{
  "success": true,
  "defectId": "DEF-2024-001",
  "ticketNumber": "TICKET-2024-5001",
  "status": "Reported",
  "assignedTo": "Maintenance Team",
  "priority": "High",
  "estimatedResolution": "2026-02-21T14:00:00Z",
  "affectedBatch": {
    "batchCode": "B-20260221-3130",
    "status": "OnHold",
    "unitsQuarantined": 5
  },
  "message": "Defect reported. Maintenance team notified. Affected units quarantined."
}
```

---

### 6. Get Shift Tasks
**Endpoint:** `GET /api/factory/shift-tasks`

**Query Parameters:**
- `shiftId`: "SHIFT-2024-050"
- `status`: "Pending" | "InProgress" | "Completed"

**Response:**
```json
{
  "shiftId": "SHIFT-2024-050",
  "shiftCode": "Morning-A",
  "workerId": "WKR-001",
  "tasks": [
    {
      "taskId": "TASK-PROD-001",
      "taskType": "Production",
      "title": "Complete Batch B-20260221-3130",
      "description": "Produce 600 units of 500ml Thick bottles",
      "priority": "High",
      "status": "InProgress",
      "assignedAt": "2026-02-21T06:00:00Z",
      "dueTime": "2026-02-21T14:00:00Z",
      "progress": 75,
      "batchCode": "B-20260221-3130"
    },
    {
      "taskId": "TASK-QC-001",
      "taskType": "QualityCheck",
      "title": "Quality Check - Batch B-20260221-3130",
      "description": "Perform comprehensive quality check on sample units",
      "priority": "High",
      "status": "Pending",
      "assignedAt": "2026-02-21T12:00:00Z",
      "dueTime": "2026-02-21T13:00:00Z",
      "batchCode": "B-20260221-3130"
    },
    {
      "taskId": "TASK-PKG-001",
      "taskType": "Packaging",
      "title": "Package Batch B-20260221-3129",
      "description": "Create pallets and cartons for completed batch",
      "priority": "Medium",
      "status": "Completed",
      "completedAt": "2026-02-21T11:30:00Z",
      "batchCode": "B-20260221-3129"
    }
  ],
  "summary": {
    "totalTasks": 8,
    "completed": 5,
    "inProgress": 2,
    "pending": 1
  }
}
```

---

### 7. Complete Task
**Endpoint:** `POST /api/factory/complete-task`

**Request:**
```json
{
  "taskId": "TASK-PROD-001",
  "workerId": "WKR-001",
  "completedAt": "2026-02-21T13:45:00Z",
  "status": "Completed",
  "actualUnits": 600,
  "targetUnits": 600,
  "qualityScore": 98.5,
  "notes": "Production completed successfully. All units passed quality check.",
  "timeSpent": 120,
  "issues": []
}
```

**Response:**
```json
{
  "success": true,
  "taskId": "TASK-PROD-001",
  "status": "Completed",
  "performanceScore": 100,
  "nextTask": {
    "taskId": "TASK-QC-002",
    "taskType": "QualityCheck",
    "title": "Quality Check - Batch B-20260221-3131",
    "priority": "High"
  },
  "message": "Task completed successfully. Great work!"
}
```

---

### 8. Get Production Targets
**Endpoint:** `GET /api/factory/production-targets`

**Query Parameters:**
- `date`: "2026-02-21"
- `shiftCode`: "Morning-A"

**Response:**
```json
{
  "date": "2026-02-21",
  "shiftCode": "Morning-A",
  "productionLineId": "LINE-01",
  "targets": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "targetUnits": 3000,
      "producedUnits": 2450,
      "remainingUnits": 550,
      "completionPercentage": 81.7,
      "status": "OnTrack"
    },
    {
      "bottleType": "rPET",
      "capacityML": 1000,
      "targetUnits": 2000,
      "producedUnits": 1800,
      "remainingUnits": 200,
      "completionPercentage": 90.0,
      "status": "OnTrack"
    }
  ],
  "overallProgress": {
    "totalTarget": 5000,
    "totalProduced": 4250,
    "completionPercentage": 85.0,
    "estimatedCompletionTime": "2026-02-21T13:30:00Z",
    "status": "OnTrack"
  },
  "efficiency": {
    "currentRate": 375,
    "targetRate": 357,
    "efficiency": 105.0
  }
}
```

---

### 9. Record Wastage
**Endpoint:** `POST /api/factory/record-wastage`

**Request:**
```json
{
  "batchId": 101,
  "batchCode": "B-20260221-3130",
  "workerId": "WKR-001",
  "wastageType": "Production" | "Quality" | "Packaging" | "Spillage" | "Damage",
  "category": "Bottles" | "Caps" | "Labels" | "Liquid" | "Packaging",
  "quantity": 5,
  "unit": "Units",
  "reason": "Failed quality check - improper seal",
  "stage": "Capping",
  "machineId": "MACH-01",
  "timestamp": "2026-02-21T13:15:00Z",
  "estimatedValue": 75.00,
  "recyclable": true,
  "disposalMethod": "Recycle",
  "photos": [],
  "notes": "Units sent to recycling facility"
}
```

**Response:**
```json
{
  "success": true,
  "wastageId": "WAST-2024-001",
  "recorded": true,
  "batchUpdated": true,
  "totalWastageToday": 15,
  "wastagePercentage": 0.6,
  "targetWastage": 2.0,
  "status": "WithinTarget",
  "message": "Wastage recorded. Units marked for recycling."
}
```

---

### 10. Get Machine Status
**Endpoint:** `GET /api/factory/machine-status`

**Query Parameters:**
- `productionLineId`: "LINE-01"

**Response:**
```json
{
  "productionLineId": "LINE-01",
  "machines": [
    {
      "machineId": "MACH-01",
      "machineName": "Filling Machine A",
      "machineType": "Filling",
      "status": "Running",
      "currentSpeed": 100,
      "targetSpeed": 100,
      "efficiency": 95.5,
      "temperature": 25.5,
      "pressure": 2.5,
      "vibration": 0.5,
      "unitsProduced": 2450,
      "uptime": 6.5,
      "downtime": 0.5,
      "lastMaintenance": "2026-02-15",
      "nextMaintenance": "2026-03-15",
      "alerts": []
    },
    {
      "machineId": "MACH-02",
      "machineName": "Capping Machine A",
      "machineType": "Capping",
      "status": "Warning",
      "currentSpeed": 95,
      "targetSpeed": 100,
      "efficiency": 92.0,
      "temperature": 28.5,
      "pressure": 3.0,
      "vibration": 1.2,
      "unitsProduced": 2400,
      "uptime": 6.0,
      "downtime": 1.0,
      "lastMaintenance": "2026-02-10",
      "nextMaintenance": "2026-03-10",
      "alerts": [
        {
          "type": "HighVibration",
          "message": "Vibration level above normal. Check alignment.",
          "severity": "Warning"
        }
      ]
    }
  ],
  "overallStatus": "Running",
  "overallEfficiency": 93.8
}
```

---

### 11. Calibrate Equipment
**Endpoint:** `POST /api/factory/calibrate-equipment`

**Request:**
```json
{
  "machineId": "MACH-01",
  "workerId": "WKR-001",
  "calibrationType": "Routine" | "Corrective" | "Verification",
  "parameters": {
    "fillVolume": {
      "target": 500,
      "measured": 500.5,
      "adjusted": true,
      "finalValue": 500.0
    },
    "cappingTorque": {
      "target": 2.5,
      "measured": 2.3,
      "adjusted": true,
      "finalValue": 2.5
    },
    "temperature": {
      "target": 25.0,
      "measured": 25.5,
      "adjusted": false,
      "finalValue": 25.5
    }
  },
  "timestamp": "2026-02-21T14:00:00Z",
  "verificationTest": {
    "performed": true,
    "result": "Pass",
    "sampleSize": 10
  },
  "notes": "Calibration completed. Machine operating within specifications.",
  "nextCalibrationDue": "2026-02-28"
}
```

**Response:**
```json
{
  "success": true,
  "calibrationId": "CAL-2024-001",
  "machineStatus": "Calibrated",
  "certificateGenerated": true,
  "certificateUrl": "https://storage.packtrack.com/calibration/2024/02/21/CAL-2024-001.pdf",
  "nextCalibrationDue": "2026-02-28",
  "message": "Equipment calibrated successfully. Production can resume."
}
```

---

### 12. Get Safety Checklist
**Endpoint:** `GET /api/factory/safety-checklist`

**Response:**
```json
{
  "shiftId": "SHIFT-2024-050",
  "workerId": "WKR-001",
  "date": "2026-02-21",
  "checklist": [
    {
      "itemId": "SAFE-001",
      "category": "PPE",
      "item": "Safety Helmet",
      "required": true,
      "status": "Checked",
      "checkedAt": "2026-02-21T06:00:00Z"
    },
    {
      "itemId": "SAFE-002",
      "category": "PPE",
      "item": "Safety Gloves",
      "required": true,
      "status": "Checked",
      "checkedAt": "2026-02-21T06:00:00Z"
    },
    {
      "itemId": "SAFE-003",
      "category": "Equipment",
      "item": "Fire Extinguisher Check",
      "required": true,
      "status": "Pending",
      "dueTime": "2026-02-21T07:00:00Z"
    },
    {
      "itemId": "SAFE-004",
      "category": "Equipment",
      "item": "Emergency Stop Button Test",
      "required": true,
      "status": "Checked",
      "checkedAt": "2026-02-21T06:15:00Z"
    },
    {
      "itemId": "SAFE-005",
      "category": "Environment",
      "item": "Floor Cleanliness",
      "required": true,
      "status": "Checked",
      "checkedAt": "2026-02-21T06:30:00Z"
    }
  ],
  "completionPercentage": 80,
  "complianceScore": 95,
  "alerts": [
    {
      "type": "PendingCheck",
      "message": "Fire extinguisher check pending",
      "severity": "Medium"
    }
  ]
}
```

---

### 13. Create Pallet
**Endpoint:** `POST /api/factory/create-pallet`

**Request:**
```json
{
  "batchId": 101,
  "batchCode": "B-20260221-3130",
  "workerId": "WKR-001",
  "cartonCount": 10,
  "unitsPerCarton": 24,
  "totalUnits": 240,
  "netWeight": 120.0,
  "grossWeight": 135.0,
  "dimensions": {
    "length": 120,
    "width": 100,
    "height": 150
  },
  "timestamp": "2026-02-21T14:30:00Z",
  "location": "Packaging Area A",
  "notes": "Pallet ready for dispatch"
}
```

**Response:**
```json
{
  "success": true,
  "palletId": 501,
  "palletCode": "PLT-20260221-001",
  "qrCode": "ECO-PLT-20260221-001",
  "status": "Packed",
  "cartons": [
    {
      "cartonId": 5001,
      "cartonCode": "CTN-20260221-001-01",
      "unitsPerCarton": 24,
      "qrCode": "ECO-CTN-20260221-001-01"
    }
  ],
  "dispatchReady": true,
  "estimatedDispatchTime": "2026-02-21T16:00:00Z",
  "message": "Pallet created successfully. Ready for dispatch."
}
```

---

## Offline Mode Support

### Data Cached Locally
- Current shift details
- Active batches
- Production targets
- Machine status (last sync)
- Safety checklist
- Last 24 hours production data

### Actions Available Offline
- View dashboard (cached data)
- Scan QR codes (queued for sync)
- Record production (queued for sync)
- Report defects (queued for sync)
- Take photos (stored locally)
- View safety checklist

### Sync Strategy
- Auto-sync every 5 minutes when online
- Manual sync button
- Priority sync: Quality checks, defects, safety issues
- Queue size limit: 200 actions
- Conflict resolution: Latest timestamp wins

---

## Push Notifications

### Notification Types
1. **New Task Assigned** - High priority
2. **Quality Check Due** - High priority
3. **Machine Alert** - Critical priority
4. **Shift Starting Soon** - Medium priority
5. **Production Target Achieved** - Low priority
6. **Safety Alert** - Critical priority
7. **Maintenance Scheduled** - Medium priority
8. **Batch Approved** - Low priority

---

## Performance Requirements

- Dashboard load time: < 2 seconds
- QR scan response: < 1 second
- Batch creation: < 3 seconds
- Quality check submission: < 2 seconds
- Photo upload: Max 5MB per image
- Offline mode: Support 8 hours without sync
- Real-time machine status updates: Every 60 seconds
