# Transport Driver APIs - Detailed Specification

## Role Overview
Transport drivers use the mobile app for delivery tracking, QR scanning at pickup/delivery points, GPS tracking, and real-time status updates.

## Dashboard Overview

### Key Metrics Displayed
- Active deliveries count
- Completed deliveries today
- Distance traveled
- Fuel consumption
- Current location
- Next delivery ETA
- Pending tasks

---

## API Specifications

### 1. Get Transport Dashboard
**Endpoint:** `GET /api/transport/dashboard`

**Headers:**
```json
{
  "Authorization": "Bearer {jwt_token}",
  "Device-Id": "{unique_device_id}"
}
```

**Response:**
```json
{
  "driverId": "DRV-001",
  "driverName": "Rajesh Kumar",
  "truckId": "TRK-MH-2024",
  "truckNumber": "MH-12-AB-1234",
  "currentStatus": "OnRoute",
  "todayStats": {
    "deliveriesCompleted": 8,
    "deliveriesPending": 3,
    "distanceTraveled": 145.5,
    "fuelConsumed": 18.2,
    "hoursOnDuty": 6.5
  },
  "currentLocation": {
    "latitude": 19.0760,
    "longitude": 72.8777,
    "address": "Andheri East, Mumbai",
    "timestamp": "2026-02-21T14:30:00Z"
  },
  "activeDeliveries": [
    {
      "taskId": "TASK-2024-001",
      "retailerName": "SuperMart Andheri",
      "address": "Shop 5, Andheri West, Mumbai",
      "scheduledTime": "2026-02-21T15:00:00Z",
      "priority": "High",
      "itemCount": 120,
      "status": "InTransit"
    }
  ],
  "alerts": [
    {
      "type": "Weather",
      "message": "Heavy rain expected in 2 hours",
      "severity": "Warning"
    }
  ]
}
```

---

### 2. Get Active Deliveries
**Endpoint:** `GET /api/transport/active-deliveries`

**Query Parameters:**
- `status` (optional): "Pending" | "InTransit" | "Completed"
- `date` (optional): "2026-02-21"

**Response:**
```json
{
  "deliveries": [
    {
      "taskId": "TASK-2024-001",
      "assignmentId": "ASGN-2024-050",
      "retailerId": 101,
      "retailerName": "SuperMart Andheri",
      "contactPerson": "Amit Shah",
      "contactPhone": "+91-9876543210",
      "address": "Shop 5, Andheri West, Mumbai - 400058",
      "location": {
        "latitude": 19.1136,
        "longitude": 72.8697
      },
      "scheduledTime": "2026-02-21T15:00:00Z",
      "timeWindow": {
        "start": "2026-02-21T14:00:00Z",
        "end": "2026-02-21T16:00:00Z"
      },
      "items": [
        {
          "batchCode": "B-20260221-3130",
          "bottleType": "Thick",
          "capacityML": 500,
          "quantity": 60,
          "palletId": "PLT-001"
        },
        {
          "batchCode": "B-20260221-3131",
          "bottleType": "rPET",
          "capacityML": 1000,
          "quantity": 60,
          "palletId": "PLT-002"
        }
      ],
      "totalUnits": 120,
      "estimatedValue": 12000.00,
      "status": "InTransit",
      "distance": 8.5,
      "eta": "2026-02-21T14:55:00Z",
      "priority": "High",
      "specialInstructions": "Call before arrival. Use back entrance."
    }
  ],
  "summary": {
    "totalDeliveries": 3,
    "totalUnits": 350,
    "totalDistance": 45.2,
    "estimatedCompletionTime": "2026-02-21T18:00:00Z"
  }
}
```

---

### 3. Scan QR Code
**Endpoint:** `POST /api/transport/scan-qr`

**Request:**
```json
{
  "qrCode": "ECO-B-20260221-3130-01-0005",
  "scanType": "Pickup" | "Delivery" | "Verification",
  "taskId": "TASK-2024-001",
  "location": {
    "latitude": 19.0760,
    "longitude": 72.8777
  },
  "timestamp": "2026-02-21T14:30:00Z",
  "notes": "All items verified and loaded"
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
    "currentStatus": "InTransit",
    "lastLocation": "Plant-Mumbai-01"
  },
  "movementLogged": true,
  "movementId": "MOV-2024-5001",
  "message": "QR scanned successfully. Item loaded for delivery."
}
```

