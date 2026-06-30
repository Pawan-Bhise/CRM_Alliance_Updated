using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data.Entity;
using CallCenter.Models;
using CallCenterSecure.Models.ViewModels;

namespace CallCenterSecure.Services
{
    public class ReportingService
    {
        public ComplaintReportViewModel GetComplaintReport(DateTime? fromDate, DateTime? toDate)
        {
            using (var db = new ApplicationDbContext())
            {
                var query = ApplyDateFilter(db.AllianceInbounds.AsNoTracking(), fromDate, toDate)
                    .AsEnumerable()
                    .Where(IsComplaintRecord);

                var rows = query.ToList();

                var report = new ComplaintReportViewModel
                {
                    Filter = new ReportFilterViewModel { FromDate = fromDate, ToDate = toDate },
                    TotalTickets = rows.Count,
                    ResolvedCount = rows.Count(x => IsClosed(x.Cmp_ComplainStatus)),
                    UnresolvedCount = rows.Count(x => !IsClosed(x.Cmp_ComplainStatus)),
                    AverageResponseTimeMinutes = CalculateAverageMinutes(rows.Select(CalculateResponseMinutes)),
                    AverageCsatScore = null,
                    Notes = "CSAT is not stored in the current schema. Branch is available from Cmp_Branch."
                };

                report.ResolutionRate = report.TotalTickets == 0
                    ? 0m
                    : Math.Round((decimal)report.ResolvedCount * 100m / report.TotalTickets, 2);

                report.CategoryBreakdown = BuildCategoryBreakdown(
                    rows.Select(x => NormalizeComplaintCategory(x.Cmp_NatureOfComplaint)).ToList());

                report.TopBranches = BuildBreakdown(
                    rows.Where(x => !string.IsNullOrWhiteSpace(x.Cmp_Branch)).Select(x => x.Cmp_Branch.Trim()).ToList(),
                    5);

                return report;
            }
        }

        public EnquiryReportViewModel GetEnquiryReport(DateTime? fromDate, DateTime? toDate)
        {
            using (var db = new ApplicationDbContext())
            {
                var query = ApplyDateFilter(db.AllianceInbounds.AsNoTracking(), fromDate, toDate)
                    .AsEnumerable()
                    .Where(IsEnquiryRecord);

                var rows = query.ToList();

                var report = new EnquiryReportViewModel
                {
                    Filter = new ReportFilterViewModel { FromDate = fromDate, ToDate = toDate },
                    TotalEnquiries = rows.Count,
                    TotalLeadCreation = rows.Count(x => !string.IsNullOrWhiteSpace(x.Lead_CustomerName)),
                    AverageResponseTimeMinutes = CalculateAverageMinutes(rows.Select(CalculateResponseMinutes)),
                    Notes = "Lead category mapping is provisional and based on Lead_ProductInterested text."
                };

                report.LeadStatusBreakdown = BuildBreakdown(
                    rows.Where(x => !string.IsNullOrWhiteSpace(x.Lead_LeadStatus)).Select(x => x.Lead_LeadStatus.Trim()).ToList(),
                    10);

                report.CategoryBreakdown = BuildCategoryBreakdown(
                    rows.Select(x => NormalizeEnquiryCategory(x.Lead_ProductInterested)).ToList());

                report.TopLocations = BuildBreakdown(
                    rows.Where(x => !string.IsNullOrWhiteSpace(x.Lead_Branch)).Select(x => x.Lead_Branch.Trim()).ToList(),
                    5);

                return report;
            }
        }

        private static IQueryable<AllianceInbound> ApplyDateFilter(IQueryable<AllianceInbound> query, DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue)
            {
                var start = fromDate.Value.Date;
                query = query.Where(x => x.DateTime >= start);
            }

            if (toDate.HasValue)
            {
                var end = toDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.DateTime <= end);
            }

