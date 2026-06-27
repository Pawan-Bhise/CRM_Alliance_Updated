using System.Web;
using CallCenterSecure.Models.ViewModels;

namespace CallCenterSecure.Services
{
    public interface ISurveyResponseService
    {
        SurveyFormResponseListViewModel GetStartModel(int? templateId, int? formId, int? customerId);
        SurveyFormResponseSubmitViewModel BuildSubmitModel(int formId, int? customerId);
        SurveyFormResponseSubmitViewModel RehydrateSubmitModel(SurveyFormResponseSubmitViewModel model);
        void Submit(SurveyFormResponseSubmitViewModel model, string submittedBy, HttpServerUtilityBase server);
    }
}