---

### 4. Update GPS Location
**Endpoint:** `POST /api/transport/update-location`

**Request:**
```json
{
  "truckId": "TRK-MH-2024",
  "latitude": 19.0760,
  "longitude": 72.8777,
  "speed": 45.5,
  "heading": 180,
  "accuracy": 10,
  "timestamp": "2026-02-21T14:30:00Z",
  "batteryLevel": 85,
  "isMoving": true
}
```

**Response:**
```json
{
  "success": true,
  "logId": "GPS-2024-50001",
  "nearbyDeliveries": [
    {
      "taskId": "TASK-2024-001",
      "distance": 2.3,
      "eta": 8
    }
  ],
  "routeDeviation": false,
  "geofenceAlerts": []
}
```

---

### 5. Get Route Details
**Endpoint:** `GET /api/transport/route-details/{routeId}`

**Response:**
```json
{
  "routeId": "ROUTE-2024-050",
  "routeCode": "MUM-WEST-01",
  "startPoint": {
    "name": "Distribution Center Mumbai",
    "latitude": 19.0176,
    "longitude": 72.8561
  },
  "endPoint": {
    "name": "Distribution Center Mumbai",
    "latitude": 19.0176,
    "longitude": 72.8561
  },
  "waypoints": [
    {
      "sequence": 1,
      "taskId": "TASK-2024-001",
      "retailerName": "SuperMart Andheri",
      "latitude": 19.1136,
      "longitude": 72.8697,
      "estimatedArrival": "2026-02-21T15:00:00Z",
      "status": "Pending"
    },
    {
      "sequence": 2,
      "taskId": "TASK-2024-002",
      "retailerName": "MegaStore Bandra",
      "latitude": 19.0596,
      "longitude": 72.8295,
      "estimatedArrival": "2026-02-21T16:30:00Z",
      "status": "Pending"
    }
  ],
  "totalDistance": 45.2,
  "estimatedDuration": 180,
  "optimizationScore": 92,
  "trafficConditions": "Moderate",
  "alternateRoutes": []
}
```

---

### 6. Mark Delivery Complete
**Endpoint:** `POST /api/transport/mark-delivery-complete`

**Request:**
```json
{
  "taskId": "TASK-2024-001",
  "completedAt": "2026-02-21T15:10:00Z",
  "location": {
    "latitude": 19.1136,
    "longitude": 72.8697
  },
  "receivedBy": "Amit Shah",
  "receiverSignature": "base64_encoded_signature_image",
  "proofOfDelivery": [
    "base64_encoded_photo_1",
    "base64_encoded_photo_2"
  ],
  "itemsDelivered": [
    {
      "batchCode": "B-20260221-3130",
      "quantityDelivered": 60,
      "quantityOrdered": 60,
      "condition": "Good"
    }
  ],
  "notes": "Delivered successfully. All items verified by receiver.",
  "issues": []
}
```

**Response:**
```json
{
  "success": true,
  "deliveryId": "DEL-2024-5001",
  "taskStatus": "Completed",
  "invoiceGenerated": true,
  "invoiceNumber": "INV-2024-10001",
  "nextDelivery": {
    "taskId": "TASK-2024-002",
    "retailerName": "MegaStore Bandra",
    "distance": 5.2,
    "eta": 25
  },
  "message": "Delivery marked complete. Invoice sent to retailer."
}
```

---

### 7. Get Fuel Status
**Endpoint:** `GET /api/transport/fuel-status`

**Response:**
```json
{
  "truckId": "TRK-MH-2024",
  "currentFuelLevel": 65.5,
  "fuelCapacity": 100,
  "fuelPercentage": 65.5,
  "estimatedRange": 320,
  "todayConsumption": 18.2,
  "averageConsumption": 12.5,
  "fuelEfficiency": 11.6,
  "lastRefuel": {
    "date": "2026-02-21T08:00:00Z",
    "quantity": 45.0,
    "cost": 4050.00,
    "location": "Fuel Station Andheri"
  },
  "nearbyFuelStations": [
    {
      "name": "HP Petrol Pump",
      "distance": 2.1,
      "pricePerLiter": 90.00
    }
  ],
  "alerts": [
    {
      "type": "LowFuel",
      "message": "Fuel level below 70%. Consider refueling.",
      "severity": "Info"
    }
  ]
}
```

