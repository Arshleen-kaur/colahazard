
from fastapi import APIRouter, HTTPException, UploadFile, File
from typing import List, Optional
from datetime import datetime

router = APIRouter()

# -------------------------------------------------------------------
# 🔹 MOCK DATABASE (temporary for hackathon demo)
# -------------------------------------------------------------------

FAKE_SHIPMENTS = {
    "SHP001": {
        "id": "SHP001",
        "status": "IN_TRANSIT",
        "driver_id": "DRV001",
        "recipient": "ABC Retail"
    }
}

# -------------------------------------------------------------------
# 1. HEALTH CHECK
# -------------------------------------------------------------------

@router.get("/health")
async def health_check():
    return {"status": "transport service running"}

# -------------------------------------------------------------------
# 2. LIST SHIPMENTS
# -------------------------------------------------------------------

@router.get("/")
async def list_shipments(status: Optional[str] = None):
    shipments = list(FAKE_SHIPMENTS.values())

    if status:
        shipments = [s for s in shipments if s["status"] == status]

    return shipments

# -------------------------------------------------------------------
# 3. GET SHIPMENT DETAILS
# -------------------------------------------------------------------

@router.get("/{shipment_id}")
async def get_shipment_details(shipment_id: str):
    shipment = FAKE_SHIPMENTS.get(shipment_id)

    if not shipment:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return shipment

# -------------------------------------------------------------------
# 4. START PICKUP
# -------------------------------------------------------------------

@router.post("/{shipment_id}/start-pickup")
async def start_pickup(shipment_id: str, lat: float, lon: float):
    shipment = FAKE_SHIPMENTS.get(shipment_id)

    if not shipment:
        raise HTTPException(status_code=404, detail="Shipment not found")

    shipment["status"] = "PICKUP_STARTED"

    return {"message": "Pickup started", "lat": lat, "lon": lon}

# -------------------------------------------------------------------
# 5. LOCATION UPDATE
# -------------------------------------------------------------------

@router.post("/{shipment_id}/location-update")
async def update_location(
    shipment_id: str,
    lat: float,
    lon: float,
    speed: Optional[float] = None,
):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return {
        "status": "location recorded",
        "lat": lat,
        "lon": lon,
        "speed": speed,
    }

# -------------------------------------------------------------------
# 6. MARK DELIVERED
# -------------------------------------------------------------------

@router.post("/{shipment_id}/deliver")
async def mark_delivered(shipment_id: str, recipient_name: str):
    shipment = FAKE_SHIPMENTS.get(shipment_id)

    if not shipment:
        raise HTTPException(status_code=404, detail="Shipment not found")

    shipment["status"] = "DELIVERED"
    shipment["recipient"] = recipient_name

    return {"message": "Shipment delivered successfully"}

# -------------------------------------------------------------------
# 7. UPLOAD SIGNATURE
# -------------------------------------------------------------------

@router.post("/{shipment_id}/signature")
async def upload_signature(
    shipment_id: str,
    file: UploadFile = File(...),
):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return {
        "message": "Signature uploaded",
        "filename": file.filename,
    }

# -------------------------------------------------------------------
# 8. REPORT ISSUE
# -------------------------------------------------------------------

@router.post("/{shipment_id}/report-issue")
async def report_issue(
    shipment_id: str,
    issue_type: str,
    description: str,
):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return {
        "message": "Issue reported",
        "type": issue_type,
        "description": description,
    }

# -------------------------------------------------------------------
# 9. WAYPOINTS (demo)
# -------------------------------------------------------------------

