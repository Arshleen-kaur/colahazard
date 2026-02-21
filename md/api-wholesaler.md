# Wholesaler APIs - Detailed Specification

## Role Overview
Wholesalers use the mobile app for bulk order management, distribution planning, warehouse operations, retailer network management, and regional analytics.

## Dashboard Overview

### Key Metrics Displayed
- Total warehouse inventory
- Active orders count
- Retailer network size
- Today's revenue
- Pending deliveries
- Credit outstanding
- Regional performance

---

## API Specifications

### 1. Get Wholesaler Dashboard
**Endpoint:** `GET /api/wholesaler/dashboard`

**Headers:**
```json
{
  "Authorization": "Bearer {jwt_token}",
  "Wholesaler-Id": "{wholesaler_id}"
}
```

**Response:**
```json
{
  "wholesalerId": 201,
  "wholesalerName": "Metro Distributors Mumbai",
  "ownerName": "Suresh Patil",
  "location": {
    "address": "Warehouse Complex, Bhiwandi, Mumbai - 421302",
    "city": "Mumbai",
    "region": "Maharashtra"
  },
  "todayStats": {
    "ordersReceived": 45,
    "ordersProcessed": 38,
    "ordersPending": 7,
    "totalRevenue": 450000.00,
    "totalProfit": 67500.00,
    "deliveriesCompleted": 32,
    "deliveriesPending": 13
  },
  "inventorySummary": {
    "totalUnits": 125000,
    "totalValue": 1875000.00,
    "warehouseCapacity": 200000,
    "occupancyPercentage": 62.5,
    "byBottleType": {
      "thick": {
        "500ml": 45000,
        "1000ml": 20000
      },
      "thin": {
        "500ml": 30000,
        "1000ml": 15000
      },
      "rPET": {
        "500ml": 10000,
        "1000ml": 5000
      }
    }
  },
  "retailerNetwork": {
    "totalRetailers": 150,
    "activeRetailers": 142,
    "newRetailersThisMonth": 8,
    "topRetailers": [
      {
        "retailerId": 101,
        "name": "SuperMart Andheri",
        "monthlyOrders": 25,
        "monthlyRevenue": 125000.00
      }
    ]
  },
  "creditSummary": {
    "totalCreditExtended": 2500000.00,
    "totalOutstanding": 450000.00,
    "overdueAmount": 75000.00,
    "creditUtilization": 18.0
  },
  "alerts": [
    {
      "type": "LowStock",
      "message": "Thick 500ml stock below reorder point",
      "severity": "High"
    },
    {
      "type": "CreditOverdue",
      "message": "5 retailers have overdue payments",
      "severity": "Medium"
    }
  ]
}
```

---

### 2. Scan QR Code
**Endpoint:** `POST /api/wholesaler/scan-qr`

**Request:**
```json
{
  "qrCode": "ECO-PLT-20260221-001",
  "scanType": "Receive" | "Dispatch" | "Verification" | "Audit",
  "wholesalerId": 201,
  "timestamp": "2026-02-21T14:30:00Z",
  "location": "Warehouse Zone A",
  "notes": "Pallet received from factory"
}
```

**Response:**
```json
{
  "success": true,
  "qrDetails": {
    "palletId": 501,
    "palletCode": "PLT-20260221-001",
    "batchCode": "B-20260221-3130",
    "totalUnits": 240,
    "bottleType": "Thick",
    "capacityML": 500,
    "manufactureDate": "2026-02-21",
    "expiryDate": "2026-08-21"
  },
  "inventoryUpdated": true,
  "warehouseLocation": "Zone-A-Rack-05",
  "message": "Pallet scanned and added to warehouse inventory."
}
```

---

### 3. Create Bulk Order
**Endpoint:** `POST /api/wholesaler/create-bulk-order`

**Request:**
```json
{
  "wholesalerId": 201,
  "supplierId": 1,
  "orderType": "Regular" | "Urgent" | "Scheduled",
  "items": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "quantity": 10000,
      "unitPrice": 15.00,
      "total": 150000.00
    },
    {
      "bottleType": "rPET",
      "capacityML": 1000,
      "quantity": 5000,
      "unitPrice": 25.00,
      "total": 125000.00
    }
  ],
  "subtotal": 275000.00,
  "tax": 49500.00,
  "totalAmount": 324500.00,
  "deliveryDate": "2026-02-25",
  "deliveryAddress": "Warehouse Complex, Bhiwandi, Mumbai",
  "paymentTerms": "Net30",
  "notes": "Urgent order for upcoming festival season"
}
```

