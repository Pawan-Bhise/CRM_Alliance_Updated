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
        byte[] ExportExcel(int formId);
        int Duplicate(int formId, string userName);
        void ToggleStatus(int formId, string userName);
        void Delete(int formId, string userName);
    }
}
