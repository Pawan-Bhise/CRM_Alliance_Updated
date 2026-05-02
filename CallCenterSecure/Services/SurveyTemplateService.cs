using System;
using System.Collections.Generic;
using System.Linq;
using CallCenterSecure.Models;

namespace CallCenterSecure.Services
{
    public class SurveyTemplateService : ISurveyTemplateService
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private static readonly List<SurveyTemplateType> Templates = new List<SurveyTemplateType>
        {
            new SurveyTemplateType { Id = 1, Name = "Exit Client Survey" },
            new SurveyTemplateType { Id = 2, Name = "Satisfaction Survey" },
            new SurveyTemplateType { Id = 3, Name = "Competitor Survey" }
        };

        public IEnumerable<SurveyTemplateType> GetAll()
        {
            return db.SurveyTemplateTypes.OrderBy(x => x.Id); //Templates.OrderBy(t => t.Id).ToList();
        }

        public SurveyTemplateType GetById(int id)
        {
            return db.SurveyTemplateTypes.FirstOrDefault(x=>x.Id==id);
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
    }
}