**Response:**
```json
{
  "success": true,
  "orderId": "WHL-ORD-2024-001",
  "orderNumber": "WHL-ORD-2024-001",
  "status": "Pending",
  "totalAmount": 324500.00,
  "estimatedDelivery": "2026-02-25T10:00:00Z",
  "supplierConfirmation": "Pending",
  "message": "Bulk order created successfully. Awaiting supplier confirmation."
}
```

---

### 4. Get Retailer Network
**Endpoint:** `GET /api/wholesaler/retailer-network`

**Query Parameters:**
- `status`: "Active" | "Inactive" | "All"
- `region`: "Mumbai" | "Pune" | "All"
- `sortBy`: "Name" | "Revenue" | "Orders"
- `page`: 1
- `pageSize`: 20

**Response:**
```json
{
  "wholesalerId": 201,
  "retailers": [
    {
      "retailerId": 101,
      "retailerName": "SuperMart Andheri",
      "ownerName": "Amit Shah",
      "contactPhone": "+91-9876543210",
      "address": "Shop 5, Andheri West, Mumbai - 400058",
      "city": "Mumbai",
      "region": "Maharashtra",
      "status": "Active",
      "registeredDate": "2025-06-15",
      "creditLimit": 50000.00,
      "creditUsed": 12000.00,
      "creditAvailable": 38000.00,
      "outstandingAmount": 12000.00,
      "monthlyStats": {
        "ordersPlaced": 25,
        "totalRevenue": 125000.00,
        "averageOrderValue": 5000.00,
        "paymentScore": 95
      },
      "lastOrderDate": "2026-02-20",
      "performance": "Excellent"
    }
  ],
  "summary": {
    "totalRetailers": 150,
    "activeRetailers": 142,
    "inactiveRetailers": 8,
    "totalCreditExtended": 2500000.00,
    "totalOutstanding": 450000.00
  },
  "pagination": {
    "currentPage": 1,
    "totalPages": 8,
    "totalRecords": 150
  }
}
```

---

### 5. Approve Credit
**Endpoint:** `POST /api/wholesaler/approve-credit`

**Request:**
```json
{
  "wholesalerId": 201,
  "retailerId": 101,
  "creditType": "New" | "Increase" | "Renewal",
  "requestedAmount": 50000.00,
  "approvedAmount": 50000.00,
  "creditPeriod": 30,
  "interestRate": 0,
  "collateral": "None",
  "terms": "Net 30 days payment terms",
  "approvedBy": "Suresh Patil",
  "notes": "Good payment history. Approved full amount."
}
```

**Response:**
```json
{
  "success": true,
  "creditId": "CREDIT-2024-001",
  "retailerId": 101,
  "approvedAmount": 50000.00,
  "creditLimit": 50000.00,
  "validFrom": "2026-02-21",
  "validUntil": "2027-02-21",
  "status": "Active",
  "message": "Credit approved successfully. Retailer notified."
}
```

---

### 6. Get Warehouse Status
**Endpoint:** `GET /api/wholesaler/warehouse-status`

**Response:**
```json
{
  "wholesalerId": 201,
  "warehouseName": "Metro Distribution Center",
  "totalCapacity": 200000,
  "currentStock": 125000,
  "occupancyPercentage": 62.5,
  "availableSpace": 75000,
  "zones": [
    {
      "zoneId": "ZONE-A",
      "zoneName": "Zone A - Premium Products",
      "capacity": 50000,
      "currentStock": 35000,
      "occupancy": 70.0,
      "temperature": 25.5,
      "humidity": 45,
      "status": "Operational",
      "assignedProducts": ["Thick 500ml", "Thick 1000ml"]
    },
    {
      "zoneId": "ZONE-B",
      "zoneName": "Zone B - Standard Products",
      "capacity": 80000,
      "currentStock": 55000,
      "occupancy": 68.75,
      "temperature": 26.0,
      "humidity": 48,
      "status": "Operational",
      "assignedProducts": ["Thin 500ml", "Thin 1000ml"]
    },
    {
      "zoneId": "ZONE-C",
      "zoneName": "Zone C - Eco Products",
      "capacity": 40000,
      "currentStock": 20000,
      "occupancy": 50.0,
      "temperature": 24.5,
      "humidity": 42,
      "status": "Operational",
      "assignedProducts": ["rPET 500ml", "rPET 1000ml"]
    }
  ],
  "recentActivity": [
    {
      "timestamp": "2026-02-21T14:30:00Z",
      "activity": "Received",
      "quantity": 2400,
      "product": "Thick 500ml",
      "zone": "ZONE-A"
    }
  ],
  "alerts": [
    {
      "type": "HighOccupancy",
      "zone": "ZONE-A",
      "message": "Zone A occupancy at 70%. Consider redistribution.",
      "severity": "Medium"
    }
  ]
}
```

