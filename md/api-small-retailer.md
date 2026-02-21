# Small Retailer APIs - Detailed Specification

## Role Overview
Small retailers use the mobile app for inventory management, sales tracking, QR scanning for stock verification, shelf optimization, customer orders, and demand forecasting.

## Dashboard Overview

### Key Metrics Displayed
- Current stock levels by bottle type
- Today's sales revenue
- Low stock alerts
- Expiring products
- Customer orders pending
- Shelf occupancy
- Demand forecast

---

## API Specifications

### 1. Get Retailer Dashboard
**Endpoint:** `GET /api/retailer/dashboard`

**Headers:**
```json
{
  "Authorization": "Bearer {jwt_token}",
  "Retailer-Id": "{retailer_id}"
}
```

**Response:**
```json
{
  "retailerId": 101,
  "retailerName": "SuperMart Andheri",
  "ownerName": "Amit Shah",
  "location": {
    "address": "Shop 5, Andheri West, Mumbai - 400058",
    "city": "Mumbai",
    "region": "Maharashtra"
  },
  "todayStats": {
    "salesCount": 145,
    "salesRevenue": 14500.00,
    "averageTransaction": 100.00,
    "topSellingProduct": "Thick 500ml",
    "stockValue": 125000.00,
    "profitMargin": 25.5
  },
  "inventorySummary": {
    "totalUnits": 1250,
    "thick": {
      "500ml": 450,
      "1000ml": 200
    },
    "thin": {
      "500ml": 300,
      "1000ml": 150
    },
    "rPET": {
      "500ml": 100,
      "1000ml": 50
    }
  },
  "alerts": [
    {
      "type": "LowStock",
      "message": "Thick 500ml stock below threshold (450 units)",
      "severity": "High",
      "actionRequired": "Reorder"
    },
    {
      "type": "Expiry",
      "message": "15 units expiring in 7 days",
      "severity": "Medium",
      "actionRequired": "Promote"
    }
  ],
  "pendingOrders": 3,
  "shelfOccupancy": 78.5,
  "demandForecast": {
    "nextWeek": {
      "thick500ml": 600,
      "thin500ml": 400,
      "rPET500ml": 150
    },
    "confidence": 85
  }
}
```

---

### 2. Scan QR Code
**Endpoint:** `POST /api/retailer/scan-qr`

**Request:**
```json
{
  "qrCode": "ECO-B-20260221-3130-01-0005",
  "scanType": "StockIn" | "StockOut" | "Verification" | "Sale" | "Return",
  "retailerId": 101,
  "timestamp": "2026-02-21T14:30:00Z",
  "quantity": 1,
  "rackId": "RACK-A-01",
  "notes": "Stock received from wholesaler"
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
    "daysToExpiry": 181,
    "mrp": 20.00,
    "wholesalePrice": 15.00,
    "currentStatus": "Retail"
  },
  "inventoryUpdated": true,
  "currentStock": {
    "bottleType": "Thick",
    "capacityML": 500,
    "totalUnits": 451
  },
  "movementLogged": true,
  "message": "Product scanned and added to inventory."
}
```

---

### 3. Get Inventory
**Endpoint:** `GET /api/retailer/inventory`

**Query Parameters:**
- `bottleType`: "Thick" | "Thin" | "rPET" (optional)
- `capacityML`: 500 | 1000 (optional)
- `status`: "InStock" | "LowStock" | "OutOfStock" | "Expiring" (optional)

