using System;
using System.Collections.Generic;
using System.Linq;
using CallCenterSecure.Models;

namespace CallCenterSecure.Services
{
    public class SurveyTemplateService : ISurveyTemplateService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public IEnumerable<SurveyTemplateType> GetAll()
        {
            return db.SurveyTemplateTypes.OrderBy(x => x.Id);
        }

        public SurveyTemplateType GetById(int id)
        {
            return db.SurveyTemplateTypes.FirstOrDefault(x => x.Id == id);
        }

        public void Update(SurveyTemplateType template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var existing = GetById(template.Id);
            if (existing == null)
            {
                throw new InvalidOperationException("Survey template not found.");
            }

            existing.Name = template.Name;
            db.SaveChanges();
        }

        public void Create(SurveyTemplateType template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            if (!CanCreate())
            {
                throw new InvalidOperationException("Cannot create more than 3 survey templates. Please delete one first.");
            }

            db.SurveyTemplateTypes.Add(template);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            var template = GetById(id);
            if (template == null)
            {
                throw new InvalidOperationException("Survey template not found.");
            }

            db.SurveyTemplateTypes.Remove(template);
            db.SaveChanges();
        }

        public bool CanCreate()
        {
            return db.SurveyTemplateTypes.Count() < 3;
        }
    }
}
