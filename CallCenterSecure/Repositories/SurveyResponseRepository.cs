using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using CallCenterSecure.Models;

namespace CallCenterSecure.Repositories
{
    public class SurveyResponseRepository : ISurveyResponseRepository
    {
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

        public IEnumerable<SurveyTemplateType> GetTemplates()
        {
            return _db.SurveyTemplateTypes.OrderBy(x => x.Id).ToList();
        }

        public IEnumerable<SurveyForm> GetFormsByTemplateId(int templateId)
        {
            return _db.SurveyForms
                .Where(x => x.SurveyTemplateId == templateId && x.IsActive)
                .OrderBy(x => x.Title)
                .ToList();
        }

        public IEnumerable<SurveyCustomerData> GetCustomersByTemplateId(int templateId)
        {
            return _db.SurveyCustomerData
                .Where(x => x.SurveyTemplateTypeId == templateId)
                .OrderBy(x => x.ClientName)
                .ToList();
        }

        public IEnumerable<SurveyCustomerData> GetCustomersByPhone(string phoneNumber)
        {
            var digits = new string((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (string.IsNullOrWhiteSpace(digits))
            {
                return new List<SurveyCustomerData>();
            }

            return _db.SurveyCustomerData
                .ToList()
                .Where(x => NormalizePhone(x.MobileNumber1) == digits || NormalizePhone(x.MobileNumber2) == digits)
                .OrderBy(x => x.Id)
                .ToList();
        }

        private static string NormalizePhone(string phoneNumber)
        {
            return new string((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        }

        public SurveyForm GetFormWithQuestions(int formId)
        {
            return _db.SurveyForms
                .Include(x => x.SurveyTemplate)
                .Include(x => x.Questions.Select(q => q.Options))
                .Include(x => x.Questions.Select(q => q.GridRows))
                .Include(x => x.Questions.Select(q => q.GridColumns))
                .FirstOrDefault(x => x.Id == formId && x.IsActive);
        }

        public SurveyCustomerData GetCustomerById(int customerId)
        {
            return _db.SurveyCustomerData.FirstOrDefault(x => x.Id == customerId);
        }

        public void AddResponse(SurveyFormResponse response)
        {
            _db.SurveyFormResponses.Add(response);
        }

        public void SaveChanges()
        {
            _db.SaveChanges();
        }
    }
}