---

### 8. Report Incident
**Endpoint:** `POST /api/transport/report-incident`

**Request:**
```json
{
  "truckId": "TRK-MH-2024",
  "incidentType": "Accident" | "Breakdown" | "Delay" | "Theft" | "Damage" | "Other",
  "severity": "Low" | "Medium" | "High" | "Critical",
  "description": "Minor accident at traffic signal. No injuries. Vehicle drivable.",
  "location": {
    "latitude": 19.0760,
    "longitude": 72.8777,
    "address": "Andheri East, Mumbai"
  },
  "timestamp": "2026-02-21T14:30:00Z",
  "photos": [
    "base64_encoded_photo_1",
    "base64_encoded_photo_2"
  ],
  "affectedDeliveries": ["TASK-2024-001", "TASK-2024-002"],
  "policeReportNumber": "FIR-2024-001",
  "requiresAssistance": false
}
```

**Response:**
```json
{
  "success": true,
  "incidentId": "INC-2024-001",
  "ticketNumber": "TICKET-2024-5001",
  "status": "Reported",
  "assignedTo": "Support Team Mumbai",
  "estimatedResolutionTime": "2026-02-21T16:00:00Z",
  "message": "Incident reported. Support team will contact you shortly.",
  "emergencyContacts": [
    {
      "name": "Fleet Manager",
      "phone": "+91-9876543210"
    }
  ]
}
```

---

### 9. Get Trip History
**Endpoint:** `GET /api/transport/trip-history`

**Query Parameters:**
- `startDate`: "2026-02-01"
- `endDate`: "2026-02-21"
- `page`: 1
- `pageSize`: 20

**Response:**
```json
{
  "trips": [
    {
      "tripId": "TRIP-2024-050",
      "date": "2026-02-21",
      "routeCode": "MUM-WEST-01",
      "startTime": "2026-02-21T09:00:00Z",
      "endTime": "2026-02-21T18:00:00Z",
      "totalDistance": 145.5,
      "totalDeliveries": 11,
      "deliveriesCompleted": 8,
      "deliveriesFailed": 0,
      "deliveriesPending": 3,
      "fuelConsumed": 18.2,
      "averageSpeed": 35.5,
      "status": "InProgress"
    }
  ],
  "summary": {
    "totalTrips": 45,
    "totalDistance": 5420.5,
    "totalDeliveries": 450,
    "successRate": 98.5,
    "averageFuelEfficiency": 12.1
  },
  "pagination": {
    "currentPage": 1,
    "totalPages": 3,
    "totalRecords": 45
  }
}
```

---

### 10. Upload Proof of Delivery
**Endpoint:** `POST /api/transport/upload-pod`

**Request (Multipart Form Data):**
```
taskId: TASK-2024-001
deliveryId: DEL-2024-5001
documentType: Photo | Signature | Invoice
file: [binary file data]
notes: Receiver signature captured
```

**Response:**
```json
{
  "success": true,
  "documentId": "DOC-2024-10001",
  "fileUrl": "https://storage.packtrack.com/pod/2024/02/21/DOC-2024-10001.jpg",
  "uploadedAt": "2026-02-21T15:15:00Z",
  "message": "Proof of delivery uploaded successfully."
}
```

---

### 11. Get Weather Alerts
**Endpoint:** `GET /api/transport/weather-alerts`

**Response:**
```json
{
  "currentLocation": {
    "latitude": 19.0760,
    "longitude": 72.8777,
    "city": "Mumbai"
  },
  "currentWeather": {
    "temperature": 32,
    "condition": "Partly Cloudy",
    "humidity": 75,
    "windSpeed": 15
  },
  "alerts": [
    {
      "type": "HeavyRain",
      "severity": "Warning",
      "message": "Heavy rain expected between 16:00 - 18:00",
      "validFrom": "2026-02-21T16:00:00Z",
      "validTo": "2026-02-21T18:00:00Z",
      "affectedAreas": ["Andheri", "Bandra", "Juhu"],
      "recommendations": [
        "Reduce speed",
        "Increase following distance",
        "Consider rescheduling non-urgent deliveries"
      ]
    }
  ],
  "routeWeather": [
    {
      "location": "Andheri",
      "time": "15:00",
      "condition": "Cloudy",
      "rainProbability": 80
    }
  ]
}
```

