using Microsoft.AspNetCore.Mvc;
using PackTrack.Services.Traceability;
using PackTrack.Models;

namespace PackTrack.Controllers
{
    /// <summary>
    /// EcoCola Traceability Mesh API Controller
    /// Complete product journey tracking and QR scanning
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TraceabilityController : ControllerBase
    {
        private readonly TraceabilityMeshService _traceabilityService;

        public TraceabilityController(TraceabilityMeshService traceabilityService)
        {
            _traceabilityService = traceabilityService;
        }

        #region QR Scanning

        /// <summary>
        /// Scan QR code and get complete traceability information
        /// </summary>
        /// <param name="request">Scan request with QR code and context</param>
        /// <returns>Complete traceability chain with movement history</returns>
        [HttpPost("scan")]
        public IActionResult ScanQRCode([FromBody] ScanRequest request)
        {
            try
            {
                var scanContext = new ScanContext
                {
                    UserId = request.UserId ?? "anonymous",
                    Location = request.Location ?? "unknown",
                    ScanType = request.ScanType ?? "manual",
                    DeviceId = request.DeviceId ?? "web",
                    Latitude = request.Latitude,
                    Longitude = request.Longitude
                };

                var result = _traceabilityService.ScanQRCode(request.QRCode, scanContext);

                if (!result.Success)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Traceability Chain

        /// <summary>
        /// Get complete traceability chain for a QR code
        /// </summary>
        /// <param name="qrCode">QR code to trace</param>
        /// <returns>Hierarchical traceability chain</returns>
        [HttpGet("chain/{qrCode}")]
        public IActionResult GetTraceabilityChain(string qrCode)
        {
            try
            {
                var scanContext = new ScanContext
                {
                    UserId = "system",
                    Location = "api",
                    ScanType = "query"
                };

                var result = _traceabilityService.ScanQRCode(qrCode, scanContext);

                if (!result.Success)
                {
                    return NotFound(new { error = result.ErrorMessage });
                }

                return Ok(result.TraceabilityChain);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Movement History

        /// <summary>
        /// Get movement history for a specific entity
        /// </summary>
        /// <param name="qrCode">QR code of the entity</param>
        /// <returns>List of movement records</returns>
        [HttpGet("movement-history/{qrCode}")]
        public IActionResult GetMovementHistory(string qrCode)
        {
            try
            {
                var scanContext = new ScanContext
                {
                    UserId = "system",
                    Location = "api",
                    ScanType = "query"
                };

                var result = _traceabilityService.ScanQRCode(qrCode, scanContext);

                if (!result.Success)
                {
                    return NotFound(new { error = result.ErrorMessage });
                }

                return Ok(result.MovementHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Analytics

        /// <summary>
        /// Get traceability analytics for a batch
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <returns>Analytics data including traceability percentage, status distribution, etc.</returns>
        [HttpGet("analytics/batch/{batchId}")]
        public IActionResult GetBatchAnalytics(int batchId)
        {
            try
            {
                var analytics = _traceabilityService.GetBatchAnalytics(batchId);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Get all bottles in a batch with their current status
        /// </summary>
        /// <param name="batchId">Batch ID</param>
        /// <returns>List of bottles with status</returns>
        [HttpGet("batch/{batchId}/bottles")]
        public IActionResult GetBatchBottles(int batchId)
        {
            try
            {
                var analytics = _traceabilityService.GetBatchAnalytics(batchId);
                return Ok(new
                {
                    batchId = batchId,
                    batchCode = analytics.BatchCode,
                    totalUnits = analytics.TotalUnits,
                    trackedUnits = analytics.TrackedUnits,
                    traceabilityPercentage = analytics.TraceabilityPercentage,
                    statusDistribution = analytics.StatusDistribution,
                    locationDistribution = analytics.LocationDistribution
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        #endregion
    }

    #region Request Models

    public class ScanRequest
    {
        public string QRCode { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public string? Location { get; set; }
        public string? ScanType { get; set; }
        public string? DeviceId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    #endregion
}
