using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using CallCenterSecure.Models;
using CallCenterSecure.Models.ViewModels;

namespace CallCenterSecure.Services
{
    public class SurveyReportService
    {
        public SurveyReportViewModel GetReport(int? templateId, int? formId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            using (var db = new ApplicationDbContext())
            {
                var forms = db.SurveyForms.AsNoTracking()
                    .Where(x => !templateId.HasValue || x.SurveyTemplateId == templateId.Value)
                    .OrderBy(x => x.Title)
                    .ToList();
                var selectedForm = formId.HasValue
                    ? forms.FirstOrDefault(x => x.Id == formId.Value)
                    : forms.FirstOrDefault(x => x.IsActive);

                var report = new SurveyReportViewModel
                {
                    SelectedTemplateId = templateId,
                    SelectedFormId = selectedForm != null ? (int?)selectedForm.Id : formId,
                    SelectedCategoryId = categoryId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SelectedFormTitle = selectedForm != null ? selectedForm.Title : string.Empty,
                    Templates = db.SurveyTemplateTypes.AsNoTracking().OrderBy(x => x.Id)
                        .Select(x => new SurveyTemplateLookupViewModel { Id = x.Id, Name = x.Name }).ToList(),
                    Forms = forms.Select(x => new SurveyFormLookupViewModel
                    {
                        Id = x.Id,
                        SurveyTemplateId = x.SurveyTemplateId,
                        Title = x.Title
                    }).ToList()
                };

                if (selectedForm == null)
                {
                    return report;
                }

                var responses = GetFilteredResponses(db, selectedForm.Id, categoryId, fromDate, toDate);
                report.TotalResponses = responses.Count;
                report.Responses = responses.OrderByDescending(x => x.SubmittedDate).Select(x => new SurveyResponseSummaryViewModel
                {
                    Id = x.Id,
                    RespondentName = x.RespondentName,
                    RespondentMobile = x.RespondentMobile,
                    SubmittedBy = x.SubmittedBy,
                    SubmittedDate = x.SubmittedDate
                }).ToList();

                var form = db.SurveyForms.AsNoTracking()
                    .Include(x => x.Questions.Select(q => q.Options))
                    .Include(x => x.Questions.Select(q => q.GridRows))
                    .Include(x => x.Questions.Select(q => q.GridColumns))
                    .FirstOrDefault(x => x.Id == selectedForm.Id);

                report.Questions = form == null
                    ? new List<SurveyQuestionReportViewModel>()
                    : form.Questions.OrderBy(x => x.DisplayOrder)
                        .Select(question => BuildQuestionReport(question, responses)).ToList();

                return report;
            }
        }

        public SurveyResponseDetailViewModel GetResponseDetail(int responseId)
        {
            using (var db = new ApplicationDbContext())
            {
                var response = db.SurveyFormResponses.AsNoTracking()
                    .Include(x => x.SurveyForm.SurveyTemplate)
                    .Include(x => x.Answers.Select(a => a.GridAnswers))
                    .Include(x => x.Answers.Select(a => a.SurveyQuestion))
                    .FirstOrDefault(x => x.Id == responseId);
                if (response == null)
                {
                    return null;
                }

                return new SurveyResponseDetailViewModel
                {
                    Id = response.Id,
                    FormTitle = response.SurveyForm.Title,
                    TemplateName = response.SurveyForm.SurveyTemplate.Name,
                    RespondentName = response.RespondentName,
                    RespondentMobile = response.RespondentMobile,
                    SubmittedBy = response.SubmittedBy,
                    SubmittedDate = response.SubmittedDate,
                    Answers = response.Answers.OrderBy(x => x.SurveyQuestion.DisplayOrder).Select(x => new SurveyResponseAnswerViewModel
                    {
                        QuestionText = x.SurveyQuestion.QuestionText,
                        QuestionType = x.SurveyQuestion.QuestionType,
                        Answer = FormatAnswer(x)
                    }).ToList()
                };
            }
        }

