using System.Collections.Generic;
using CallCenterSecure.Models;

namespace CallCenterSecure.Repositories
{
    public interface ISurveyResponseRepository
    {
        IEnumerable<SurveyTemplateType> GetTemplates();
        IEnumerable<SurveyForm> GetFormsByTemplateId(int templateId);
        IEnumerable<SurveyCustomerData> GetCustomersByTemplateId(int templateId);
        SurveyForm GetFormWithQuestions(int formId);
        SurveyCustomerData GetCustomerById(int customerId);
        void AddResponse(SurveyFormResponse response);
        void SaveChanges();
    }
}
