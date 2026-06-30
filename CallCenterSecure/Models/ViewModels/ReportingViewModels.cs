using System;
using System.Collections.Generic;

namespace CallCenterSecure.Models.ViewModels
{
    public class ReportFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class ReportBreakdownRowViewModel
    {
        public string Label { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ComplaintReportViewModel
    {
        public ComplaintReportViewModel()
        {
            Filter = new ReportFilterViewModel();
            CategoryBreakdown = new List<ReportBreakdownRowViewModel>();
            TopBranches = new List<ReportBreakdownRowViewModel>();
        }

        public ReportFilterViewModel Filter { get; set; }
        public int TotalTickets { get; set; }
        public double? AverageResponseTimeMinutes { get; set; }
        public decimal ResolutionRate { get; set; }
        public double? AverageCsatScore { get; set; }
        public int ResolvedCount { get; set; }
        public int UnresolvedCount { get; set; }
        public List<ReportBreakdownRowViewModel> CategoryBreakdown { get; set; }
        public List<ReportBreakdownRowViewModel> TopBranches { get; set; }
        public string Notes { get; set; }
    }

    public class EnquiryReportViewModel
    {
        public EnquiryReportViewModel()
        {
            Filter = new ReportFilterViewModel();
            CategoryBreakdown = new List<ReportBreakdownRowViewModel>();
            LeadStatusBreakdown = new List<ReportBreakdownRowViewModel>();
            TopLocations = new List<ReportBreakdownRowViewModel>();
        }

        public ReportFilterViewModel Filter { get; set; }
        public int TotalEnquiries { get; set; }
        public int TotalLeadCreation { get; set; }
        public double? AverageResponseTimeMinutes { get; set; }
        public List<ReportBreakdownRowViewModel> LeadStatusBreakdown { get; set; }
        public List<ReportBreakdownRowViewModel> CategoryBreakdown { get; set; }
        public List<ReportBreakdownRowViewModel> TopLocations { get; set; }
        public string Notes { get; set; }
    }
}