        public byte[] ExportCsv(int? formId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            using (var db = new ApplicationDbContext())
            {
                var form = db.SurveyForms.AsNoTracking()
                    .Include(x => x.Questions)
                    .FirstOrDefault(x => x.Id == formId.Value);
                if (form == null)
                {
                    throw new InvalidOperationException("Survey form not found.");
                }

                var responses = GetFilteredResponses(db, form.Id, categoryId, fromDate, toDate);
                var questions = form.Questions.OrderBy(x => x.DisplayOrder).ToList();
                var builder = new StringBuilder();
                builder.AppendLine(string.Join(",", new[] { "Response Id", "Respondent Name", "Respondent Mobile", "Submitted By", "Submitted Date" }
                    .Concat(questions.Select(x => x.QuestionText)).Select(CsvEscape)));

                foreach (var response in responses.OrderByDescending(x => x.SubmittedDate))
                {
                    var values = new List<string>
                    {
                        response.Id.ToString(CultureInfo.InvariantCulture),
                        response.RespondentName,
                        response.RespondentMobile,
                        response.SubmittedBy,
                        response.SubmittedDate.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                    };
                    values.AddRange(questions.Select(question =>
                    {
                        var answer = response.Answers.FirstOrDefault(x => x.SurveyQuestionId == question.Id);
                        return answer == null ? string.Empty : FormatAnswer(answer);
                    }));
                    builder.AppendLine(string.Join(",", values.Select(CsvEscape)));
                }

                return Encoding.UTF8.GetBytes(builder.ToString());
            }
        }

        private static List<SurveyFormResponse> GetFilteredResponses(ApplicationDbContext db, int formId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            var query = db.SurveyFormResponses.AsNoTracking()
                .Include(x => x.Answers.Select(a => a.GridAnswers))
                .Where(x => x.SurveyFormId == formId);

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.SurveyCategoryId == categoryId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.SubmittedDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(x => x.SubmittedDate < toDate.Value.Date.AddDays(1));
            }

            return query.ToList();
        }

        private static SurveyQuestionReportViewModel BuildQuestionReport(SurveyQuestion question, List<SurveyFormResponse> responses)
        {
            var answers = responses.SelectMany(x => x.Answers.Where(a => a.SurveyQuestionId == question.Id)).ToList();
            var values = answers.Select(GetPrimaryValue).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var report = new SurveyQuestionReportViewModel
            {
                QuestionId = question.Id,
                DisplayOrder = question.DisplayOrder,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                AnsweredCount = values.Count,
                UnansweredCount = responses.Count - values.Count
            };

            if (question.QuestionType == SurveyQuestionTypeCatalog.Checkboxes)
            {
                report.Options = BuildOptions(question.Options.Select(x => x.OptionText), answers.SelectMany(GetCheckboxValues), values.Count);
            }
            else if (question.QuestionType == SurveyQuestionTypeCatalog.MultipleChoice || question.QuestionType == SurveyQuestionTypeCatalog.Dropdown)
            {
                report.Options = BuildOptions(question.Options.Select(x => x.OptionText), values, values.Count);
            }
            else if (question.QuestionType == SurveyQuestionTypeCatalog.Nps)
            {
                AddNumericSummary(report, values);
                report.Promoters = values.Count(IsPromoter);
                report.Passives = values.Count(IsPassive);
                report.Detractors = values.Count(IsDetractor);
                report.NpsScore = values.Count == 0 ? (double?)null : Math.Round((report.Promoters - report.Detractors) * 100d / values.Count, 2);
            }
            else if (question.QuestionType == SurveyQuestionTypeCatalog.LinearScale || question.QuestionType == SurveyQuestionTypeCatalog.Ranking)
            {
                AddNumericSummary(report, values);
            }
            else if (question.QuestionType == SurveyQuestionTypeCatalog.MultipleChoiceGrid || question.QuestionType == SurveyQuestionTypeCatalog.CheckboxGrid)
            {
                report.GridRows = question.GridRows.OrderBy(x => x.DisplayOrder).Select(row => new SurveyReportGridRowViewModel
                {
                    RowText = row.RowText,
                    Options = BuildGridOptions(question, answers, row.RowText)
                }).ToList();
            }
            else
            {
                report.TextAnswers = values.Take(100).ToList();
            }

            return report;
        }

