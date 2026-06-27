using System.Collections.Generic;
using CallCenterSecure.Models;

namespace CallCenterSecure.Repositories
{
    public interface ISurveyFormRepository
    {
        IEnumerable<SurveyTemplateType> GetAllTemplates();
        SurveyTemplateType GetTemplateById(int id);
        IEnumerable<SurveyForm> GetFormsByTemplateId(int surveyTemplateId);
        SurveyForm GetFormById(int id);
        void AddForm(SurveyForm form);
        void RemoveQuestion(SurveyQuestion question);
        void SaveChanges();
    }
}
