# app/api/mobile/dashboard/wholesaler/pricing.py

from fastapi import APIRouter, Depends, Query, Path, Body, UploadFile, File
from fastapi.responses import HTMLResponse
from sqlalchemy.orm import Session
from typing import List, Optional
from datetime import date

from app.config.database import get_db
from app.core.dependencies import get_current_user
from app.core.roles import require_role
from app.core.pagination import paginate
from app.schemas.common import SuccessResponse
from app.schemas.wholesaler_pricing import (
    PriceCreate,
    PriceUpdate,
    PriceResponse,
    TierPricingRequest,
    VolumeDiscountRequest,
    RegionalPricingRequest,
    PricingApprovalRequest,
    MarginAnalysisResponse,
    CompetitorComparisonResponse,
    ProfitForecastResponse,
    PriceSimulationRequest
)
from app.services.dashboard.wholesaler_service import WholesalerService

router = APIRouter()


# =========================================================
# 💰 CREATE PRICE ENTRY
# =========================================================
@router.post("/pricing", response_model=PriceResponse)
async def create_price(
    payload: PriceCreate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.create_price(payload, wholesaler_id=current_user.id)


# =========================================================
# ✏️ UPDATE PRICE
# =========================================================
@router.put("/pricing/{price_id}", response_model=PriceResponse)
async def update_price(
    price_id: str,
    payload: PriceUpdate,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.update_price(price_id, payload)


# =========================================================
# 📊 LIST PRICING
# =========================================================
@router.get("/pricing", response_model=List[PriceResponse])
async def list_pricing(
    page: int = Query(1, ge=1),
    limit: int = Query(20, le=100),
    product_id: Optional[str] = None,
    region: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)

    prices = service.get_pricing(
        wholesaler_id=current_user.id,
        product_id=product_id,
        region=region
    )

    return paginate(prices, page, limit)


# =========================================================
# 📈 TIERED PRICING
# =========================================================
@router.post("/pricing/tiered", response_model=SuccessResponse)
async def set_tier_pricing(
    payload: TierPricingRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.set_tier_pricing(payload)
    return {"message": "Tier pricing configured successfully"}


# =========================================================
# 📦 VOLUME DISCOUNT
# =========================================================
@router.post("/pricing/volume-discount", response_model=SuccessResponse)
async def set_volume_discount(
    payload: VolumeDiscountRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.set_volume_discount(payload)
    return {"message": "Volume discount applied successfully"}


# =========================================================
# 🌍 REGIONAL PRICING
# =========================================================
@router.post("/pricing/regional", response_model=SuccessResponse)
async def set_regional_pricing(
    payload: RegionalPricingRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    service.set_regional_pricing(payload)
    return {"message": "Regional pricing updated successfully"}


# =========================================================
# 🔄 TOGGLE DYNAMIC PRICING
# =========================================================
@router.post("/pricing/dynamic-toggle", response_model=SuccessResponse)
async def toggle_dynamic_pricing(
    enable: bool = Body(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler", "admin"])
    service = WholesalerService(db)
    service.toggle_dynamic_pricing(current_user.id, enable)
    return {"message": "Dynamic pricing updated successfully"}


# =========================================================
# ✅ APPROVE PRICING CHANGE
# =========================================================
@router.post("/pricing/approve", response_model=SuccessResponse)
async def approve_pricing(
    payload: PricingApprovalRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["admin"])
    service = WholesalerService(db)
    service.approve_pricing(payload.price_id)
    return {"message": "Pricing approved successfully"}


# =========================================================
# 📊 MARGIN ANALYSIS
# =========================================================
@router.get("/pricing/margin-analysis", response_model=MarginAnalysisResponse)
async def margin_analysis(
    product_id: Optional[str] = None,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.calculate_margin_analysis(current_user.id, product_id)


# =========================================================
# 🆚 COMPETITOR PRICE COMPARISON
# =========================================================
@router.get("/pricing/competitor-comparison", response_model=CompetitorComparisonResponse)
async def competitor_comparison(
    product_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.compare_competitor_prices(product_id)


# =========================================================
# 🔮 PROFIT FORECAST
# =========================================================
@router.get("/pricing/profit-forecast", response_model=ProfitForecastResponse)
async def profit_forecast(
    product_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.forecast_profit(product_id)


# =========================================================
# 🧮 PRICE SIMULATION
# =========================================================
@router.post("/pricing/simulate", response_model=ProfitForecastResponse)
async def simulate_price(
    payload: PriceSimulationRequest,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.simulate_price_change(payload)


# =========================================================
# 📤 EXPORT PRICING DATA
# =========================================================
@router.get("/pricing/export")
async def export_pricing(
    format: str = Query("csv"),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.export_pricing(current_user.id, format)


# =========================================================
# 📥 BULK PRICING UPLOAD
# =========================================================
@router.post("/pricing/bulk-upload")
async def bulk_upload_pricing(
    file: UploadFile = File(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["wholesaler"])
    service = WholesalerService(db)
    return service.bulk_upload_pricing(file, current_user.id)


# =========================================================
# 🌐 PRICING DASHBOARD (HTML)
# =========================================================
@router.get("/pricing/dashboard", response_class=HTMLResponse)
async def pricing_dashboard():
    return """
<!DOCTYPE html>
<html>
<head>
    <title>Wholesaler Pricing Dashboard</title>
    <style>
        body {
            background: #0f172a;
            color: white;
            font-family: Arial, sans-serif;
            padding: 40px;
        }
        h1 { color: #10b981; }
        .card {
            background: #1e293b;
            padding: 20px;
            margin-bottom: 20px;
            border-radius: 10px;
        }
        button {
            background: #10b981;
            border: none;
            padding: 10px 15px;
            border-radius: 6px;
            color: white;
            cursor: pointer;
        }
        button:hover {
            background: #059669;
        }
    </style>
</head>
<body>

<h1>💰 Wholesaler Pricing Dashboard</h1>

<div class="card">
    <h2>Margin Analysis</h2>
    <button onclick="alert('GET /pricing/margin-analysis')">View Margins</button>
</div>

<div class="card">
    <h2>Profit Forecast</h2>
    <button onclick="alert('GET /pricing/profit-forecast')">View Forecast</button>
</div>

<div class="card">
    <h2>Competitor Comparison</h2>
    <button onclick="alert('GET /pricing/competitor-comparison')">Compare</button>
</div>

<script>
    console.log("Wholesaler pricing dashboard loaded.");
</script>

</body>
</html>
"""