---

### 7. Plan Distribution
**Endpoint:** `POST /api/wholesaler/plan-distribution`

**Request:**
```json
{
  "wholesalerId": 201,
  "distributionDate": "2026-02-22",
  "deliveries": [
    {
      "retailerId": 101,
      "items": [
        {
          "bottleType": "Thick",
          "capacityML": 500,
          "quantity": 240
        }
      ],
      "priority": "High",
      "timeWindow": {
        "start": "10:00",
        "end": "12:00"
      }
    },
    {
      "retailerId": 102,
      "items": [
        {
          "bottleType": "rPET",
          "capacityML": 1000,
          "quantity": 120
        }
      ],
      "priority": "Medium",
      "timeWindow": {
        "start": "14:00",
        "end": "16:00"
      }
    }
  ],
  "optimizationGoal": "MinimizeDistance" | "MinimizeTime" | "MaximizeDeliveries",
  "availableTrucks": 3
}
```

**Response:**
```json
{
  "success": true,
  "distributionId": "DIST-2024-001",
  "distributionDate": "2026-02-22",
  "routes": [
    {
      "routeId": "ROUTE-001",
      "truckId": "TRK-MH-2024",
      "driverAssigned": "Rajesh Kumar",
      "deliveries": [
        {
          "sequence": 1,
          "retailerId": 101,
          "retailerName": "SuperMart Andheri",
          "estimatedArrival": "10:30",
          "distance": 15.5
        },
        {
          "sequence": 2,
          "retailerId": 103,
          "retailerName": "MegaStore Bandra",
          "estimatedArrival": "11:45",
          "distance": 8.2
        }
      ],
      "totalDistance": 23.7,
      "estimatedDuration": 120,
      "totalUnits": 480
    }
  ],
  "optimization": {
    "totalDistance": 45.5,
    "totalDuration": 240,
    "trucksUsed": 2,
    "deliveriesScheduled": 15,
    "efficiencyScore": 92
  },
  "message": "Distribution plan created. Routes optimized for minimum distance."
}
```

---

### 8. Get Pending Orders
**Endpoint:** `GET /api/wholesaler/pending-orders`

**Query Parameters:**
- `status`: "Pending" | "Processing" | "ReadyToShip"
- `priority`: "High" | "Medium" | "Low"
- `date`: "2026-02-21"

**Response:**
```json
{
  "wholesalerId": 201,
  "orders": [
    {
      "orderId": "ORD-2024-001",
      "retailerId": 101,
      "retailerName": "SuperMart Andheri",
      "orderDate": "2026-02-21T10:00:00Z",
      "requestedDeliveryDate": "2026-02-22",
      "priority": "High",
      "status": "Processing",
      "items": [
        {
          "bottleType": "Thick",
          "capacityML": 500,
          "quantity": 240,
          "unitPrice": 18.00,
          "total": 4320.00
        }
      ],
      "totalAmount": 4320.00,
      "paymentMethod": "Credit",
      "paymentStatus": "Pending",
      "fulfillmentStatus": "PartiallyPicked",
      "pickedUnits": 120,
      "remainingUnits": 120,
      "assignedWarehouse": "ZONE-A",
      "notes": "Urgent order for weekend sale"
    }
  ],
  "summary": {
    "totalOrders": 45,
    "pendingOrders": 7,
    "processingOrders": 25,
    "readyToShip": 13,
    "totalValue": 225000.00
  }
}
```