@router.get("/{shipment_id}/waypoints")
async def get_waypoints(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return [
        {"stop": "Factory", "status": "COMPLETED"},
        {"stop": "Warehouse", "status": "IN_PROGRESS"},
        {"stop": "Retailer", "status": "PENDING"},
    ]

# -------------------------------------------------------------------
# 10. TELEMETRY
# -------------------------------------------------------------------

@router.get("/{shipment_id}/telemetry")
async def get_telemetry(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return {
        "temperature": 5.2,
        "humidity": 62,
        "shock": 0.3,
        "timestamp": datetime.utcnow(),
    }

# -------------------------------------------------------------------
# 🔹 EXTRA MOCK STORAGE
# -------------------------------------------------------------------

FAKE_EVENTS = {}
FAKE_EXPENSES = {}
FAKE_NOTES = {}

# -------------------------------------------------------------------
# 11. DRIVER HISTORY SUMMARY
# -------------------------------------------------------------------

@router.get("/history/summary")
async def get_driver_history_summary():
    """
    Returns demo stats for dashboard.
    """
    return {
        "total_shipments": len(FAKE_SHIPMENTS),
        "delivered": sum(1 for s in FAKE_SHIPMENTS.values() if s["status"] == "DELIVERED"),
        "in_transit": sum(1 for s in FAKE_SHIPMENTS.values() if s["status"] == "IN_TRANSIT"),
        "avg_rating": 4.6
    }

# -------------------------------------------------------------------
# 12. ADD INTERNAL NOTE
# -------------------------------------------------------------------

@router.post("/{shipment_id}/notes")
async def add_internal_note(shipment_id: str, note: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    FAKE_NOTES.setdefault(shipment_id, []).append({
        "note": note,
        "timestamp": datetime.utcnow()
    })

    return {"message": "Note added"}

# -------------------------------------------------------------------
# 13. GET SHIPMENT NOTES
# -------------------------------------------------------------------

@router.get("/{shipment_id}/notes")
async def get_notes(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return FAKE_NOTES.get(shipment_id, [])

# -------------------------------------------------------------------
# 14. LOG FUEL EXPENSE (simplified)
# -------------------------------------------------------------------

@router.post("/{shipment_id}/fuel-log")
async def log_fuel(
    shipment_id: str,
    amount: float,
    liters: float,
    odometer: int,
):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    FAKE_EXPENSES.setdefault(shipment_id, []).append({
        "type": "fuel",
        "amount": amount,
        "liters": liters,
        "odometer": odometer,
        "timestamp": datetime.utcnow()
    })

    return {"message": "Fuel logged"}

# -------------------------------------------------------------------
# 15. GET EXPENSES
# -------------------------------------------------------------------

@router.get("/{shipment_id}/expenses")
async def get_expenses(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return FAKE_EXPENSES.get(shipment_id, [])

# -------------------------------------------------------------------
# 16. TIMELINE VIEW 
# -------------------------------------------------------------------

@router.get("/{shipment_id}/timeline")
async def get_timeline(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    shipment = FAKE_SHIPMENTS[shipment_id]

    return {
        "shipment_id": shipment_id,
        "current_status": shipment["status"],
        "events": [
            {"event": "CREATED", "time": "2025-01-01T10:00:00"},
            {"event": "PICKUP_STARTED", "time": "2025-01-01T12:00:00"},
            {"event": shipment["status"], "time": datetime.utcnow()}
        ]
    }

# -------------------------------------------------------------------
# 17. SIMPLE SEARCH 
# -------------------------------------------------------------------

@router.get("/search")
async def search_shipments(query: str):
    results = []

    for shipment in FAKE_SHIPMENTS.values():
        if query.lower() in shipment["id"].lower():
            results.append(shipment)

    return {
        "query": query,
        "count": len(results),
        "results": results
    }

# -------------------------------------------------------------------
# 18. DRIVER LEADERBOARD 
# -------------------------------------------------------------------

@router.get("/metrics/leaderboard")
async def get_leaderboard():
    return [
        {"driver": "DRV001", "score": 92},
        {"driver": "DRV002", "score": 88},
        {"driver": "DRV003", "score": 81},
    ]


# -------------------------------------------------------------------
# 19. ETA PREDICTION 
# -------------------------------------------------------------------

@router.get("/{shipment_id}/predict-delay")
async def predict_delay(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    shipment = FAKE_SHIPMENTS[shipment_id]

    # simple demo logic
    risk = "LOW"
    if shipment["status"] == "IN_TRANSIT":
        risk = "MEDIUM"

    return {
        "shipment_id": shipment_id,
        "delay_risk": risk,
        "predicted_eta_hours": 6,
        "confidence": 0.87
    }

# -------------------------------------------------------------------
# 20. DRIVER PANIC BUTTON 
# -------------------------------------------------------------------

@router.post("/{shipment_id}/panic")
async def trigger_panic(shipment_id: str, message: Optional[str] = None):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    FAKE_EVENTS.setdefault(shipment_id, []).append({
        "type": "PANIC_ALERT",
        "message": message or "Driver triggered emergency",
        "time": datetime.utcnow()
    })

    return {
        "status": "EMERGENCY_TRIGGERED",
        "dispatch_notified": True
    }

# -------------------------------------------------------------------
# 21. VEHICLE HEALTH (OBD mock)
# -------------------------------------------------------------------

@router.get("/{shipment_id}/vehicle-health")
async def get_vehicle_health(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return {
        "engine_temp": 88,
        "oil_life": 72,
        "tire_pressure_ok": True,
        "battery_health": "GOOD"
    }

# -------------------------------------------------------------------
# 22. CARBON FOOTPRINT 
# -------------------------------------------------------------------

@router.get("/{shipment_id}/carbon-footprint")
async def carbon_footprint(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    return {
        "shipment_id": shipment_id,
        "co2_kg": 124.6,
        "distance_km": 342,
        "efficiency_rating": "B+"
    }

# -------------------------------------------------------------------
# 23. SMART ALERT ENGINE 
# -------------------------------------------------------------------

@router.get("/{shipment_id}/alerts")
async def get_smart_alerts(shipment_id: str):
    if shipment_id not in FAKE_SHIPMENTS:
        raise HTTPException(status_code=404, detail="Shipment not found")

    alerts = []

    shipment = FAKE_SHIPMENTS[shipment_id]

    if shipment["status"] == "CREATED":
        alerts.append("Shipment not yet started")

    if shipment["status"] == "IN_TRANSIT":
        alerts.append("Monitor temperature levels")

    return {
        "shipment_id": shipment_id,
        "alerts": alerts
    }

# -------------------------------------------------------------------
# 24. BULK STATUS UPDATE (admin style)
# -------------------------------------------------------------------

@router.post("/batch/update-status")
async def batch_update_status(
    shipment_ids: List[str],
    
):
    updated = 0

    for sid in shipment_ids:
        if sid in FAKE_SHIPMENTS:
            updated += 1

    return {
        "updated_count": updated,
        "requested": len(shipment_ids)
    }

# -------------------------------------------------------------------
# 25. LIVE HEATMAP DATA 
# -------------------------------------------------------------------

@router.get("/global/heatmap")
async def get_heatmap():
    points = []

    for s in FAKE_SHIPMENTS.values():
        if s.get("location"):
            points.append(s["location"])

    return {
        "active_points": len(points),
        "coordinates": points
    }

# -------------------------------------------------------------------
# 26. SYSTEM HEALTH CHECK
# -------------------------------------------------------------------

@router.get("/system/health")
async def module_health():
    return {
        "module": "transport",
        "status": "healthy",
        "active_shipments": len(FAKE_SHIPMENTS),
        "timestamp": datetime.utcnow()
    }