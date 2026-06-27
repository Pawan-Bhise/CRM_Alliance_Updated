using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Web.Mvc;

namespace CallCenterSecure.Models.ViewModels
{
    public class SurveyFormIndexViewModel
    {
        public SurveyFormIndexViewModel()
        {
            SurveyTemplates = new List<SelectListItem>();
            Forms = new List<SurveyFormListItemViewModel>();
        }

        public int? SelectedSurveyTemplateId { get; set; }

        public List<SelectListItem> SurveyTemplates { get; set; }

        public List<SurveyFormListItemViewModel> Forms { get; set; }
    }

    public class SurveyFormListItemViewModel
    {
        public int Id { get; set; }

        public int SurveyTemplateId { get; set; }

        public string SurveyTemplateName { get; set; }

        public string Title { get; set; }

        public string Category { get; set; }

        public int QuestionCount { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsActive { get; set; }
    }

    public class SurveyFormBuilderViewModel
    {
        public SurveyFormBuilderViewModel()
        {
            Questions = new List<SurveyQuestionBuilderViewModel>();
            SurveyTemplates = new List<SelectListItem>();
            AvailableQuestionTypes = SurveyQuestionTypeCatalog.GetSelectList();
        }

        public int Id { get; set; }

        [Required]
        [Display(Name = "Survey Template")]
        public int SurveyTemplateId { get; set; }

        [Required]
        [MaxLength(200)]
        [Display(Name = "Form Title")]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public List<SurveyQuestionBuilderViewModel> Questions { get; set; }

        public List<SelectListItem> SurveyTemplates { get; set; }

        public List<SelectListItem> AvailableQuestionTypes { get; set; }
    }

    public class SurveyQuestionBuilderViewModel
    {
        public SurveyQuestionBuilderViewModel()
        {
            Options = new List<SurveyQuestionOptionViewModel>();
            GridRows = new List<SurveyGridTextItemViewModel>();
            GridColumns = new List<SurveyGridTextItemViewModel>();
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(1000)]
        [Display(Name = "Question Text")]
        public string QuestionText { get; set; }

        [Required]
        [MaxLength(50)]
        public string QuestionType { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsRequired { get; set; }

        public int? MinValue { get; set; }

        public int? MaxValue { get; set; }

        [MaxLength(100)]
        public string MinLabel { get; set; }

        [MaxLength(100)]
        public string MaxLabel { get; set; }

        public List<SurveyQuestionOptionViewModel> Options { get; set; }

        public List<SurveyGridTextItemViewModel> GridRows { get; set; }

        public List<SurveyGridTextItemViewModel> GridColumns { get; set; }
    }

    public class SurveyQuestionOptionViewModel
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string OptionText { get; set; }

        public int DisplayOrder { get; set; }
    }

    public class SurveyGridTextItemViewModel
    {
        public int Id { get; set; }

        [MaxLength(500)]
        public string Text { get; set; }

        public int DisplayOrder { get; set; }
    }

    public static class SurveyQuestionTypeCatalog
    {
        public const string ShortAnswer = "Short Answer";
        public const string Paragraph = "Paragraph";
        public const string MultipleChoice = "Multiple Choice";
        public const string Checkboxes = "Checkboxes";
        public const string Dropdown = "Dropdown";
        public const string FileUpload = "File Upload";
        public const string LinearScale = "Linear Scale";
        public const string MultipleChoiceGrid = "Multiple Choice Grid";
        public const string CheckboxGrid = "Checkbox Grid";

        public static List<SelectListItem> GetSelectList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Text = ShortAnswer, Value = ShortAnswer },
                new SelectListItem { Text = Paragraph, Value = Paragraph },
                new SelectListItem { Text = MultipleChoice, Value = MultipleChoice },
                new SelectListItem { Text = Checkboxes, Value = Checkboxes },
                new SelectListItem { Text = Dropdown, Value = Dropdown },
                new SelectListItem { Text = FileUpload, Value = FileUpload },
                new SelectListItem { Text = LinearScale, Value = LinearScale },
                new SelectListItem { Text = MultipleChoiceGrid, Value = MultipleChoiceGrid },
                new SelectListItem { Text = CheckboxGrid, Value = CheckboxGrid }
            };
        }
    }
}
