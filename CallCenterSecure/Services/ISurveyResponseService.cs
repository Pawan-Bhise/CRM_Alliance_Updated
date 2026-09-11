using System.Web;
using CallCenterSecure.Models.ViewModels;

namespace CallCenterSecure.Services
{
    public interface ISurveyResponseService
    {
        SurveyFormResponseListViewModel GetStartModel(int? templateId, int? formId, int? customerId, int? categoryId, string phoneNumber = null);
        SurveyFormResponseSubmitViewModel BuildSubmitModel(int formId, int? customerId, int? categoryId);
        SurveyFormResponseSubmitViewModel RehydrateSubmitModel(SurveyFormResponseSubmitViewModel model);
        void Submit(SurveyFormResponseSubmitViewModel model, string submittedBy, HttpServerUtilityBase server);
    }
}
