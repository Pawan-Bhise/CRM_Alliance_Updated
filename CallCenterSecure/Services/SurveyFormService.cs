using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CallCenterSecure.Models;
using CallCenterSecure.Models.ViewModels;
using CallCenterSecure.Repositories;

namespace CallCenterSecure.Services
{
    public class SurveyFormService : ISurveyFormService
    {
        private readonly ISurveyFormRepository _surveyFormRepository;

        public SurveyFormService()
            : this(new SurveyFormRepository())
        {
        }

        internal SurveyFormService(ISurveyFormRepository surveyFormRepository)
        {
            _surveyFormRepository = surveyFormRepository;
        }

        public SurveyFormIndexViewModel GetIndexModel(int? surveyTemplateId)
        {
            var templates = _surveyFormRepository.GetAllTemplates().ToList();
            var selectedTemplateId = surveyTemplateId ?? templates.Select(x => (int?)x.Id).FirstOrDefault();

            var model = new SurveyFormIndexViewModel
            {
                SelectedSurveyTemplateId = selectedTemplateId,
                SurveyTemplates = templates.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                    Selected = selectedTemplateId.HasValue && selectedTemplateId.Value == x.Id
                }).ToList()
            };

            if (selectedTemplateId.HasValue)
            {
                model.Forms = _surveyFormRepository.GetFormsByTemplateId(selectedTemplateId.Value)
                    .Select(x => new SurveyFormListItemViewModel
                    {
                        Id = x.Id,
                        SurveyTemplateId = x.SurveyTemplateId,
                        SurveyTemplateName = x.SurveyTemplate != null ? x.SurveyTemplate.Name : string.Empty,
                        Title = x.Title,
                        Category = x.Category,
                        QuestionCount = x.Questions != null ? x.Questions.Count : 0,
                        CreatedDate = x.CreatedDate,
                        ModifiedDate = x.ModifiedDate,
                        IsActive = x.IsActive
                    })
                    .ToList();
            }