**Response:**
```json
{
  "retailerId": 101,
  "lastUpdated": "2026-02-21T14:30:00Z",
  "inventory": [
    {
      "itemId": "INV-001",
      "bottleType": "Thick",
      "capacityML": 500,
      "currentStock": 450,
      "minThreshold": 200,
      "maxCapacity": 800,
      "status": "LowStock",
      "stockValue": 6750.00,
      "averageDailySales": 75,
      "daysOfStock": 6,
      "reorderPoint": 200,
      "suggestedReorder": 350,
      "racks": [
        {
          "rackId": "RACK-A-01",
          "rackCode": "A-01",
          "capacity": 400,
          "currentUnits": 250,
          "occupancy": 62.5
        },
        {
          "rackId": "RACK-A-02",
          "rackCode": "A-02",
          "capacity": 400,
          "currentUnits": 200,
          "occupancy": 50.0
        }
      ],
      "expiringUnits": [
        {
          "batchCode": "B-20260115-2001",
          "quantity": 15,
          "expiryDate": "2026-02-28",
          "daysToExpiry": 7
        }
      ]
    },
    {
      "itemId": "INV-002",
      "bottleType": "rPET",
      "capacityML": 500,
      "currentStock": 100,
      "minThreshold": 50,
      "maxCapacity": 300,
      "status": "InStock",
      "stockValue": 1500.00,
      "averageDailySales": 15,
      "daysOfStock": 6.7,
      "reorderPoint": 50,
      "suggestedReorder": 0,
      "racks": [
        {
          "rackId": "RACK-B-01",
          "rackCode": "B-01",
          "capacity": 300,
          "currentUnits": 100,
          "occupancy": 33.3
        }
      ],
      "expiringUnits": []
    }
  ],
  "summary": {
    "totalItems": 6,
    "totalUnits": 1250,
    "totalValue": 18750.00,
    "lowStockItems": 2,
    "outOfStockItems": 0,
    "expiringItems": 1
  }
}
```

---

### 4. Record Sale
**Endpoint:** `POST /api/retailer/record-sale`

**Request:**
```json
{
  "retailerId": 101,
  "saleType": "Cash" | "Card" | "UPI" | "Credit",
  "items": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "quantity": 12,
      "unitPrice": 20.00,
      "discount": 0,
      "total": 240.00
    },
    {
      "bottleType": "rPET",
      "capacityML": 1000,
      "quantity": 6,
      "unitPrice": 35.00,
      "discount": 10.00,
      "total": 200.00
    }
  ],
  "subtotal": 440.00,
  "tax": 79.20,
  "discount": 10.00,
  "totalAmount": 509.20,
  "paymentMethod": "UPI",
  "customerInfo": {
    "name": "Rahul Verma",
    "phone": "+91-9876543210",
    "loyaltyId": "LOY-2024-001"
  },
  "timestamp": "2026-02-21T14:45:00Z",
  "notes": "Bulk purchase discount applied"
}
```

**Response:**
```json
{
  "success": true,
  "saleId": "SALE-2024-10001",
  "invoiceNumber": "INV-2024-10001",
  "invoiceUrl": "https://storage.packtrack.com/invoices/2024/02/21/INV-2024-10001.pdf",
  "totalAmount": 509.20,
  "paymentStatus": "Paid",
  "inventoryUpdated": true,
  "updatedStock": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "previousStock": 450,
      "currentStock": 438
    },
    {
      "bottleType": "rPET",
      "capacityML": 1000,
      "previousStock": 50,
      "currentStock": 44
    }
  ],
  "loyaltyPoints": {
    "earned": 50,
    "totalPoints": 550
  },
  "message": "Sale recorded successfully. Invoice generated."
}
```

---

### 5. Create Customer Order
**Endpoint:** `POST /api/retailer/create-order`

**Request:**
```json
{
  "retailerId": 101,
  "orderType": "Delivery" | "Pickup",
  "customerInfo": {
    "name": "Priya Sharma",
    "phone": "+91-9876543211",
    "email": "priya@example.com",
    "address": "Flat 201, Building A, Andheri West, Mumbai - 400058"
  },
  "items": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "quantity": 24,
      "unitPrice": 20.00
    }
  ],
  "deliveryDate": "2026-02-22",
  "deliveryTime": "10:00-12:00",
  "paymentMethod": "COD",
  "notes": "Call before delivery",
  "timestamp": "2026-02-21T15:00:00Z"
}
```

**Response:**
```json
{
  "success": true,
  "orderId": "ORD-2024-001",
  "orderNumber": "ORD-2024-001",
  "status": "Pending",
  "totalAmount": 480.00,
  "estimatedDelivery": "2026-02-22T11:00:00Z",
  "trackingUrl": "https://packtrack.com/track/ORD-2024-001",
  "message": "Order created successfully. Customer will be notified."
}
```

---

### 6. Get Stock Alerts
**Endpoint:** `GET /api/retailer/stock-alerts`

