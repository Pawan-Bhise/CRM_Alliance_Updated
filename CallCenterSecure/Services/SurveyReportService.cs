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
        public SurveyReportViewModel GetReport(int? templateId, int? formId, int? categoryId, DateTime? fromDate, DateTime? toDate, string reportTab, int page, bool invalidDateFilter = false)
        {
            using (var db = new ApplicationDbContext())
            {
                var forms = db.SurveyForms.AsNoTracking()
                    .Where(x => !templateId.HasValue || x.SurveyTemplateId == templateId.Value)
                    .OrderBy(x => x.Title)
                    .ToList();
                var selectedForm = formId.HasValue
                    ? forms.FirstOrDefault(x => x.Id == formId.Value)
                    : null;

                var report = new SurveyReportViewModel
                {
                    SelectedTemplateId = templateId,
                    SelectedFormId = selectedForm != null ? (int?)selectedForm.Id : null,
                    SelectedCategoryId = categoryId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    DateFilterError = invalidDateFilter
                        ? "Enter valid From and To dates."
                        : fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date
                            ? "The From date must be on or before the To date."
                            : null,
                    SelectedFormTitle = selectedForm != null ? selectedForm.Title : "All forms",
                    ReportTab = string.Equals(reportTab, "responses", StringComparison.OrdinalIgnoreCase) ? "responses" : "summary",
                    PageSize = 50,
                    Templates = db.SurveyTemplateTypes.AsNoTracking().OrderBy(x => x.Id)
                        .Select(x => new SurveyTemplateLookupViewModel { Id = x.Id, Name = x.Name }).ToList(),
                    Forms = forms.Select(x => new SurveyFormLookupViewModel
                    {
                        Id = x.Id,
                        SurveyTemplateId = x.SurveyTemplateId,
                        Title = x.Title
                    }).ToList()
                };

                var responseQuery = GetFilteredResponseQuery(db, selectedForm != null ? (int?)selectedForm.Id : null, templateId, categoryId, fromDate, toDate, invalidDateFilter);
                report.TotalResponses = responseQuery.Count();
                report.TotalPages = (int)Math.Ceiling(report.TotalResponses / (double)report.PageSize);
                report.CurrentPage = report.TotalPages == 0 ? 1 : Math.Max(1, Math.Min(page, report.TotalPages));

                if (report.ReportTab == "responses")
                {
                    report.Responses = responseQuery
                        .Include(x => x.SurveyForm)
                        .OrderByDescending(x => x.SubmittedDate).ThenByDescending(x => x.Id)
                        .Skip((report.CurrentPage - 1) * report.PageSize)
                        .Take(report.PageSize)
                        .Select(x => new SurveyResponseSummaryViewModel
                        {
                            Id = x.Id,
                            FormTitle = x.SurveyForm.Title,
                            RespondentName = x.RespondentName,
                            RespondentMobile = x.RespondentMobile,
                            SubmittedBy = x.SubmittedBy,
                            SubmittedDate = x.SubmittedDate
                        }).ToList();
                }

                var form = selectedForm == null ? null : db.SurveyForms.AsNoTracking()
                    .Include(x => x.Questions.Select(q => q.Options))
                    .Include(x => x.Questions.Select(q => q.GridRows))
                    .Include(x => x.Questions.Select(q => q.GridColumns))
                    .FirstOrDefault(x => x.Id == selectedForm.Id);

                if (form != null && report.ReportTab == "summary")
                {
                    var responses = responseQuery
                        .Include(x => x.Answers.Select(a => a.GridAnswers))
                        .ToList();
                    var questionSnapshots = GetQuestionSnapshots(db, selectedForm.Id);
                    report.Questions = BuildQuestionVersions(form.Questions, questionSnapshots)
                        .Select(version => BuildQuestionReport(version.Question, responses, version.QuestionText, version.QuestionType, version.DisplayOrder, version.IsCurrent))
                        .ToList();
                }

                return report;
            }
        }

        public SurveyResponseDetailViewModel GetResponseDetail(int responseId)
        {
            using (var db = new ApplicationDbContext())
            {
                var response = db.SurveyFormResponses.AsNoTracking()
                    .Include(x => x.SurveyForm.SurveyTemplate)
                    .Include(x => x.SurveyCustomerData)
                    .Include(x => x.Answers.Select(a => a.GridAnswers))
                    .Include(x => x.Answers.Select(a => a.SurveyQuestion))
                    .FirstOrDefault(x => x.Id == responseId);
                if (response == null)
                {
                    return null;
                }

                var customer = response.SurveyCustomerData;
                SurveyCustomerLookupViewModel customerSnapshot = null;
                if (customer != null)
                {
                    var tracking = db.SurveyCustomerFormTrackings.AsNoTracking()
                        .Include(x => x.CallStatus)
                        .Include(x => x.FormStatus)
                        .FirstOrDefault(x => x.SurveyCustomerDataId == customer.Id
                            && x.SurveyTemplateTypeId == response.SurveyForm.SurveyTemplateId
                            && x.SurveyFormId == response.SurveyFormId);

                    customerSnapshot = new SurveyCustomerLookupViewModel
                    {
                        Id = customer.Id,
                        SurveyTemplateTypeId = customer.SurveyTemplateTypeId,
                        ClientName = customer.ClientName,
                        Gender = customer.Gender,
                        LoanProduct = customer.LoanProduct,
                        CustomerCode = customer.CustomerCode,
                        MobileNumber1 = customer.MobileNumber1,
                        MobileNumber2 = customer.MobileNumber2,
                        Region = customer.Region,
                        Branch = customer.Branch,
                        AreaType = customer.AreaType,
                        Location = customer.Location,
                        Age = customer.Age,
                        NumberOfFamilyMembers = customer.NumberOfFamilyMembers,
                        BusinessCategory = customer.BusinessCategory,
                        ActivitiesSector = customer.ActivitiesSector,
                        LevelOfEducation = customer.LevelOfEducation,
                        IncomeLevel = customer.IncomeLevel,
                        HouseholdAssets = customer.HouseholdAssets,
                        PovertyScore = customer.PovertyScore,
                        LoanCycle = customer.LoanCycle,
                        DisbursedAmount = customer.DisbursedAmount,
                        CustomerStatus = customer.CustomerStatus,
                        CallStatus = tracking != null && tracking.CallStatus != null ? tracking.CallStatus.Name : string.Empty,
                        FormStatus = tracking != null && tracking.FormStatus != null ? tracking.FormStatus.Name : string.Empty,
                        CallRemarks = tracking != null ? tracking.CallRemarks : string.Empty
                    };
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
                    Customer = customerSnapshot,
                    Answers = response.Answers
                        .OrderBy(x => x.QuestionDisplayOrderSnapshot ?? (x.SurveyQuestion != null ? x.SurveyQuestion.DisplayOrder : int.MaxValue))
                        .Select(x => new SurveyResponseAnswerViewModel
                    {
                        QuestionText = x.QuestionTextSnapshot ?? (x.SurveyQuestion != null ? x.SurveyQuestion.QuestionText : string.Empty),
                        QuestionType = x.QuestionTypeSnapshot ?? (x.SurveyQuestion != null ? x.SurveyQuestion.QuestionType : string.Empty),
                        Answer = FormatAnswer(x)
                    }).ToList()
                };
            }
        }

        public byte[] ExportCsv(int templateId, int formId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            using (var db = new ApplicationDbContext())
            {
                var form = db.SurveyForms.AsNoTracking()
                    .Include(x => x.Questions)
                    .FirstOrDefault(x => x.Id == formId && x.SurveyTemplateId == templateId);
                if (form == null)
                {
                    throw new ArgumentException("The selected form does not belong to the selected template.");
                }

                var responses = GetFilteredResponses(db, formId, templateId, categoryId, fromDate, toDate);
                var questionSnapshots = GetQuestionSnapshots(db, formId);
                var questions = form.Questions
                    .OrderBy(question => question.DisplayOrder)
                    .Select(question => new SurveyQuestionExportColumn
                    {
                        QuestionId = question.Id,
                        QuestionText = question.QuestionText,
                        QuestionType = question.QuestionType,
                        DisplayOrder = question.DisplayOrder,
                        IsCurrent = !question.IsDeleted
                    })
                    .ToList();

                var currentQuestions = form.Questions.Where(question => !question.IsDeleted).ToDictionary(question => question.Id);
                var historicalQuestionVersions = questionSnapshots
                    .Select(snapshot => new SurveyQuestionExportColumn
                    {
                        QuestionId = snapshot.QuestionId,
                        QuestionText = snapshot.QuestionText,
                        QuestionType = snapshot.QuestionType,
                        DisplayOrder = snapshot.DisplayOrder,
                        IsCurrent = currentQuestions.ContainsKey(snapshot.QuestionId)
                            && string.Equals(currentQuestions[snapshot.QuestionId].QuestionText, snapshot.QuestionText, StringComparison.Ordinal)
                            && string.Equals(currentQuestions[snapshot.QuestionId].QuestionType, snapshot.QuestionType, StringComparison.Ordinal)
                    })
                    .Where(version =>
                    {
                        return !questions.Any(question => question.QuestionId == version.QuestionId
                            && string.Equals(question.QuestionText, version.QuestionText, StringComparison.Ordinal)
                            && string.Equals(question.QuestionType, version.QuestionType, StringComparison.Ordinal));
                    })
                    .ToList();
                questions.AddRange(historicalQuestionVersions);
                questions = BuildExportQuestionColumns(questions);

                var customerIds = responses.Where(x => x.SurveyCustomerDataId.HasValue).Select(x => x.SurveyCustomerDataId.Value).Distinct().ToList();
                var trackingByCustomerAndForm = customerIds.Any()
                    ? db.SurveyCustomerFormTrackings.AsNoTracking()
                        .Include(x => x.CallStatus)
                        .Include(x => x.FormStatus)
                        .Where(x => customerIds.Contains(x.SurveyCustomerDataId) && x.SurveyFormId == formId)
                        .ToList()
                        .GroupBy(x => new { x.SurveyCustomerDataId, x.SurveyFormId })
                        .ToDictionary(x => Tuple.Create(x.Key.SurveyCustomerDataId, x.Key.SurveyFormId), x => x.First())
                    : new Dictionary<Tuple<int, int>, SurveyCustomerFormTracking>();

                var builder = new StringBuilder();
                var headers = new[]
                {
                    "ClientName", "Gender", "CustomerCode", "MobileNumber1", "MobileNumber2",
                    "Region", "Branch", "AreaType", "Location", "LoanProduct", "Age",
                    "NumberOfFamilyMembers", "BusinessCategory", "ActivitiesSector", "LevelOfEducation",
                    "IncomeLevel", "HouseholdAssets", "PovertyScore", "LoanCycle", "DisbursedAmount",
                    "CustomerStatus", "Call Status", "Form Status", "Call Remarks", "Response Id",
                    "Respondent Name", "Respondent Mobile", "Submitted By", "Submitted Date", "Form"
                }.Concat(questions.Select(question => question.Header));
                var headerList = headers.ToList();
                builder.AppendLine(string.Join(",", headerList.Select(CsvEscape)));

                foreach (var response in responses.OrderByDescending(x => x.SubmittedDate).ThenByDescending(x => x.Id))
                {
                    var customer = response.SurveyCustomerData;
                    SurveyCustomerFormTracking tracking;
                    trackingByCustomerAndForm.TryGetValue(Tuple.Create(response.SurveyCustomerDataId.GetValueOrDefault(), response.SurveyFormId), out tracking);

                    var values = new List<string>
                    {
                        customer != null && !string.IsNullOrWhiteSpace(customer.ClientName) ? customer.ClientName : response.RespondentName,
                        customer != null ? customer.Gender : string.Empty,
                        customer != null ? customer.CustomerCode : string.Empty,
                        customer != null && !string.IsNullOrWhiteSpace(customer.MobileNumber1) ? customer.MobileNumber1 : response.RespondentMobile,
                        customer != null ? customer.MobileNumber2 : string.Empty,
                        customer != null ? customer.Region : string.Empty,
                        customer != null ? customer.Branch : string.Empty,
                        customer != null ? customer.AreaType : string.Empty,
                        customer != null ? customer.Location : string.Empty,
                        customer != null ? customer.LoanProduct : string.Empty,
                        customer != null && customer.Age.HasValue ? customer.Age.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                        customer != null && customer.NumberOfFamilyMembers.HasValue ? customer.NumberOfFamilyMembers.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                        customer != null ? customer.BusinessCategory : string.Empty,
                        customer != null ? customer.ActivitiesSector : string.Empty,
                        customer != null ? customer.LevelOfEducation : string.Empty,
                        customer != null ? customer.IncomeLevel : string.Empty,
                        customer != null ? customer.HouseholdAssets : string.Empty,
                        customer != null && customer.PovertyScore.HasValue ? customer.PovertyScore.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                        customer != null && customer.LoanCycle.HasValue ? customer.LoanCycle.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                        customer != null ? customer.DisbursedAmount : string.Empty,
                        customer != null ? customer.CustomerStatus : string.Empty,
                        tracking != null && tracking.CallStatus != null ? tracking.CallStatus.Name : string.Empty,
                        tracking != null && tracking.FormStatus != null ? tracking.FormStatus.Name : string.Empty,
                        tracking != null ? tracking.CallRemarks : string.Empty,
                        response.Id.ToString(CultureInfo.InvariantCulture),
                        response.RespondentName,
                        response.RespondentMobile,
                        response.SubmittedBy,
                        response.SubmittedDate.ToString("d/M/yyyy HH:mm", CultureInfo.InvariantCulture),
                        response.SurveyForm != null ? response.SurveyForm.Title : string.Empty
                    };

                    values.AddRange(questions.Select(question =>
                        {
                            var answer = response.Answers.FirstOrDefault(x => x.SurveyQuestionId == question.QuestionId
                                && string.Equals(x.QuestionTextSnapshot ?? question.QuestionText, question.QuestionText, StringComparison.Ordinal)
                                && string.Equals(x.QuestionTypeSnapshot ?? question.QuestionType, question.QuestionType, StringComparison.Ordinal));
                            return answer == null ? string.Empty : FormatAnswer(answer);
                        }));

                    builder.AppendLine(string.Join(",", values.Select(CsvEscape)));
                }

                return Encoding.UTF8.GetBytes(builder.ToString());
            }
        }

        private static List<SurveyFormResponse> GetFilteredResponses(ApplicationDbContext db, int? formId, int? templateId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            return GetFilteredResponseQuery(db, formId, templateId, categoryId, fromDate, toDate)
                .Include(x => x.SurveyForm)
                .Include(x => x.SurveyCustomerData)
                .Include(x => x.Answers.Select(a => a.GridAnswers))
                .ToList();
        }

        private static IQueryable<SurveyFormResponse> GetFilteredResponseQuery(ApplicationDbContext db, int? formId, int? templateId, int? categoryId, DateTime? fromDate, DateTime? toDate, bool invalidDateFilter = false)
        {
            var query = db.SurveyFormResponses.AsNoTracking().AsQueryable();
            if (formId.HasValue)
            {
                query = query.Where(x => x.SurveyFormId == formId.Value);
            }
            else if (templateId.HasValue)
            {
                query = query.Where(x => x.SurveyForm.SurveyTemplateId == templateId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(x => x.SurveyCategoryId == categoryId.Value);
            }

            if (invalidDateFilter || (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date))
            {
                return query.Where(x => false);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(x => x.SubmittedDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                var endDate = toDate.Value.Date;
                if (endDate < DateTime.MaxValue.Date)
                {
                    query = query.Where(x => x.SubmittedDate < endDate.AddDays(1));
                }
            }

            return query;
        }

        private static List<SurveyQuestionVersion> BuildQuestionVersions(
            IEnumerable<SurveyQuestion> formQuestions,
            List<SurveyQuestionVersion> questionSnapshots)
        {
            var versions = formQuestions.Select(question => new SurveyQuestionVersion
            {
                Question = question,
                QuestionId = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                DisplayOrder = question.DisplayOrder,
                IsCurrent = !question.IsDeleted
            }).ToList();

            var currentQuestions = formQuestions.Where(question => !question.IsDeleted).ToDictionary(question => question.Id);
            var historicalVersions = questionSnapshots
                .Select(snapshot =>
                {
                    SurveyQuestion currentQuestion;
                    var isCurrent = currentQuestions.TryGetValue(snapshot.QuestionId, out currentQuestion)
                        && string.Equals(currentQuestion.QuestionText, snapshot.QuestionText, StringComparison.Ordinal)
                        && string.Equals(currentQuestion.QuestionType, snapshot.QuestionType, StringComparison.Ordinal);
                    return new SurveyQuestionVersion
                    {
                        Question = isCurrent ? currentQuestion : new SurveyQuestion
                        {
                            Id = snapshot.QuestionId,
                            QuestionText = snapshot.QuestionText,
                            QuestionType = snapshot.QuestionType,
                            DisplayOrder = snapshot.DisplayOrder
                        },
                        QuestionId = snapshot.QuestionId,
                        QuestionText = snapshot.QuestionText,
                        QuestionType = snapshot.QuestionType,
                        DisplayOrder = snapshot.DisplayOrder,
                        IsCurrent = isCurrent
                    };
                })
                .Where(version => !versions.Any(existing => existing.QuestionId == version.QuestionId
                    && string.Equals(existing.QuestionText, version.QuestionText, StringComparison.Ordinal)
                    && string.Equals(existing.QuestionType, version.QuestionType, StringComparison.Ordinal)))
                .ToList();

            versions.AddRange(historicalVersions);
            return versions
                .GroupBy(version => new { version.QuestionId, version.QuestionText, version.QuestionType })
                .Select(group => group.OrderByDescending(version => version.IsCurrent).First())
                .OrderBy(version => version.DisplayOrder)
                .ThenBy(version => version.QuestionId)
                .ThenBy(version => version.IsCurrent)
                .ToList();
        }

        private static List<SurveyQuestionVersion> GetQuestionSnapshots(ApplicationDbContext db, int formId)
        {
            return db.SurveyFormAnswers.AsNoTracking()
                .Where(answer => answer.SurveyFormResponse.SurveyFormId == formId
                    && answer.QuestionTextSnapshot != null
                    && answer.QuestionTypeSnapshot != null)
                .Select(answer => new
                {
                    answer.SurveyQuestionId,
                    answer.QuestionTextSnapshot,
                    answer.QuestionTypeSnapshot,
                    answer.QuestionDisplayOrderSnapshot
                })
                .ToList()
                .GroupBy(snapshot => new
                {
                    snapshot.SurveyQuestionId,
                    snapshot.QuestionTextSnapshot,
                    snapshot.QuestionTypeSnapshot,
                    snapshot.QuestionDisplayOrderSnapshot
                })
                .Select(group => new SurveyQuestionVersion
                {
                    QuestionId = group.Key.SurveyQuestionId,
                    QuestionText = group.Key.QuestionTextSnapshot,
                    QuestionType = group.Key.QuestionTypeSnapshot,
                    DisplayOrder = group.Key.QuestionDisplayOrderSnapshot ?? int.MaxValue
                })
                .ToList();
        }

        private static SurveyQuestionReportViewModel BuildQuestionReport(
            SurveyQuestion question,
            List<SurveyFormResponse> responses,
            string questionText,
            string questionType,
            int displayOrder,
            bool isCurrent)
        {
            var answers = responses.SelectMany(response => response.Answers.Where(answer =>
                answer.SurveyQuestionId == question.Id
                && ((answer.QuestionTextSnapshot == null && isCurrent)
                    || string.Equals(answer.QuestionTextSnapshot, questionText, StringComparison.Ordinal))
                && ((answer.QuestionTypeSnapshot == null && isCurrent)
                    || string.Equals(answer.QuestionTypeSnapshot, questionType, StringComparison.Ordinal)))).ToList();
            var values = answers.Select(GetPrimaryValue).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var answeredResponseCount = answers.Where(HasAnswer).Select(x => x.SurveyFormResponseId).Distinct().Count();
            var report = new SurveyQuestionReportViewModel
            {
                QuestionId = question.Id,
                DisplayOrder = displayOrder,
                QuestionText = questionText,
                QuestionType = questionType,
                AnsweredCount = answeredResponseCount,
                UnansweredCount = Math.Max(0, responses.Count - answeredResponseCount)
            };

            if (questionType == SurveyQuestionTypeCatalog.Checkboxes)
            {
                report.Options = BuildOptions(question.Options.Select(x => x.OptionText), answers.SelectMany(GetCheckboxValues), report.AnsweredCount);
            }
            else if (questionType == SurveyQuestionTypeCatalog.MultipleChoice || questionType == SurveyQuestionTypeCatalog.Dropdown)
            {
                report.Options = BuildOptions(question.Options.Select(x => x.OptionText), values, report.AnsweredCount);
            }
            else if (questionType == SurveyQuestionTypeCatalog.Nps)
            {
                AddNumericSummary(report, values);
                report.Promoters = values.Count(IsPromoter);
                report.Passives = values.Count(IsPassive);
                report.Detractors = values.Count(IsDetractor);
                report.NpsScore = values.Count == 0 ? (double?)null : Math.Round((report.Promoters - report.Detractors) * 100d / values.Count, 2);
            }
            else if (questionType == SurveyQuestionTypeCatalog.LinearScale || questionType == SurveyQuestionTypeCatalog.Ranking)
            {
                AddNumericSummary(report, values);
            }
            else if (questionType == SurveyQuestionTypeCatalog.MultipleChoiceGrid || questionType == SurveyQuestionTypeCatalog.CheckboxGrid)
            {
                var gridRows = question.GridRows.Any()
                    ? question.GridRows.OrderBy(row => row.DisplayOrder).Select(row => row.RowText).ToList()
                    : answers.SelectMany(answer => answer.GridAnswers).Select(answer => answer.RowText).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                report.GridRows = gridRows.Select(rowText => new SurveyReportGridRowViewModel
                {
                    RowText = rowText,
                    Options = BuildGridOptions(question, answers, rowText)
                }).ToList();
            }
            else
            {
                report.TextAnswers = values.Take(100).ToList();
            }

            return report;
        }

        private static List<SurveyQuestionExportColumn> BuildExportQuestionColumns(List<SurveyQuestionExportColumn> questions)
        {
            var distinctQuestions = questions
                .GroupBy(question => new { question.QuestionId, question.QuestionText, question.QuestionType })
                .Select(group => group.OrderByDescending(question => question.IsCurrent).First())
                .OrderBy(question => question.DisplayOrder)
                .ThenBy(question => question.QuestionId)
                .ThenBy(question => question.IsCurrent)
                .ToList();

            foreach (var group in distinctQuestions.GroupBy(question => question.QuestionId))
            {
                var versions = group.ToList();
                if (versions.Count > 1)
                {
                    var historicalVersions = versions.Where(question => !question.IsCurrent)
                        .OrderBy(question => question.DisplayOrder)
                        .ThenBy(question => question.QuestionText, StringComparer.Ordinal)
                        .ToList();
                    for (var index = 0; index < historicalVersions.Count; index++)
                    {
                        historicalVersions[index].Header = historicalVersions.Count == 1
                            ? historicalVersions[index].QuestionText + " (Historical)"
                            : historicalVersions[index].QuestionText + " (Historical version " + (index + 1) + ")";
                    }
                }
            }

            foreach (var question in distinctQuestions.Where(question => question.Header == null))
            {
                question.Header = question.QuestionText;
            }

            foreach (var duplicateHeader in distinctQuestions.GroupBy(question => question.Header, StringComparer.OrdinalIgnoreCase).Where(group => group.Count() > 1))
            {
                foreach (var question in duplicateHeader)
                {
                    question.Header = string.Format("{0} (Question {1}, ID {2})", question.Header, question.DisplayOrder, question.QuestionId);
                }
            }

            return distinctQuestions;
        }

        private static bool HasAnswer(SurveyFormAnswer answer)
        {
            return !string.IsNullOrWhiteSpace(answer.AnswerText)
                || !string.IsNullOrWhiteSpace(answer.SelectedOption)
                || !string.IsNullOrWhiteSpace(answer.SelectedOptionsCsv)
                || !string.IsNullOrWhiteSpace(answer.FileName)
                || !string.IsNullOrWhiteSpace(answer.FilePath)
                || (answer.GridAnswers != null && answer.GridAnswers.Any(x =>
                    !string.IsNullOrWhiteSpace(x.SelectedColumnText)
                    || !string.IsNullOrWhiteSpace(x.SelectedColumnTextsCsv)));
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

        private sealed class SurveyQuestionVersion
        {
            public SurveyQuestion Question { get; set; }
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public string QuestionType { get; set; }
            public int DisplayOrder { get; set; }
            public bool IsCurrent { get; set; }
        }

        private sealed class SurveyQuestionExportColumn
        {
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public string QuestionType { get; set; }
            public int DisplayOrder { get; set; }
            public bool IsCurrent { get; set; }
            public string Header { get; set; }
        }

        private static string CsvEscape(string value)
        {
            var safeValue = value ?? string.Empty;
            return "\"" + safeValue.Replace("\"", "\"\"") + "\"";
        }
    }
}
