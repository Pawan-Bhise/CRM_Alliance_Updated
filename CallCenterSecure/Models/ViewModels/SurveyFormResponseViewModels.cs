using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CallCenterSecure.Models.ViewModels
{
    public class SurveyFormResponseListViewModel
    {
        public SurveyFormResponseListViewModel()
        {
            Templates = new List<SurveyTemplateLookupViewModel>();
            Forms = new List<SurveyFormLookupViewModel>();
            Customers = new List<SurveyCustomerLookupViewModel>();
        }

        public int? SelectedTemplateId { get; set; }
        public int? SelectedFormId { get; set; }
        public int? SelectedCustomerId { get; set; }

        public List<SurveyTemplateLookupViewModel> Templates { get; set; }
        public List<SurveyFormLookupViewModel> Forms { get; set; }
        public List<SurveyCustomerLookupViewModel> Customers { get; set; }
    }

    public class SurveyFormResponseSubmitViewModel
    {
        public SurveyFormResponseSubmitViewModel()
        {
            Questions = new List<SurveyQuestionResponseViewModel>();
        }

        public int SurveyFormId { get; set; }
        public int SurveyTemplateId { get; set; }
        public int? SurveyCustomerDataId { get; set; }

        public string SurveyTemplateName { get; set; }
        public string SurveyFormTitle { get; set; }
        public string SurveyFormCategory { get; set; }
        public string SurveyFormDescription { get; set; }

        [Display(Name = "Respondent Name")]
        [MaxLength(100)]
        public string RespondentName { get; set; }

        [Display(Name = "Respondent Mobile")]
        [MaxLength(50)]
        public string RespondentMobile { get; set; }

        public string SelectedCustomerName { get; set; }
        public string SelectedCustomerCode { get; set; }

        public List<SurveyQuestionResponseViewModel> Questions { get; set; }
    }

    public class SurveyQuestionResponseViewModel
    {
        public SurveyQuestionResponseViewModel()
        {
            Options = new List<SurveyOptionLookupViewModel>();
            GridRows = new List<string>();
            GridColumns = new List<string>();
            SelectedOptions = new List<string>();
            GridAnswers = new List<SurveyGridAnswerInputViewModel>();
        }

        public int SurveyQuestionId { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsRequired { get; set; }

        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        public string MinLabel { get; set; }
        public string MaxLabel { get; set; }

        public string AnswerText { get; set; }
        public string SelectedOption { get; set; }
        public List<string> SelectedOptions { get; set; }
        public HttpPostedFileBase File { get; set; }

        public List<SurveyOptionLookupViewModel> Options { get; set; }
        public List<string> GridRows { get; set; }
        public List<string> GridColumns { get; set; }
        public List<SurveyGridAnswerInputViewModel> GridAnswers { get; set; }
    }

    public class SurveyGridAnswerInputViewModel
    {
        public string RowText { get; set; }
        public string SelectedColumnText { get; set; }
        public List<string> SelectedColumnTexts { get; set; }
    }

    public class SurveyTemplateLookupViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SurveyFormLookupViewModel
    {
        public int Id { get; set; }
        public int SurveyTemplateId { get; set; }
        public string Title { get; set; }
    }

    public class SurveyCustomerLookupViewModel
    {
        public int Id { get; set; }
        public int? SurveyTemplateTypeId { get; set; }
        public string ClientName { get; set; }
        public string CustomerCode { get; set; }
    }

    public class SurveyOptionLookupViewModel
    {
        public string OptionText { get; set; }
    }
}
