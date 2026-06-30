using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using CallCenterSecure.Models;
using CallCenterSecure.Models.ViewModels;
using CallCenterSecure.Repositories;

namespace CallCenterSecure.Services
{
    public class SurveyResponseService : ISurveyResponseService
    {
        private static readonly HashSet<string> AllowedFileExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png"
        };

        private const int MaxFileSizeBytes = 5 * 1024 * 1024;

        private readonly ISurveyResponseRepository _surveyResponseRepository;

        public SurveyResponseService()
            : this(new SurveyResponseRepository())
        {
        }

        internal SurveyResponseService(ISurveyResponseRepository surveyResponseRepository)
        {
            _surveyResponseRepository = surveyResponseRepository;
        }

        public SurveyFormResponseListViewModel GetStartModel(int? templateId, int? formId, int? customerId, int? categoryId)
        {
            var model = new SurveyFormResponseListViewModel();
            model.Categories = GetCategories();
            var templates = _surveyResponseRepository.GetTemplates().ToList();

            model.Templates = templates.Select(x => new SurveyTemplateLookupViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();

            model.SelectedTemplateId = templateId ?? templates.Select(x => (int?)x.Id).FirstOrDefault();

            if (model.SelectedTemplateId.HasValue)
            {
                model.Forms = _surveyResponseRepository.GetFormsByTemplateId(model.SelectedTemplateId.Value)
                    .Select(x => new SurveyFormLookupViewModel
                    {
                        Id = x.Id,
                        SurveyTemplateId = x.SurveyTemplateId,
                        Title = x.Title
                    }).ToList();

                model.Customers = _surveyResponseRepository.GetCustomersByTemplateId(model.SelectedTemplateId.Value)
                    .Select(x => new SurveyCustomerLookupViewModel
                    {
                        Id = x.Id,
                        SurveyTemplateTypeId = x.SurveyTemplateTypeId,
                        ClientName = x.ClientName,
                        CustomerCode = x.CustomerCode
                    }).ToList();
            }

            model.SelectedFormId = formId;
            model.SelectedCustomerId = customerId;
            model.SelectedCategoryId = categoryId;

            return model;
        }

        public SurveyFormResponseSubmitViewModel BuildSubmitModel(int formId, int? customerId, int? categoryId)
        {
            var form = _surveyResponseRepository.GetFormWithQuestions(formId);
            if (form == null)
            {
                return null;
            }

            var model = new SurveyFormResponseSubmitViewModel
            {
                SurveyFormId = form.Id,
                SurveyTemplateId = form.SurveyTemplateId,
                SurveyCategoryId = categoryId,
                SurveyCategoryName = GetCategories().FirstOrDefault(x => x.Id == categoryId.GetValueOrDefault()) != null ? GetCategories().FirstOrDefault(x => x.Id == categoryId.GetValueOrDefault()).Name : string.Empty,
                SurveyCustomerDataId = customerId,
                SurveyTemplateName = form.SurveyTemplate != null ? form.SurveyTemplate.Name : string.Empty,
                SurveyFormTitle = form.Title,
                SurveyFormCategory = form.Category,
                SurveyFormDescription = form.Description,
                Questions = form.Questions
                    .OrderBy(x => x.DisplayOrder)
                    .Select(MapQuestion)
                    .ToList()
            };

            if (customerId.HasValue)
            {
                var customer = _surveyResponseRepository.GetCustomerById(customerId.Value);
                if (customer != null)
                {
                    model.SelectedCustomerName = customer.ClientName;
                    model.SelectedCustomerCode = customer.CustomerCode;
                    model.RespondentName = customer.ClientName;
                    model.RespondentMobile = !string.IsNullOrWhiteSpace(customer.MobileNumber1) ? customer.MobileNumber1 : customer.MobileNumber2;
                }
            }

            return model;
        }

        public SurveyFormResponseSubmitViewModel RehydrateSubmitModel(SurveyFormResponseSubmitViewModel model)
        {
            var latest = BuildSubmitModel(model.SurveyFormId, model.SurveyCustomerDataId, model.SurveyCategoryId);
            if (latest == null)
            {
                return null;
            }

            latest.RespondentName = model.RespondentName;
            latest.RespondentMobile = model.RespondentMobile;
            latest.SurveyCategoryName = model.SurveyCategoryName;

            var answerLookup = (model.Questions ?? new List<SurveyQuestionResponseViewModel>())
                .ToDictionary(x => x.SurveyQuestionId, x => x);

            foreach (var question in latest.Questions)
            {
                SurveyQuestionResponseViewModel incoming;
                if (!answerLookup.TryGetValue(question.SurveyQuestionId, out incoming))
                {
                    continue;
                }

                question.AnswerText = incoming.AnswerText;
                question.SelectedOption = incoming.SelectedOption;
                question.SelectedOptions = incoming.SelectedOptions ?? new List<string>();
                question.File = incoming.File;

                if (incoming.GridAnswers != null && incoming.GridAnswers.Any())
                {
                    foreach (var row in question.GridAnswers)
                    {
                        var incomingRow = incoming.GridAnswers.FirstOrDefault(x => string.Equals(x.RowText, row.RowText, StringComparison.OrdinalIgnoreCase));
                        if (incomingRow == null)
                        {
                            continue;
                        }

                        row.SelectedColumnText = incomingRow.SelectedColumnText;
                        row.SelectedColumnTexts = incomingRow.SelectedColumnTexts ?? new List<string>();
                    }
                }
            }

            return latest;
        }

        public void Submit(SurveyFormResponseSubmitViewModel model, string submittedBy, HttpServerUtilityBase server)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var hydrated = RehydrateSubmitModel(model);
            if (hydrated == null)
            {
                throw new InvalidOperationException("Survey form not found.");
            }

            Validate(hydrated);

            var response = new SurveyFormResponse
            {
                SurveyFormId = hydrated.SurveyFormId,
                SurveyCategoryId = hydrated.SurveyCategoryId,
                SurveyCustomerDataId = hydrated.SurveyCustomerDataId,
                RespondentName = TrimOrNull(hydrated.RespondentName),
                RespondentMobile = TrimOrNull(hydrated.RespondentMobile),
                SubmittedBy = string.IsNullOrWhiteSpace(submittedBy) ? "System" : submittedBy,
                SubmittedDate = DateTime.Now,
                Answers = new List<SurveyFormAnswer>()
            };

            foreach (var question in hydrated.Questions)
            {
                var answer = new SurveyFormAnswer
                {
                    SurveyQuestionId = question.SurveyQuestionId,
                    AnswerText = TrimOrNull(question.AnswerText),
                    SelectedOption = TrimOrNull(question.SelectedOption),
                    SelectedOptionsCsv = question.SelectedOptions != null && question.SelectedOptions.Any() ? string.Join(",", question.SelectedOptions.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim())) : null
                };

                if (question.QuestionType == SurveyQuestionTypeCatalog.FileUpload && question.File != null && question.File.ContentLength > 0)
                {
                    var fileInfo = SaveFile(question.File, server);
                    answer.FileName = fileInfo.FileName;
                    answer.FilePath = fileInfo.FilePath;
                }

                if (question.QuestionType == SurveyQuestionTypeCatalog.MultipleChoiceGrid || question.QuestionType == SurveyQuestionTypeCatalog.CheckboxGrid)
                {
                    var gridAnswers = new List<SurveyFormGridAnswer>();
                    foreach (var row in question.GridAnswers)
                    {
                        gridAnswers.Add(new SurveyFormGridAnswer
                        {
                            RowText = row.RowText,
                            SelectedColumnText = TrimOrNull(row.SelectedColumnText),
                            SelectedColumnTextsCsv = row.SelectedColumnTexts != null && row.SelectedColumnTexts.Any() ? string.Join(",", row.SelectedColumnTexts.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim())) : null
                        });
                    }

                    answer.GridAnswers = gridAnswers;
                }