---

### 12. Send Emergency Alert
**Endpoint:** `POST /api/transport/emergency-alert`

**Request:**
```json
{
  "truckId": "TRK-MH-2024",
  "emergencyType": "Accident" | "Medical" | "Breakdown" | "Theft" | "Other",
  "location": {
    "latitude": 19.0760,
    "longitude": 72.8777
  },
  "message": "Vehicle breakdown. Need immediate assistance.",
  "requiresPolice": false,
  "requiresAmbulance": false,
  "requiresTowTruck": true
}
```

**Response:**
```json
{
  "success": true,
  "alertId": "ALERT-2024-001",
  "status": "Dispatched",
  "estimatedArrival": "2026-02-21T15:00:00Z",
  "emergencyContacts": [
    {
      "service": "Tow Truck",
      "provider": "Mumbai Towing Services",
      "phone": "+91-9876543210",
      "eta": 20
    },
    {
      "service": "Fleet Manager",
      "name": "Suresh Patil",
      "phone": "+91-9876543211"
    }
  ],
  "message": "Emergency alert sent. Help is on the way."
}
```

---

### 13. Get Maintenance Schedule
**Endpoint:** `GET /api/transport/maintenance-schedule`

**Response:**
```json
{
  "truckId": "TRK-MH-2024",
  "lastMaintenance": {
    "date": "2026-01-15",
    "type": "Regular Service",
    "mileage": 45000,
    "cost": 8500.00,
    "serviceCenter": "Authorized Service Center Mumbai"
  },
  "nextMaintenance": {
    "dueDate": "2026-03-15",
    "dueType": "Date",
    "dueMileage": 50000,
    "currentMileage": 47500,
    "remainingKm": 2500,
    "type": "Regular Service",
    "estimatedCost": 9000.00
  },
  "upcomingTasks": [
    {
      "task": "Oil Change",
      "dueIn": "500 km",
      "priority": "Medium"
    },
    {
      "task": "Tire Rotation",
      "dueIn": "1000 km",
      "priority": "Low"
    }
  ],
  "alerts": [
    {
      "type": "MaintenanceDue",
      "message": "Regular service due in 2500 km",
      "severity": "Info"
    }
  ]
}
```

---

## Offline Mode Support

### Data Cached Locally
- Active deliveries
- Route information
- Last 7 days trip history
- Truck details
- Emergency contacts

### Actions Available Offline
- View dashboard (cached data)
- Scan QR codes (queued for sync)
- Mark deliveries complete (queued for sync)
- Take photos/signatures (stored locally)
- View route map (if previously loaded)

### Sync Strategy
- Auto-sync when network available
- Manual sync button
- Conflict resolution: Server wins for status, merge for scans
- Queue size limit: 100 actions

---

## Push Notifications

### Notification Types
1. **New Delivery Assigned** - High priority
2. **Route Updated** - Medium priority
3. **Weather Alert** - High priority
4. **Delivery Time Window Approaching** - High priority
5. **Fuel Low** - Medium priority
6. **Maintenance Due** - Low priority
7. **Emergency Alert Response** - Critical priority
8. **Message from Fleet Manager** - Medium priority

---

## Security Features

### Authentication
- JWT tokens with 8-hour expiry
- Refresh token mechanism
- Device binding
- Biometric authentication support

### Data Protection
- End-to-end encryption for sensitive data
- Secure storage for offline data
- Photo/signature encryption
- GPS data anonymization (when off-duty)

### Access Control
- Role-based permissions
- Geofencing validation
- Time-based access (shift hours)
- Action audit logging

---

## Performance Requirements

- Dashboard load time: < 2 seconds
- QR scan response: < 1 second
- GPS update frequency: Every 30 seconds (when moving)
- Offline mode: Support 24 hours without sync
- Photo upload: Max 5MB per image
- Battery optimization: Background GPS with adaptive frequency
