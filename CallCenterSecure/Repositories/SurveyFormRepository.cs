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
                .OrderByDescending(x => x.CreatedDate)
                .ThenByDescending(x => x.Id)
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

        public void RemoveForm(SurveyForm form)
        {
            var trackingRows = _db.SurveyCustomerFormTrackings
                .Where(x => x.SurveyFormId == form.Id)
                .ToList();

            if (trackingRows.Any())
            {
                _db.SurveyCustomerFormTrackings.RemoveRange(trackingRows);
            }

            _db.SurveyForms.Remove(form);
        }

        public void RemoveQuestion(SurveyQuestion question)
        {
            question.IsDeleted = true;
        }

        public void UpdateQuestion(SurveyQuestion existing, SurveyQuestion updated)
        {
            existing.QuestionText = updated.QuestionText;
            existing.QuestionType = updated.QuestionType;
            existing.DisplayOrder = updated.DisplayOrder;
            existing.IsRequired = updated.IsRequired;
            existing.ConditionalParentQuestionIndex = updated.ConditionalParentQuestionIndex;
            existing.ConditionalParentOptionText = updated.ConditionalParentOptionText;
            existing.MinValue = updated.MinValue;
            existing.MaxValue = updated.MaxValue;
            existing.MinLabel = updated.MinLabel;
            existing.MaxLabel = updated.MaxLabel;

            _db.SurveyQuestionOptions.RemoveRange(existing.Options.ToList());
            _db.SurveyGridRows.RemoveRange(existing.GridRows.ToList());
            _db.SurveyGridColumns.RemoveRange(existing.GridColumns.ToList());

            foreach (var option in updated.Options)
            {
                option.SurveyQuestionId = existing.Id;
                _db.SurveyQuestionOptions.Add(option);
            }

            foreach (var row in updated.GridRows)
            {
                row.SurveyQuestionId = existing.Id;
                _db.SurveyGridRows.Add(row);
            }

            foreach (var column in updated.GridColumns)
            {
                column.SurveyQuestionId = existing.Id;
                _db.SurveyGridColumns.Add(column);
            }
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }
    }
}