---

### 9. Process Return
**Endpoint:** `POST /api/wholesaler/process-return`

**Request:**
```json
{
  "wholesalerId": 201,
  "retailerId": 101,
  "returnType": "Defective" | "Expired" | "Overstocked" | "Damaged",
  "items": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "batchCode": "B-20260115-2001",
      "quantity": 24,
      "reason": "Approaching expiry date",
      "condition": "Good"
    }
  ],
  "returnDate": "2026-02-21",
  "pickupRequired": true,
  "pickupAddress": "Shop 5, Andheri West, Mumbai",
  "refundMethod": "Credit" | "Cash" | "Replacement",
  "photos": ["base64_encoded_photo"],
  "notes": "Products in good condition. Requesting credit note."
}
```

**Response:**
```json
{
  "success": true,
  "returnId": "RET-2024-001",
  "returnNumber": "RET-2024-001",
  "status": "Approved",
  "refundAmount": 360.00,
  "refundMethod": "Credit",
  "creditNoteNumber": "CN-2024-001",
  "pickupScheduled": true,
  "pickupDate": "2026-02-22",
  "pickupTimeWindow": "10:00-12:00",
  "message": "Return approved. Pickup scheduled. Credit note generated."
}
```

---

### 10. Get Regional Analytics
**Endpoint:** `GET /api/wholesaler/regional-analytics`

**Query Parameters:**
- `region`: "Mumbai" | "Pune" | "All"
- `period`: "Week" | "Month" | "Quarter"
- `startDate`: "2026-02-01"
- `endDate`: "2026-02-21"

**Response:**
```json
{
  "wholesalerId": 201,
  "period": {
    "startDate": "2026-02-01",
    "endDate": "2026-02-21"
  },
  "regions": [
    {
      "regionName": "Mumbai",
      "retailers": 85,
      "activeRetailers": 80,
      "totalOrders": 1250,
      "totalRevenue": 6250000.00,
      "totalProfit": 937500.00,
      "averageOrderValue": 5000.00,
      "topProducts": [
        {
          "bottleType": "Thick",
          "capacityML": 500,
          "unitsSold": 125000,
          "revenue": 2250000.00
        }
      ],
      "growth": {
        "ordersGrowth": 15.5,
        "revenueGrowth": 18.2,
        "retailerGrowth": 8.5
      },
      "marketShare": 65.0
    }
  ],
  "comparison": {
    "topRegion": "Mumbai",
    "fastestGrowing": "Pune",
    "highestRevenue": "Mumbai"
  },
  "insights": [
    {
      "type": "Opportunity",
      "message": "Pune region showing 25% growth. Consider expanding distribution."
    },
    {
      "type": "Alert",
      "message": "Mumbai region inventory turnover slowing. Review stock levels."
    }
  ]
}
```

---

### 11. Update Pricing Strategy
**Endpoint:** `POST /api/wholesaler/update-pricing`

**Request:**
```json
{
  "wholesalerId": 201,
  "pricingStrategy": "Volume" | "Seasonal" | "Promotional" | "Standard",
  "effectiveFrom": "2026-02-22",
  "priceUpdates": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "currentPrice": 18.00,
      "newPrice": 19.00,
      "reason": "Raw material cost increase",
      "applicableTo": "All" | "NewOrders" | "SpecificRetailers"
    }
  ],
  "volumeDiscounts": [
    {
      "minQuantity": 1000,
      "maxQuantity": 5000,
      "discountPercentage": 5.0
    },
    {
      "minQuantity": 5001,
      "maxQuantity": 10000,
      "discountPercentage": 8.0
    }
  ],
  "notifyRetailers": true
}
```

**Response:**
```json
{
  "success": true,
  "pricingId": "PRICE-2024-001",
  "updatedItems": 6,
  "effectiveFrom": "2026-02-22",
  "retailersNotified": 142,
  "estimatedImpact": {
    "revenueIncrease": 125000.00,
    "marginImprovement": 2.5
  },
  "message": "Pricing updated successfully. Retailers notified."
}
```

---

### 12. Get Performance Report
**Endpoint:** `GET /api/wholesaler/performance-report`