                response.Answers.Add(answer);
            }

            _surveyResponseRepository.AddResponse(response);
            _surveyResponseRepository.SaveChanges();
        }

        private static List<SurveyCategoryLookupViewModel> GetCategories()
        {
            return new List<SurveyCategoryLookupViewModel>
            {
                new SurveyCategoryLookupViewModel { Id = 1, Name = "Complaint" },
                new SurveyCategoryLookupViewModel { Id = 2, Name = "Enquiry" }
            };
        }

        private static SurveyQuestionResponseViewModel MapQuestion(SurveyQuestion question)
        {
            var vm = new SurveyQuestionResponseViewModel
            {
                SurveyQuestionId = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                DisplayOrder = question.DisplayOrder,
                IsRequired = question.IsRequired,
                MinValue = question.MinValue,
                MaxValue = question.MaxValue,
                MinLabel = question.MinLabel,
                MaxLabel = question.MaxLabel,
                Options = question.Options.OrderBy(x => x.DisplayOrder).Select(x => new SurveyOptionLookupViewModel { OptionText = x.OptionText }).ToList(),
                GridRows = question.GridRows.OrderBy(x => x.DisplayOrder).Select(x => x.RowText).ToList(),
                GridColumns = question.GridColumns.OrderBy(x => x.DisplayOrder).Select(x => x.ColumnText).ToList()
            };

            vm.GridAnswers = vm.GridRows.Select(x => new SurveyGridAnswerInputViewModel
            {
                RowText = x,
                SelectedColumnTexts = new List<string>()
            }).ToList();

            return vm;
        }

        private static void Validate(SurveyFormResponseSubmitViewModel model)
        {
            foreach (var question in model.Questions)
            {
                if (!question.IsRequired)
                {
                    continue;
                }

                switch (question.QuestionType)
                {
                    case SurveyQuestionTypeCatalog.ShortAnswer:
                    case SurveyQuestionTypeCatalog.Paragraph:
                        if (string.IsNullOrWhiteSpace(question.AnswerText))
                        {
                            throw new InvalidOperationException("All required text questions must be answered.");
                        }
                        break;
                    case SurveyQuestionTypeCatalog.MultipleChoice:
                    case SurveyQuestionTypeCatalog.Dropdown:
                        if (string.IsNullOrWhiteSpace(question.SelectedOption))
                        {
                            throw new InvalidOperationException("All required selection questions must be answered.");
                        }
                        break;
                    case SurveyQuestionTypeCatalog.Checkboxes:
                        if (question.SelectedOptions == null || !question.SelectedOptions.Any())
                        {
                            throw new InvalidOperationException("All required checkbox questions must be answered.");
                        }
                        break;
                    case SurveyQuestionTypeCatalog.FileUpload:
                        if (question.File == null || question.File.ContentLength == 0)
                        {
                            throw new InvalidOperationException("All required file upload questions must be answered.");
                        }
                        break;
                    case SurveyQuestionTypeCatalog.LinearScale:
                        int value;
                        if (string.IsNullOrWhiteSpace(question.AnswerText) || !int.TryParse(question.AnswerText, out value))
                        {
                            throw new InvalidOperationException("All required linear scale questions must be answered.");
                        }

                        if (question.MinValue.HasValue && value < question.MinValue.Value)
                        {
                            throw new InvalidOperationException("Linear scale answer is below minimum value.");
                        }

                        if (question.MaxValue.HasValue && value > question.MaxValue.Value)
                        {
                            throw new InvalidOperationException("Linear scale answer is above maximum value.");
                        }
                        break;
                    case SurveyQuestionTypeCatalog.MultipleChoiceGrid:
                        if (question.GridAnswers.Any(x => string.IsNullOrWhiteSpace(x.SelectedColumnText)))
                        {
                            throw new InvalidOperationException("All required multiple choice grid rows must be answered.");
                        }
                        break;
                    case SurveyQuestionTypeCatalog.CheckboxGrid:
                        if (question.GridAnswers.Any(x => x.SelectedColumnTexts == null || !x.SelectedColumnTexts.Any()))
                        {
                            throw new InvalidOperationException("All required checkbox grid rows must be answered.");
                        }
                        break;
                }

                if (question.QuestionType == SurveyQuestionTypeCatalog.FileUpload && question.File != null && question.File.ContentLength > 0)
                {
                    ValidateFile(question.File);
                }
            }
        }

        private static void ValidateFile(HttpPostedFileBase file)
        {
            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedFileExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Unsupported file type for upload question.");
            }

            if (file.ContentLength > MaxFileSizeBytes)
            {
                throw new InvalidOperationException("Uploaded file exceeds 5 MB size limit.");
            }
        }

        private static SavedFileInfo SaveFile(HttpPostedFileBase file, HttpServerUtilityBase server)
        {
            ValidateFile(file);

            var extension = Path.GetExtension(file.FileName);
            var nameOnly = Path.GetFileNameWithoutExtension(file.FileName);
            var safeName = string.Concat(nameOnly.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safeName))
            {
                safeName = "survey_file";
            }

            var uniqueFileName = safeName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + extension;
            var relativeFolder = "~/Uploads/SurveyResponses";
            var folderPath = server.MapPath(relativeFolder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var fullPath = Path.Combine(folderPath, uniqueFileName);
            file.SaveAs(fullPath);

            return new SavedFileInfo
            {
                FileName = uniqueFileName,
                FilePath = fullPath
            };
        }

        private static string TrimOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private sealed class SavedFileInfo
        {
            public string FileName { get; set; }
            public string FilePath { get; set; }
        }
    }
}
