using System.Collections.Generic;
using CallCenterSecure.Models;

namespace CallCenterSecure.Services
{
    public interface ISurveyTemplateService
    {
        IEnumerable<SurveyTemplateType> GetAll();
        SurveyTemplateType GetById(int id);
        void Update(SurveyTemplateType template);
        void Create(SurveyTemplateType template);
        void Delete(int id);
        bool CanCreate();
    }
}
