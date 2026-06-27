using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CallCenterSecure.Models;

namespace CallCenterSecure.Repositories
{
    public class SurveyFormRepository : ISurveyFormRepository
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public IEnumerable<SurveyTemplateType> GetAllTemplates()
        {
            return _db.SurveyTemplateTypes.OrderBy(x => x.Id).ToList();
        }

        public SurveyTemplateType GetTemplateById(int id)
        {
            return _db.SurveyTemplateTypes.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<SurveyForm> GetFormsByTemplateId(int surveyTemplateId)
        {
            return _db.SurveyForms
                .Include(x => x.SurveyTemplate)
                .Include(x => x.Questions)
                .Where(x => x.SurveyTemplateId == surveyTemplateId)
                .OrderBy(x => x.Id)
                .ToList();
        }

        public SurveyForm GetFormById(int id)
        {
            return _db.SurveyForms
                .Include(x => x.SurveyTemplate)
                .Include(x => x.Questions.Select(q => q.Options))
                .Include(x => x.Questions.Select(q => q.GridRows))
                .Include(x => x.Questions.Select(q => q.GridColumns))
                .FirstOrDefault(x => x.Id == id);
        }

        public void AddForm(SurveyForm form)
        {
            _db.SurveyForms.Add(form);
        }

        public void RemoveQuestion(SurveyQuestion question)
        {
            _db.SurveyQuestions.Remove(question);
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }
    }
}
