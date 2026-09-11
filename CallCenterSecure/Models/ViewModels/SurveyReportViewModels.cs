using System;
using System.Collections.Generic;

namespace CallCenterSecure.Models.ViewModels
{
    public class SurveyReportViewModel
    {
        public SurveyReportViewModel()
        {
            Templates = new List<SurveyTemplateLookupViewModel>();
            Forms = new List<SurveyFormLookupViewModel>();
            Responses = new List<SurveyResponseSummaryViewModel>();
            Questions = new List<SurveyQuestionReportViewModel>();
        }

        public int? SelectedTemplateId { get; set; }
        public int? SelectedFormId { get; set; }
        public int? SelectedCategoryId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SelectedFormTitle { get; set; }
        public int TotalResponses { get; set; }
        public List<SurveyTemplateLookupViewModel> Templates { get; set; }
        public List<SurveyFormLookupViewModel> Forms { get; set; }
        public List<SurveyResponseSummaryViewModel> Responses { get; set; }
        public List<SurveyQuestionReportViewModel> Questions { get; set; }
    }

    public class SurveyQuestionReportViewModel
    {
        public SurveyQuestionReportViewModel()
        {
            Options = new List<SurveyReportOptionViewModel>();
            TextAnswers = new List<string>();
            GridRows = new List<SurveyReportGridRowViewModel>();
        }

        public int QuestionId { get; set; }
        public int DisplayOrder { get; set; }
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public int AnsweredCount { get; set; }
        public int UnansweredCount { get; set; }
        public double? Average { get; set; }
        public double? Minimum { get; set; }
        public double? Maximum { get; set; }
        public double? NpsScore { get; set; }
        public int Promoters { get; set; }
        public int Passives { get; set; }
        public int Detractors { get; set; }
        public List<SurveyReportOptionViewModel> Options { get; set; }
        public List<string> TextAnswers { get; set; }
        public List<SurveyReportGridRowViewModel> GridRows { get; set; }
    }

    public class SurveyReportOptionViewModel
    {
        public string Label { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class SurveyReportGridRowViewModel
    {
        public SurveyReportGridRowViewModel()
        {
            Options = new List<SurveyReportOptionViewModel>();
        }

        public string RowText { get; set; }
        public List<SurveyReportOptionViewModel> Options { get; set; }
    }

    public class SurveyResponseSummaryViewModel
    {
        public int Id { get; set; }
        public string RespondentName { get; set; }
        public string RespondentMobile { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedDate { get; set; }
    }

    public class SurveyResponseDetailViewModel
    {
        public SurveyResponseDetailViewModel()
        {
            Answers = new List<SurveyResponseAnswerViewModel>();
        }

        public int Id { get; set; }
        public string FormTitle { get; set; }
        public string TemplateName { get; set; }
        public string RespondentName { get; set; }
        public string RespondentMobile { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime SubmittedDate { get; set; }
        public List<SurveyResponseAnswerViewModel> Answers { get; set; }
    }

    public class SurveyResponseAnswerViewModel
    {
        public string QuestionText { get; set; }
        public string QuestionType { get; set; }
        public string Answer { get; set; }
    }
}
