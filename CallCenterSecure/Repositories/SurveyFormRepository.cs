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
            // Remove any answers that reference this question first to avoid FK constraint errors
            var answers = _db.SurveyFormAnswers
                .Where(a => a.SurveyQuestionId == question.Id)
                .ToList();

            foreach (var ans in answers)
            {
                // Remove any grid answers tied to the answer
                var gridAnswers = _db.SurveyFormGridAnswers.Where(g => g.SurveyFormAnswerId == ans.Id).ToList();
                if (gridAnswers.Any())
                {
                    _db.SurveyFormGridAnswers.RemoveRange(gridAnswers);
                }

                _db.SurveyFormAnswers.Remove(ans);
            }

            // Remove question child collections (options / grid rows / grid columns) to keep the model consistent
            if (question.Options != null && question.Options.Any())
            {
                _db.SurveyQuestionOptions.RemoveRange(question.Options);
            }

            if (question.GridRows != null && question.GridRows.Any())
            {
                _db.SurveyGridRows.RemoveRange(question.GridRows);
            }

            if (question.GridColumns != null && question.GridColumns.Any())
            {
                _db.SurveyGridColumns.RemoveRange(question.GridColumns);
            }

            _db.SurveyQuestions.Remove(question);
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }
    }
}