**Response:**
```json
{
  "retailerId": 101,
  "alerts": [
    {
      "alertId": "ALERT-001",
      "type": "LowStock",
      "severity": "High",
      "bottleType": "Thick",
      "capacityML": 500,
      "currentStock": 450,
      "threshold": 200,
      "daysOfStock": 6,
      "suggestedReorder": 350,
      "estimatedCost": 5250.00,
      "message": "Stock running low. Reorder recommended.",
      "createdAt": "2026-02-21T14:00:00Z"
    },
    {
      "alertId": "ALERT-002",
      "type": "Expiry",
      "severity": "Medium",
      "bottleType": "Thick",
      "capacityML": 500,
      "batchCode": "B-20260115-2001",
      "quantity": 15,
      "expiryDate": "2026-02-28",
      "daysToExpiry": 7,
      "suggestedAction": "Promote with discount",
      "message": "15 units expiring in 7 days. Consider promotion.",
      "createdAt": "2026-02-21T06:00:00Z"
    },
    {
      "alertId": "ALERT-003",
      "type": "HighDemand",
      "severity": "Low",
      "bottleType": "rPET",
      "capacityML": 1000,
      "currentStock": 50,
      "averageDailySales": 15,
      "forecastedDemand": 120,
      "message": "Demand increasing. Consider stocking up.",
      "createdAt": "2026-02-21T12:00:00Z"
    }
  ],
  "summary": {
    "totalAlerts": 3,
    "highSeverity": 1,
    "mediumSeverity": 1,
    "lowSeverity": 1
  }
}
```

---

### 7. Update Prices
**Endpoint:** `POST /api/retailer/update-prices`

**Request:**
```json
{
  "retailerId": 101,
  "priceUpdates": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "currentPrice": 20.00,
      "newPrice": 22.00,
      "effectiveFrom": "2026-02-22",
      "reason": "Wholesale price increase"
    },
    {
      "bottleType": "rPET",
      "capacityML": 1000,
      "currentPrice": 35.00,
      "newPrice": 32.00,
      "effectiveFrom": "2026-02-22",
      "reason": "Promotional discount"
    }
  ],
  "timestamp": "2026-02-21T15:30:00Z"
}
```

**Response:**
```json
{
  "success": true,
  "updatedItems": 2,
  "priceHistory": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "oldPrice": 20.00,
      "newPrice": 22.00,
      "changePercentage": 10.0,
      "effectiveFrom": "2026-02-22"
    }
  ],
  "message": "Prices updated successfully. Changes effective from 2026-02-22."
}
```

---

### 8. Get Sales History
**Endpoint:** `GET /api/retailer/sales-history`

**Query Parameters:**
- `startDate`: "2026-02-01"
- `endDate`: "2026-02-21"
- `groupBy`: "Day" | "Week" | "Month"
- `page`: 1
- `pageSize`: 20

**Response:**
```json
{
  "retailerId": 101,
  "period": {
    "startDate": "2026-02-01",
    "endDate": "2026-02-21"
  },
  "sales": [
    {
      "date": "2026-02-21",
      "salesCount": 145,
      "totalRevenue": 14500.00,
      "totalProfit": 3625.00,
      "averageTransaction": 100.00,
      "topProduct": "Thick 500ml",
      "paymentMethods": {
        "cash": 5800.00,
        "card": 4350.00,
        "upi": 4350.00
      }
    },
    {
      "date": "2026-02-20",
      "salesCount": 132,
      "totalRevenue": 13200.00,
      "totalProfit": 3300.00,
      "averageTransaction": 100.00,
      "topProduct": "Thick 500ml",
      "paymentMethods": {
        "cash": 5280.00,
        "card": 3960.00,
        "upi": 3960.00
      }
    }
  ],
  "summary": {
    "totalSales": 2850,
    "totalRevenue": 285000.00,
    "totalProfit": 71250.00,
    "averageDailySales": 135.7,
    "averageDailyRevenue": 13571.43,
    "profitMargin": 25.0,
    "topSellingProducts": [
      {
        "bottleType": "Thick",
        "capacityML": 500,
        "unitsSold": 1500,
        "revenue": 30000.00
      }
    ]
  },
  "pagination": {
    "currentPage": 1,
    "totalPages": 2,
    "totalRecords": 21
  }
}
```

