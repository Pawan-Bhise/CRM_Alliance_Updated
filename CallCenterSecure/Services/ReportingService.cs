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
                    .Where(x => HasCallObjective(x, 2) && HasTicketType(x, 3));

                var rows = query.ToList();
                var branchNames = db.RegionBranches.AsNoTracking()
                    .ToDictionary(x => x.Id, x => x.BranchName);
                var complaintCategories = db.NatureOfComplaint.AsNoTracking()
                    .ToDictionary(x => x.ComplaintId, x => x.ComplaintsDescrption);

                var report = new ComplaintReportViewModel
                {
                    Filter = new ReportFilterViewModel { FromDate = fromDate, ToDate = toDate },
                    TotalTickets = rows.Count,
                    ResolvedCount = rows.Count(x => IsClosed(x.Cmp_ComplainStatus)),
                    UnresolvedCount = rows.Count(x => !IsClosed(x.Cmp_ComplainStatus)),
                    AverageResponseTimeMinutes = CalculateAverageMinutes(rows.Select(CalculateResponseMinutes)),
                    AverageCsatScore = null,
                    Notes = "CSAT is not stored in the current schema. Response time uses call duration when available."
                };

                report.ResolutionRate = report.TotalTickets == 0
                    ? 0m
                    : Math.Round((decimal)report.ResolvedCount * 100m / report.TotalTickets, 2);

                report.CategoryBreakdown = BuildCategoryBreakdown(
                    rows.Select(x => NormalizeComplaintCategory(x.Cmp_NatureOfComplaint, complaintCategories)).ToList(),
                    new[] { "Product Issues", "Service Delay", "Staff Behavior", "Others" });

                report.TopBranches = BuildBreakdown(
                    rows.Where(x => !string.IsNullOrWhiteSpace(x.Cmp_Branch))
                        .Select(x => ResolveLookupLabel(x.Cmp_Branch, branchNames)).ToList(),
                    5);

                return report;
            }
        }

        public EnquiryReportViewModel GetEnquiryReport(DateTime? fromDate, DateTime? toDate)
        {
            using (var db = new ApplicationDbContext())
            {
                var allRows = ApplyDateFilter(db.AllianceInbounds.AsNoTracking(), fromDate, toDate).ToList();
                var rows = allRows.Where(x => HasCallObjective(x, 1)).ToList();
                var branchNames = db.RegionBranches.AsNoTracking()
                    .ToDictionary(x => x.Id, x => x.BranchName);
                var productNames = db.Products.AsNoTracking()
                    .ToDictionary(x => x.Id, x => x.Name);

                var report = new EnquiryReportViewModel
                {
                    Filter = new ReportFilterViewModel { FromDate = fromDate, ToDate = toDate },
                    TotalEnquiries = rows.Count,
                    TotalLeadCreation = rows.Count(x => !string.IsNullOrWhiteSpace(x.Lead_CustomerName)),
                    AverageResponseTimeMinutes = CalculateAverageMinutes(rows.Select(CalculateResponseMinutes)),
                    Notes = "Lead creation uses a populated lead customer name. Response time uses call duration when available."
                };

                report.LeadStatusBreakdown = BuildBreakdown(
                    rows.Where(x => !string.IsNullOrWhiteSpace(x.Lead_LeadStatus)).Select(x => x.Lead_LeadStatus.Trim()).ToList(),
                    10);

                var productUsageCounts = rows
                    .Select(x => int.TryParse((x.Product ?? string.Empty).Trim(), out var productId)
                        ? (int?)productId
                        : null)
                    .Where(x => x.HasValue && productNames.ContainsKey(x.Value))
                    .GroupBy(x => x.Value)
                    .ToDictionary(x => x.Key, x => x.Count());
                var productUsageTotal = productUsageCounts.Values.Sum();

                report.CategoryBreakdown = productNames
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                    .OrderBy(x => x.Value)
                    .Select(x =>
                    {
                        var count = productUsageCounts.ContainsKey(x.Key) ? productUsageCounts[x.Key] : 0;
                        return new ReportBreakdownRowViewModel
                        {
                            Label = x.Value.Trim(),
                            Count = count,
                            Percentage = productUsageTotal == 0
                                ? 0
                                : Math.Round((decimal)count * 100m / productUsageTotal, 2)
                        };
                    })
                    .ToList();

                report.TopLocations = BuildBreakdown(
                    rows.Where(x => !string.IsNullOrWhiteSpace(x.Lead_Branch))
                        .Select(x => ResolveLookupLabel(x.Lead_Branch, branchNames)).ToList(),
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

        private static bool HasCallObjective(AllianceInbound inbound, int objectiveId)
        {
            return int.TryParse(inbound.CallObjective, out var storedObjectiveId)
                   && storedObjectiveId == objectiveId;
        }

        private static bool HasTicketType(AllianceInbound inbound, int ticketTypeId)
        {
            return int.TryParse(inbound.TicketType, out var storedTicketTypeId)
                   && storedTicketTypeId == ticketTypeId;
        }

        private static string ResolveLookupLabel(string rawValue, Dictionary<int, string> lookup)
        {
            var value = (rawValue ?? string.Empty).Trim();
            if (int.TryParse(value, out var id) && lookup.TryGetValue(id, out var label)
                && !string.IsNullOrWhiteSpace(label))
            {
                return label.Trim();
            }

            return value;
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

        private static List<ReportBreakdownRowViewModel> BuildCategoryBreakdown(List<string> labels, string[] fixedOrder)
        {
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

        private static string NormalizeComplaintCategory(string raw, Dictionary<int, string> complaintCategories)
        {
            var value = ResolveLookupLabel(raw, complaintCategories);
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