            return model;
        }

        public SurveyFormBuilderViewModel GetCreateModel(int? surveyTemplateId)
        {
            var model = new SurveyFormBuilderViewModel();
            var templates = _surveyFormRepository.GetAllTemplates().ToList();
            var selectedTemplateId = surveyTemplateId ?? templates.Select(x => x.Id).FirstOrDefault();

            model.SurveyTemplateId = selectedTemplateId;
            model.SurveyTemplates = templates.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
                Selected = x.Id == selectedTemplateId
            }).ToList();

            return model;
        }

        public SurveyFormBuilderViewModel GetEditModel(int id)
        {
            var form = _surveyFormRepository.GetFormById(id);
            if (form == null)
            {
                return null;
            }

            var model = new SurveyFormBuilderViewModel
            {
                Id = form.Id,
                SurveyTemplateId = form.SurveyTemplateId,
                Title = form.Title,
                Category = form.Category,
                Description = form.Description,
                Questions = form.Questions
                    .OrderBy(q => q.DisplayOrder)
                    .Select(MapQuestionToViewModel)
                    .ToList()
            };

            return PrepareBuilderModel(model);
        }

        public SurveyFormBuilderViewModel PrepareBuilderModel(SurveyFormBuilderViewModel model)
        {
            var templates = _surveyFormRepository.GetAllTemplates().ToList();
            model.SurveyTemplates = templates.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
                Selected = x.Id == model.SurveyTemplateId
            }).ToList();

            model.AvailableQuestionTypes = SurveyQuestionTypeCatalog.GetSelectList();
            model.Questions = model.Questions ?? new List<SurveyQuestionBuilderViewModel>();

            for (var questionIndex = 0; questionIndex < model.Questions.Count; questionIndex++)
            {
                var question = model.Questions[questionIndex];
                question.DisplayOrder = questionIndex + 1;
                question.Options = (question.Options ?? new List<SurveyQuestionOptionViewModel>())
                    .Where(x => !string.IsNullOrWhiteSpace(x.OptionText))
                    .Select((x, idx) =>
                    {
                        x.OptionText = x.OptionText.Trim();
                        x.DisplayOrder = idx + 1;
                        return x;
                    })
                    .ToList();

                question.GridRows = (question.GridRows ?? new List<SurveyGridTextItemViewModel>())
                    .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                    .Select((x, idx) =>
                    {
                        x.Text = x.Text.Trim();
                        x.DisplayOrder = idx + 1;
                        return x;
                    })
                    .ToList();

                question.GridColumns = (question.GridColumns ?? new List<SurveyGridTextItemViewModel>())
                    .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                    .Select((x, idx) =>
                    {
                        x.Text = x.Text.Trim();
                        x.DisplayOrder = idx + 1;
                        return x;
                    })
                    .ToList();
            }

            return model;
        }

        public void Create(SurveyFormBuilderViewModel model, string userName)
        {
            ValidateTemplate(model.SurveyTemplateId);
            var preparedModel = PrepareAndValidateModel(model);

            var entity = new SurveyForm
            {
                SurveyTemplateId = preparedModel.SurveyTemplateId,
                Title = preparedModel.Title.Trim(),
                Category = preparedModel.Category.Trim(),
                Description = string.IsNullOrWhiteSpace(preparedModel.Description) ? null : preparedModel.Description.Trim(),
                CreatedBy = string.IsNullOrWhiteSpace(userName) ? "System" : userName,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Questions = BuildQuestions(preparedModel)
            };

            _surveyFormRepository.AddForm(entity);
            _surveyFormRepository.SaveChanges();
        }

        public void Update(SurveyFormBuilderViewModel model, string userName)
        {
            var existing = _surveyFormRepository.GetFormById(model.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Survey form not found.");
            }

            ValidateTemplate(model.SurveyTemplateId);
            var preparedModel = PrepareAndValidateModel(model);

            existing.SurveyTemplateId = preparedModel.SurveyTemplateId;
            existing.Title = preparedModel.Title.Trim();
            existing.Category = preparedModel.Category.Trim();
            existing.Description = string.IsNullOrWhiteSpace(preparedModel.Description) ? null : preparedModel.Description.Trim();
            existing.ModifiedBy = string.IsNullOrWhiteSpace(userName) ? "System" : userName;
            existing.ModifiedDate = DateTime.Now;
            existing.IsActive = true;

            var existingQuestions = existing.Questions.ToList();
            foreach (var question in existingQuestions)
            {
                _surveyFormRepository.RemoveQuestion(question);
            }

            foreach (var question in BuildQuestions(preparedModel))
            {
                existing.Questions.Add(question);
            }

            _surveyFormRepository.SaveChanges();
        }

        private static SurveyQuestionBuilderViewModel MapQuestionToViewModel(SurveyQuestion question)
        {
            return new SurveyQuestionBuilderViewModel
            {
                Id = question.Id,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType,
                DisplayOrder = question.DisplayOrder,
                IsRequired = question.IsRequired,
                MinValue = question.MinValue,
                MaxValue = question.MaxValue,
                MinLabel = question.MinLabel,
                MaxLabel = question.MaxLabel,
                Options = question.Options
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new SurveyQuestionOptionViewModel
                    {
                        Id = x.Id,
                        OptionText = x.OptionText,
                        DisplayOrder = x.DisplayOrder
                    }).ToList(),
                GridRows = question.GridRows
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new SurveyGridTextItemViewModel
                    {
                        Id = x.Id,
                        Text = x.RowText,
                        DisplayOrder = x.DisplayOrder
                    }).ToList(),
                GridColumns = question.GridColumns
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x => new SurveyGridTextItemViewModel
                    {
                        Id = x.Id,
                        Text = x.ColumnText,
                        DisplayOrder = x.DisplayOrder
                    }).ToList()
            };
        }

        private static List<SurveyQuestion> BuildQuestions(SurveyFormBuilderViewModel model)
        {
            return model.Questions.Select((question, questionIndex) =>
            {
                var entity = new SurveyQuestion
                {
                    QuestionText = question.QuestionText.Trim(),
                    QuestionType = question.QuestionType,
                    DisplayOrder = questionIndex + 1,
                    IsRequired = question.IsRequired,
                    CreatedDate = DateTime.Now,
                    MinValue = question.QuestionType == SurveyQuestionTypeCatalog.LinearScale ? question.MinValue : null,
                    MaxValue = question.QuestionType == SurveyQuestionTypeCatalog.LinearScale ? question.MaxValue : null,
                    MinLabel = question.QuestionType == SurveyQuestionTypeCatalog.LinearScale ? TrimOrNull(question.MinLabel) : null,
                    MaxLabel = question.QuestionType == SurveyQuestionTypeCatalog.LinearScale ? TrimOrNull(question.MaxLabel) : null,
                    Options = new List<SurveyQuestionOption>(),
                    GridRows = new List<SurveyGridRow>(),
                    GridColumns = new List<SurveyGridColumn>()
                };

                if (IsOptionQuestion(question.QuestionType))
                {
                    entity.Options = question.Options.Select((option, optionIndex) => new SurveyQuestionOption
                    {
                        OptionText = option.OptionText.Trim(),
                        DisplayOrder = optionIndex + 1
                    }).ToList();
                }

                if (IsGridQuestion(question.QuestionType))
                {
                    entity.GridRows = question.GridRows.Select((row, rowIndex) => new SurveyGridRow
                    {
                        RowText = row.Text.Trim(),
                        DisplayOrder = rowIndex + 1
                    }).ToList();

                    entity.GridColumns = question.GridColumns.Select((column, columnIndex) => new SurveyGridColumn
                    {
                        ColumnText = column.Text.Trim(),
                        DisplayOrder = columnIndex + 1
                    }).ToList();
                }

                return entity;
            }).ToList();
        }

        private static bool IsOptionQuestion(string questionType)
        {
            return questionType == SurveyQuestionTypeCatalog.MultipleChoice
                || questionType == SurveyQuestionTypeCatalog.Checkboxes
                || questionType == SurveyQuestionTypeCatalog.Dropdown;
        }

        private static bool IsGridQuestion(string questionType)
        {
            return questionType == SurveyQuestionTypeCatalog.MultipleChoiceGrid
                || questionType == SurveyQuestionTypeCatalog.CheckboxGrid;
        }

        private static string TrimOrNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private void ValidateTemplate(int surveyTemplateId)
        {
            var template = _surveyFormRepository.GetTemplateById(surveyTemplateId);
            if (template == null)
            {
                throw new InvalidOperationException("Selected survey template does not exist.");
            }
        }

        private static SurveyFormBuilderViewModel PrepareAndValidateModel(SurveyFormBuilderViewModel model)
        {
            model.Title = model.Title?.Trim();
            model.Category = model.Category?.Trim();
            model.Description = TrimOrNull(model.Description);

            model.Questions = (model.Questions ?? new List<SurveyQuestionBuilderViewModel>())
                .Where(x => !string.IsNullOrWhiteSpace(x.QuestionText) && !string.IsNullOrWhiteSpace(x.QuestionType))
                .Select((x, index) =>
                {
                    x.QuestionText = x.QuestionText.Trim();
                    x.QuestionType = x.QuestionType.Trim();
                    x.DisplayOrder = index + 1;

                    x.Options = (x.Options ?? new List<SurveyQuestionOptionViewModel>())
                        .Where(o => !string.IsNullOrWhiteSpace(o.OptionText))
                        .Select((o, optionIndex) => new SurveyQuestionOptionViewModel
                        {
                            Id = o.Id,
                            OptionText = o.OptionText.Trim(),
                            DisplayOrder = optionIndex + 1
                        }).ToList();

                    x.GridRows = (x.GridRows ?? new List<SurveyGridTextItemViewModel>())
                        .Where(r => !string.IsNullOrWhiteSpace(r.Text))
                        .Select((r, rowIndex) => new SurveyGridTextItemViewModel
                        {
                            Id = r.Id,
                            Text = r.Text.Trim(),
                            DisplayOrder = rowIndex + 1
                        }).ToList();

                    x.GridColumns = (x.GridColumns ?? new List<SurveyGridTextItemViewModel>())
                        .Where(c => !string.IsNullOrWhiteSpace(c.Text))
                        .Select((c, columnIndex) => new SurveyGridTextItemViewModel
                        {
                            Id = c.Id,
                            Text = c.Text.Trim(),
                            DisplayOrder = columnIndex + 1
                        }).ToList();

                    return x;
                })
                .ToList();

            if (!model.Questions.Any())
            {
                throw new InvalidOperationException("At least one question is required.");
            }

            foreach (var question in model.Questions)
            {
                if (IsOptionQuestion(question.QuestionType) && !question.Options.Any())
                {
                    throw new InvalidOperationException("Option-based questions require at least one option.");
                }

                if (question.QuestionType == SurveyQuestionTypeCatalog.LinearScale)
                {
                    if (!question.MinValue.HasValue || !question.MaxValue.HasValue)
                    {
                        throw new InvalidOperationException("Linear scale questions require both min and max values.");
                    }

                    if (question.MinValue.Value >= question.MaxValue.Value)
                    {
                        throw new InvalidOperationException("Linear scale min value must be less than max value.");
                    }
                }

                if (IsGridQuestion(question.QuestionType) && (!question.GridRows.Any() || !question.GridColumns.Any()))
                {
                    throw new InvalidOperationException("Grid questions require at least one row and one column.");
                }
            }

            return model;
        }
    }
}