---

### 9. Generate Invoice
**Endpoint:** `POST /api/retailer/generate-invoice`

**Request:**
```json
{
  "saleId": "SALE-2024-10001",
  "retailerId": 101,
  "invoiceType": "Tax" | "Retail" | "Wholesale",
  "format": "PDF" | "Email" | "Print",
  "customerEmail": "customer@example.com",
  "includeQR": true,
  "includeLogo": true
}
```

**Response:**
```json
{
  "success": true,
  "invoiceId": "INV-2024-10001",
  "invoiceNumber": "INV-2024-10001",
  "invoiceUrl": "https://storage.packtrack.com/invoices/2024/02/21/INV-2024-10001.pdf",
  "qrCode": "INV-QR-2024-10001",
  "emailSent": true,
  "message": "Invoice generated and sent to customer."
}
```

---

### 10. Get Expiry Alerts
**Endpoint:** `GET /api/retailer/expiry-alerts`

**Query Parameters:**
- `daysThreshold`: 30 (default: products expiring within 30 days)

**Response:**
```json
{
  "retailerId": 101,
  "threshold": 30,
  "expiringProducts": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "batchCode": "B-20260115-2001",
      "quantity": 15,
      "manufactureDate": "2026-01-15",
      "expiryDate": "2026-02-28",
      "daysToExpiry": 7,
      "currentPrice": 20.00,
      "stockValue": 300.00,
      "rackLocation": "RACK-A-01",
      "urgency": "High",
      "suggestedActions": [
        "Apply 20% discount",
        "Bundle with popular items",
        "Promote on social media"
      ]
    },
    {
      "bottleType": "Thin",
      "capacityML": 1000,
      "batchCode": "B-20260120-2050",
      "quantity": 25,
      "manufactureDate": "2026-01-20",
      "expiryDate": "2026-03-15",
      "daysToExpiry": 22,
      "currentPrice": 30.00,
      "stockValue": 750.00,
      "rackLocation": "RACK-B-02",
      "urgency": "Medium",
      "suggestedActions": [
        "Feature in window display",
        "Offer combo deals"
      ]
    }
  ],
  "summary": {
    "totalExpiringUnits": 40,
    "totalValue": 1050.00,
    "highUrgency": 1,
    "mediumUrgency": 1,
    "lowUrgency": 0
  }
}
```

---

### 11. Apply Promotion
**Endpoint:** `POST /api/retailer/apply-promotion`

**Request:**
```json
{
  "retailerId": 101,
  "promotionType": "Discount" | "BOGO" | "Bundle" | "Clearance",
  "applicableItems": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "batchCode": "B-20260115-2001"
    }
  ],
  "discountType": "Percentage" | "Fixed",
  "discountValue": 20.0,
  "startDate": "2026-02-22",
  "endDate": "2026-02-28",
  "conditions": {
    "minQuantity": 1,
    "maxQuantity": 15
  },
  "promotionName": "Clearance Sale - Expiring Soon",
  "description": "20% off on selected items expiring soon"
}
```

**Response:**
```json
{
  "success": true,
  "promotionId": "PROMO-2024-001",
  "status": "Active",
  "affectedItems": 1,
  "estimatedImpact": {
    "potentialRevenue": 240.00,
    "potentialProfit": 60.00
  },
  "message": "Promotion created and activated successfully."
}
```

---

### 12. Get Demand Forecast
**Endpoint:** `GET /api/retailer/demand-forecast`

**Query Parameters:**
- `period`: "Week" | "Month" | "Quarter"
- `bottleType`: "Thick" | "Thin" | "rPET" (optional)