**Query Parameters:**
- `reportType`: "Sales" | "Inventory" | "Delivery" | "Financial" | "Comprehensive"
- `period`: "Week" | "Month" | "Quarter" | "Year"
- `format`: "JSON" | "PDF" | "Excel"

**Response:**
```json
{
  "wholesalerId": 201,
  "reportType": "Comprehensive",
  "period": "Month",
  "generatedAt": "2026-02-21T15:00:00Z",
  "salesPerformance": {
    "totalOrders": 1250,
    "totalRevenue": 6250000.00,
    "totalProfit": 937500.00,
    "averageOrderValue": 5000.00,
    "orderFulfillmentRate": 96.5,
    "onTimeDeliveryRate": 94.2
  },
  "inventoryPerformance": {
    "averageStockLevel": 120000,
    "stockTurnoverRatio": 4.5,
    "stockoutIncidents": 3,
    "excessStockValue": 125000.00,
    "inventoryAccuracy": 99.2
  },
  "deliveryPerformance": {
    "totalDeliveries": 1200,
    "onTimeDeliveries": 1130,
    "lateDeliveries": 70,
    "averageDeliveryTime": 45,
    "deliverySuccessRate": 98.5
  },
  "financialPerformance": {
    "totalRevenue": 6250000.00,
    "totalCost": 5312500.00,
    "grossProfit": 937500.00,
    "profitMargin": 15.0,
    "operatingExpenses": 312500.00,
    "netProfit": 625000.00
  },
  "retailerPerformance": {
    "activeRetailers": 142,
    "newRetailers": 8,
    "churnedRetailers": 2,
    "averageOrdersPerRetailer": 8.8,
    "topRetailers": [
      {
        "retailerId": 101,
        "name": "SuperMart Andheri",
        "orders": 25,
        "revenue": 125000.00
      }
    ]
  },
  "kpis": {
    "orderFulfillmentRate": 96.5,
    "onTimeDeliveryRate": 94.2,
    "inventoryTurnover": 4.5,
    "profitMargin": 15.0,
    "customerSatisfaction": 4.5
  },
  "reportUrl": "https://storage.packtrack.com/reports/2024/02/21/PERF-2024-001.pdf"
}
```

---

### 13. Coordinate with Supplier
**Endpoint:** `POST /api/wholesaler/coordinate-supplier`

**Request:**
```json
{
  "wholesalerId": 201,
  "supplierId": 1,
  "coordinationType": "Order" | "Delivery" | "Quality" | "Payment" | "General",
  "subject": "Urgent order requirement for festival season",
  "message": "Need 50,000 units of Thick 500ml by Feb 25. Can you expedite?",
  "priority": "High",
  "attachments": [],
  "requestedResponse": "Urgent"
}
```

**Response:**
```json
{
  "success": true,
  "messageId": "MSG-2024-001",
  "status": "Sent",
  "sentAt": "2026-02-21T15:30:00Z",
  "expectedResponse": "2026-02-21T18:00:00Z",
  "message": "Message sent to supplier. Awaiting response."
}
```

---

## Offline Mode Support

### Data Cached Locally
- Current inventory summary
- Retailer network (basic info)
- Pending orders
- Last 7 days analytics
- Warehouse status

### Actions Available Offline
- View dashboard (cached data)
- Scan QR codes (queued for sync)
- View retailer details (cached)
- View pending orders (cached)
- Create distribution plans (queued)

### Sync Strategy
- Auto-sync every 15 minutes when online
- Manual sync button
- Priority sync: Orders, payments, inventory updates
- Queue size limit: 1000 actions
- Conflict resolution: Server wins for inventory, merge for orders

---

## Push Notifications

### Notification Types
1. **New Order Received** - High priority
2. **Payment Received** - Medium priority
3. **Low Stock Alert** - High priority
4. **Credit Overdue** - High priority
5. **Delivery Completed** - Low priority
6. **Return Request** - Medium priority
7. **Supplier Message** - Medium priority
8. **Performance Report Ready** - Low priority

---

## Performance Requirements

- Dashboard load time: < 3 seconds
- QR scan response: < 1 second
- Order processing: < 3 seconds
- Report generation: < 5 seconds
- Distribution planning: < 10 seconds
- Offline mode: Support 48 hours without sync
- Real-time inventory updates: Every 5 minutes
