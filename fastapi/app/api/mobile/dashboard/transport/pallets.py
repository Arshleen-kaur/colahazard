# app/api/mobile/dashboard/transport/pallets.py

from fastapi import APIRouter, Depends, Query, Path
from sqlalchemy.orm import Session
from typing import List

from app.config.database import get_db
from app.core.dependencies import get_current_user
from app.core.roles import require_role
from app.core.pagination import paginate
from app.schemas.pallet import (
    PalletResponse,
    PalletCreate,
    PalletUpdateStatus
)
from app.services.dashboard.transport_service import TransportService

router = APIRouter()


# ---------------------------------------------------------
# 📦 GET ALL PALLETS (Assigned to Driver)
# ---------------------------------------------------------
@router.get("/pallets", response_model=List[PalletResponse])
async def get_driver_pallets(
    page: int = Query(1, ge=1),
    limit: int = Query(10, le=100),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    pallets = service.get_driver_pallets(current_user.id)

    return paginate(pallets, page, limit)


# ---------------------------------------------------------
# 📦 GET PALLET DETAILS
# ---------------------------------------------------------
@router.get("/pallet/{pallet_id}", response_model=PalletResponse)
async def get_pallet_details(
    pallet_id: str = Path(...),
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    return service.get_pallet_by_id(pallet_id)


# ---------------------------------------------------------
# 📷 SCAN PALLET QR
# ---------------------------------------------------------
@router.post("/pallet/scan")
async def scan_pallet_qr(
    qr_code: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    return service.scan_pallet(qr_code, current_user.id)


# ---------------------------------------------------------
# 🚛 LOAD PALLET TO TRUCK
# ---------------------------------------------------------
@router.post("/pallet/load")
async def load_pallet(
    pallet_id: str,
    shipment_id: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    return service.load_pallet(pallet_id, shipment_id)


# ---------------------------------------------------------
# 📦 UNLOAD PALLET
# ---------------------------------------------------------
@router.post("/pallet/unload")
async def unload_pallet(
    pallet_id: str,
    location: str,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    return service.unload_pallet(pallet_id, location)


# ---------------------------------------------------------
# 🔄 UPDATE PALLET STATUS
# ---------------------------------------------------------
@router.patch("/pallet/status")
async def update_pallet_status(
    payload: PalletUpdateStatus,
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    return service.update_status(
        pallet_id=payload.pallet_id,
        status=payload.status
    )


# ---------------------------------------------------------
# 📊 PALLET ANALYTICS (Transport View)
# ---------------------------------------------------------
@router.get("/pallet/analytics")
async def pallet_analytics(
    current_user=Depends(get_current_user),
    db: Session = Depends(get_db),
):
    require_role(current_user, ["transport"])

    service = TransportService(db)
    return service.pallet_analytics(current_user.id)