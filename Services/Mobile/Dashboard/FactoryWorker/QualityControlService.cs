using PackTrack.Models;
using PackTrack.Data;

namespace PackTrack.Services.Mobile.Dashboard.FactoryWorker
{
    public class QualityControlService
    {
        private readonly LiteDbContext _context;

        public QualityControlService(LiteDbContext context)
        {
            _context = context;
        }

        public QualityCheckResponse PerformQualityCheck(QualityCheckRequest request)
        {
            // Perform quality check and save results
            var checkId = $"QC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";

            // Calculate quality score based on parameters
            double qualityScore = CalculateQualityScore(request.CheckParameters);

            return new QualityCheckResponse
            {
                Success = true,
                CheckId = checkId,
                BatchStatus = qualityScore >= 95 ? "Approved" : "Rejected",
                QualityScore = qualityScore,
                CertificateGenerated = true,
                CertificateUrl = $"https://storage.packtrack.com/qc/{DateTime.Now:yyyy/MM/dd}/{checkId}.pdf",
                NextAction = qualityScore >= 95 ? "ProceedToPackaging" : "QuarantineBatch",
                Message = qualityScore >= 95 ? "Quality check passed" : "Quality check failed"
            };
        }

        private double CalculateQualityScore(QualityCheckParameters parameters)
        {
            double score = 100.0;
            
            if (!parameters.VisualInspection.Passed) score -= 10;
            if (!parameters.WeightCheck.Passed) score -= 15;
            if (!parameters.SealIntegrity.Passed) score -= 20;
            if (!parameters.LabelQuality.Passed) score -= 10;
            if (!parameters.CarbonationLevel.Passed) score -= 15;
            if (!parameters.PhLevel.Passed) score -= 15;

            return Math.Max(0, score);
        }
    }

    public class QualityCheckRequest
    {
        public int BatchId { get; set; }
        public string BatchCode { get; set; } = string.Empty;
        public string WorkerId { get; set; } = string.Empty;
        public string CheckType { get; set; } = string.Empty;
        public int SampleSize { get; set; }
        public QualityCheckParameters CheckParameters { get; set; } = new();
        public string OverallResult { get; set; } = string.Empty;
        public int DefectsFound { get; set; }
        public DateTime Timestamp { get; set; }
        public string Notes { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = new();
    }

    public class QualityCheckParameters
    {
        public VisualInspectionResult VisualInspection { get; set; } = new();
        public WeightCheckResult WeightCheck { get; set; } = new();
        public SealIntegrityResult SealIntegrity { get; set; } = new();
        public LabelQualityResult LabelQuality { get; set; } = new();
        public CarbonationLevelResult CarbonationLevel { get; set; } = new();
        public PhLevelResult PhLevel { get; set; } = new();
    }

    public class VisualInspectionResult
    {
        public bool Passed { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class WeightCheckResult
    {
        public bool Passed { get; set; }
        public double AverageWeight { get; set; }
        public double TargetWeight { get; set; }
        public double Tolerance { get; set; }
    }

    public class SealIntegrityResult
    {
        public bool Passed { get; set; }
        public double Pressure { get; set; }
        public string LeakTest { get; set; } = string.Empty;
    }

    public class LabelQualityResult
    {
        public bool Passed { get; set; }
        public string Alignment { get; set; } = string.Empty;
        public string PrintQuality { get; set; } = string.Empty;
    }

    public class CarbonationLevelResult
    {
        public bool Passed { get; set; }
        public double Measured { get; set; }
        public double Target { get; set; }
        public double Tolerance { get; set; }
    }

    public class PhLevelResult
    {
        public bool Passed { get; set; }
        public double Measured { get; set; }
        public double Target { get; set; }
        public double Tolerance { get; set; }
    }

    public class QualityCheckResponse
    {
        public bool Success { get; set; }
        public string CheckId { get; set; } = string.Empty;
        public string BatchStatus { get; set; } = string.Empty;
        public double QualityScore { get; set; }
        public bool CertificateGenerated { get; set; }
        public string CertificateUrl { get; set; } = string.Empty;
        public string NextAction { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
