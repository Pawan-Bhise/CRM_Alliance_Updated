using System.Collections.Generic;
using CallCenterSecure.Models;

namespace CallCenterSecure.Repositories
{
    public interface ISurveyResponseRepository
    {
        IEnumerable<SurveyTemplateType> GetTemplates();
        IEnumerable<SurveyForm> GetFormsByTemplateId(int templateId);
        IEnumerable<SurveyCustomerData> GetCustomersByTemplateId(int templateId);
        IEnumerable<SurveyCustomerData> GetCustomersByPhone(string phoneNumber);
        SurveyForm GetFormWithQuestions(int formId);
        SurveyCustomerData GetCustomerById(int customerId);
        SurveyCustomerFormTracking GetCustomerFormTracking(int customerId, int templateId, int formId);
        IEnumerable<SurveyCustomerFormTracking> GetCustomerFormTrackings(int templateId, int formId);
        SurveyCallStatusMaster GetCallStatus(int id);
        SurveyFormStatusMaster GetFormStatus(int id);
        IEnumerable<SurveyCallStatusMaster> GetCallStatuses();
        IEnumerable<SurveyFormStatusMaster> GetFormStatuses();
        void AddCustomerFormTracking(SurveyCustomerFormTracking tracking);
        void AddResponse(SurveyFormResponse response);
        void SaveChanges();
    }
}