            return query;
        }

        private static bool IsComplaintRecord(AllianceInbound inbound)
        {
            return !string.IsNullOrWhiteSpace(inbound.Cmp_CustomerName)
                   || !string.IsNullOrWhiteSpace(inbound.Cmp_CaseDetail)
                   || !string.IsNullOrWhiteSpace(inbound.Cmp_ComplainStatus)
                   || !string.IsNullOrWhiteSpace(inbound.Cmp_Branch)
                   || !string.IsNullOrWhiteSpace(inbound.Cmp_NatureOfComplaint);
        }

        private static bool IsEnquiryRecord(AllianceInbound inbound)
        {
            return !string.IsNullOrWhiteSpace(inbound.Lead_CustomerName)
                   || !string.IsNullOrWhiteSpace(inbound.Lead_LeadStatus)
                   || !string.IsNullOrWhiteSpace(inbound.Lead_Branch)
                   || !string.IsNullOrWhiteSpace(inbound.Lead_ProductInterested);
        }

        private static bool IsClosed(string status)
        {
            return !string.IsNullOrWhiteSpace(status) && status.Trim().Equals("Closed", StringComparison.OrdinalIgnoreCase);
        }

        private static double? CalculateResponseMinutes(AllianceInbound inbound)
        {
            if (inbound.Duration.HasValue && inbound.Duration.Value > 0)
            {
                return inbound.Duration.Value;
            }

            if (inbound.CallStartDateTime.HasValue && inbound.CallEndDateTime.HasValue && inbound.CallEndDateTime.Value >= inbound.CallStartDateTime.Value)
            {
                return (inbound.CallEndDateTime.Value - inbound.CallStartDateTime.Value).TotalMinutes;
            }

            return null;
        }

        private static double? CalculateAverageMinutes(IEnumerable<double?> values)
        {
            var list = values.Where(x => x.HasValue).Select(x => x.Value).ToList();
            if (!list.Any())
            {
                return null;
            }

            return Math.Round(list.Average(), 2);
        }

        private static List<ReportBreakdownRowViewModel> BuildCategoryBreakdown(List<string> labels)
        {
            var fixedOrder = new[] { "Product Issues", "Service Delay", "Staff Behavior", "Others" };
            var normalized = labels.GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var total = labels.Count;
            return fixedOrder.Select(label =>
            {
                var count = normalized.ContainsKey(label) ? normalized[label] : 0;
                return new ReportBreakdownRowViewModel
                {
                    Label = label,
                    Count = count,
                    Percentage = total == 0 ? 0 : Math.Round((decimal)count * 100m / total, 2)
                };
            }).ToList();
        }

        private static List<ReportBreakdownRowViewModel> BuildBreakdown(List<string> labels, int take)
        {
            var grouped = labels
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Label)
                .Take(take)
                .ToList();

            var total = labels.Count;

            return grouped.Select(x => new ReportBreakdownRowViewModel
            {
                Label = x.Label,
                Count = x.Count,
                Percentage = total == 0 ? 0 : Math.Round((decimal)x.Count * 100m / total, 2)
            }).ToList();
        }

        private static string NormalizeComplaintCategory(string raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Others";
            }

            int categoryId;
            if (int.TryParse(value, out categoryId))
            {
                var provider = new CallCenter.BusinessLogic.CategoryProvider();
                var category = provider.GetCategoryById(categoryId);
                value = category != null ? category.CategoryName : value;
            }

            var normalized = value.ToLowerInvariant();
            if (normalized.Contains("product"))
            {
                return "Product Issues";
            }

            if (normalized.Contains("service"))
            {
                return "Service Delay";
            }

            if (normalized.Contains("staff"))
            {
                return "Staff Behavior";
            }

            return "Others";
        }

        private static string NormalizeEnquiryCategory(string raw)
        {
            var value = (raw ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Others";
            }

            var normalized = value.ToLowerInvariant();
            if (normalized.Contains("sa"))
            {
                return "SA";
            }

            if (normalized.Contains("mm"))
            {
                return "MM";
            }

            if (normalized.Contains("agri"))
            {
                return "Agri";
            }

            if (normalized.Contains("saving"))
            {
                return "Savings";
            }

            return "Others";
        }
    }
}