**Response:**
```json
{
  "retailerId": 101,
  "period": "Week",
  "forecastDate": "2026-02-22 to 2026-02-28",
  "forecasts": [
    {
      "bottleType": "Thick",
      "capacityML": 500,
      "currentStock": 450,
      "averageDailySales": 75,
      "forecastedDemand": 600,
      "confidence": 85,
      "trend": "Increasing",
      "seasonalFactor": 1.15,
      "recommendedOrder": 350,
      "estimatedCost": 5250.00,
      "estimatedRevenue": 12000.00,
      "estimatedProfit": 3000.00
    },
    {
      "bottleType": "rPET",
      "capacityML": 500,
      "currentStock": 100,
      "averageDailySales": 15,
      "forecastedDemand": 120,
      "confidence": 78,
      "trend": "Stable",
      "seasonalFactor": 1.05,
      "recommendedOrder": 50,
      "estimatedCost": 750.00,
      "estimatedRevenue": 2400.00,
      "estimatedProfit": 600.00
    }
  ],
  "insights": [
    {
      "type": "Trend",
      "message": "Thick 500ml demand increasing by 15% due to seasonal factors"
    },
    {
      "type": "Recommendation",
      "message": "Consider bulk ordering Thick 500ml to meet forecasted demand"
    }
  ],
  "summary": {
    "totalForecastedDemand": 1450,
    "totalRecommendedOrder": 750,
    "estimatedInvestment": 11250.00,
    "estimatedReturn": 29000.00,
    "roi": 157.8
  }
}
```

---

### 13. Optimize Shelf Layout
**Endpoint:** `POST /api/retailer/optimize-shelf`

**Request:**
```json
{
  "retailerId": 101,
  "optimizationGoal": "MaximizeSales" | "MinimizeWastage" | "BalanceStock",
  "constraints": {
    "totalShelfSpace": 2000,
    "rackCount": 5,
    "preferredProducts": ["Thick 500ml", "rPET 500ml"]
  },
  "currentLayout": [
    {
      "rackId": "RACK-A-01",
      "capacity": 400,
      "currentProduct": "Thick 500ml",
      "currentUnits": 250
    }
  ]
}
```

**Response:**
```json
{
  "success": true,
  "optimizationScore": 92,
  "recommendedLayout": [
    {
      "rackId": "RACK-A-01",
      "rackCode": "A-01",
      "recommendedProduct": "Thick 500ml",
      "recommendedCapacity": 400,
      "recommendedUnits": 350,
      "reason": "High demand product - prime location",
      "expectedSales": 75,
      "expectedRevenue": 1500.00
    },
    {
      "rackId": "RACK-A-02",
      "rackCode": "A-02",
      "recommendedProduct": "rPET 500ml",
      "recommendedCapacity": 300,
      "recommendedUnits": 200,
      "reason": "Growing demand - visible location",
      "expectedSales": 20,
      "expectedRevenue": 400.00
    }
  ],
  "improvements": [
    {
      "metric": "Sales",
      "currentValue": 135,
      "projectedValue": 155,
      "improvement": 14.8
    },
    {
      "metric": "Wastage",
      "currentValue": 5,
      "projectedValue": 2,
      "improvement": 60.0
    }
  ],
  "message": "Shelf layout optimized. Implement recommendations for better performance."
}
```

---

## Offline Mode Support

### Data Cached Locally
- Current inventory
- Last 30 days sales history
- Customer orders
- Price list
- Expiry alerts

### Actions Available Offline
- View dashboard (cached data)
- Scan QR codes (queued for sync)
- Record sales (queued for sync)
- View inventory (cached data)
- Generate invoices (queued for sync)

### Sync Strategy
- Auto-sync every 10 minutes when online
- Manual sync button
- Priority sync: Sales, inventory updates
- Queue size limit: 500 actions
- Conflict resolution: Server wins for inventory, merge for sales

---

## Push Notifications

### Notification Types
1. **Low Stock Alert** - High priority
2. **Expiry Alert** - High priority
3. **New Order Received** - Medium priority
4. **Payment Received** - Medium priority
5. **Promotion Ending Soon** - Low priority
6. **Daily Sales Report** - Low priority
7. **Demand Forecast Update** - Low priority
8. **Price Update** - Medium priority

---

## Performance Requirements

- Dashboard load time: < 2 seconds
- QR scan response: < 1 second
- Sale recording: < 2 seconds
- Invoice generation: < 3 seconds
- Inventory sync: < 5 seconds
- Offline mode: Support 24 hours without sync
- Photo upload: Max 3MB per image
