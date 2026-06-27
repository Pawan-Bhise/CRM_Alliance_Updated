using CallCenterSecure.Models.ViewModels;

namespace CallCenterSecure.Services
{
    public interface ISurveyFormService
    {
        SurveyFormIndexViewModel GetIndexModel(int? surveyTemplateId);
        SurveyFormBuilderViewModel GetCreateModel(int? surveyTemplateId);
        SurveyFormBuilderViewModel GetEditModel(int id);
        SurveyFormBuilderViewModel PrepareBuilderModel(SurveyFormBuilderViewModel model);
        void Create(SurveyFormBuilderViewModel model, string userName);
        void Update(SurveyFormBuilderViewModel model, string userName);
    }
}