        private static List<SurveyReportOptionViewModel> BuildOptions(IEnumerable<string> declaredOptions, IEnumerable<string> values, int denominator)
        {
            var allOptions = declaredOptions.Concat(values).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return allOptions.Select(option =>
            {
                var count = values.Count(x => string.Equals(x == null ? null : x.Trim(), option, StringComparison.OrdinalIgnoreCase));
                return new SurveyReportOptionViewModel
                {
                    Label = option,
                    Count = count,
                    Percentage = denominator == 0 ? 0 : Math.Round((decimal)count * 100m / denominator, 2)
                };
            }).OrderByDescending(x => x.Count).ThenBy(x => x.Label).ToList();
        }

        private static List<SurveyReportOptionViewModel> BuildGridOptions(SurveyQuestion question, List<SurveyFormAnswer> answers, string rowText)
        {
            var values = answers.SelectMany(answer => answer.GridAnswers.Where(row => string.Equals(row.RowText, rowText, StringComparison.OrdinalIgnoreCase))
                .SelectMany(GetGridValues)).ToList();
            var columns = question.GridColumns.Select(x => x.ColumnText);
            return BuildOptions(columns, values, values.Count);
        }

        private static void AddNumericSummary(SurveyQuestionReportViewModel report, List<string> values)
        {
            var numbers = values.Select(x => ParseNumber(x)).Where(x => x.HasValue).Select(x => x.Value).ToList();
            if (!numbers.Any())
            {
                return;
            }

            report.Average = Math.Round(numbers.Average(), 2);
            report.Minimum = numbers.Min();
            report.Maximum = numbers.Max();
        }

        private static string GetPrimaryValue(SurveyFormAnswer answer)
        {
            if (!string.IsNullOrWhiteSpace(answer.SelectedOption))
            {
                return answer.SelectedOption.Trim();
            }

            return answer.AnswerText == null ? string.Empty : answer.AnswerText.Trim();
        }

        private static IEnumerable<string> GetCheckboxValues(SurveyFormAnswer answer)
        {
            return (answer.SelectedOptionsCsv ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        }

        private static IEnumerable<string> GetGridValues(SurveyFormGridAnswer answer)
        {
            var values = new List<string>();
            if (!string.IsNullOrWhiteSpace(answer.SelectedColumnText))
            {
                values.Add(answer.SelectedColumnText.Trim());
            }

            values.AddRange((answer.SelectedColumnTextsCsv ?? string.Empty).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
            return values;
        }

        private static string FormatAnswer(SurveyFormAnswer answer)
        {
            var values = new List<string>();
            if (!string.IsNullOrWhiteSpace(answer.AnswerText))
            {
                values.Add(answer.AnswerText.Trim());
            }
            if (!string.IsNullOrWhiteSpace(answer.SelectedOption))
            {
                values.Add(answer.SelectedOption.Trim());
            }
            if (!string.IsNullOrWhiteSpace(answer.SelectedOptionsCsv))
            {
                values.Add(answer.SelectedOptionsCsv.Trim());
            }
            if (!string.IsNullOrWhiteSpace(answer.FileName))
            {
                values.Add(answer.FileName.Trim());
            }
            if (answer.GridAnswers != null)
            {
                values.AddRange(answer.GridAnswers.Select(x => x.RowText + ": " + string.Join(", ", GetGridValues(x))));
            }
            return string.Join(" | ", values);
        }

        private static double? ParseNumber(string value)
        {
            double number;
            return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out number) ? (double?)number : null;
        }

        private static bool IsPromoter(string value)
        {
            var number = ParseNumber(value);
            return number.HasValue && number.Value >= 9;
        }

        private static bool IsPassive(string value)
        {
            var number = ParseNumber(value);
            return number.HasValue && number.Value >= 7 && number.Value <= 8;
        }

        private static bool IsDetractor(string value)
        {
            var number = ParseNumber(value);
            return number.HasValue && number.Value <= 6;
        }

        private static string CsvEscape(string value)
        {
            var safeValue = value ?? string.Empty;
            return "\"" + safeValue.Replace("\"", "\"\"") + "\"";
        }
    }
}
