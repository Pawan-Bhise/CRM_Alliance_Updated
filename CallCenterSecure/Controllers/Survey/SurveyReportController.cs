using System;
using System.Linq;
using System.Web.Mvc;
using CallCenterSecure.Services;

namespace CallCenterSecure.Controllers.Survey
{
    public class SurveyReportController : Controller
    {
        private readonly SurveyReportService _surveyReportService;

        public SurveyReportController()
            : this(new SurveyReportService())
        {
        }

        internal SurveyReportController(SurveyReportService surveyReportService)
        {
            _surveyReportService = surveyReportService;
        }

        public ActionResult Index(int? templateId, int? formId, int? categoryId, DateTime? fromDate, DateTime? toDate, string reportTab, int page = 1)
        {
            var invalidDateFilter = HasDateBindingError();
            var model = _surveyReportService.GetReport(templateId, formId, categoryId, fromDate, toDate, reportTab, page, invalidDateFilter);
            return View(model);
        }

        public ActionResult Details(int id)
        {
            var model = _surveyReportService.GetResponseDetail(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        public ActionResult ExportCsv(int? templateId, int? formId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            if (HasDateBindingError())
            {
                return new HttpStatusCodeResult(400, "Enter valid From and To dates.");
            }

            if (!templateId.HasValue || !formId.HasValue)
            {
                return new HttpStatusCodeResult(400, "Select both a survey template and a survey form before exporting.");
            }

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            {
                return new HttpStatusCodeResult(400, "The From date must be on or before the To date.");
            }

            byte[] bytes;
            try
            {
                bytes = _surveyReportService.ExportCsv(templateId.Value, formId.Value, categoryId, fromDate, toDate);
            }
            catch (ArgumentException ex)
            {
                return new HttpStatusCodeResult(400, ex.Message);
            }

            var fileScope = formId.Value.ToString();
            return File(bytes, "text/csv", string.Format("survey-responses-{0}.csv", fileScope));
        }

        private bool HasDateBindingError()
        {
            return (ModelState.ContainsKey("fromDate") && ModelState["fromDate"].Errors.Any())
                || (ModelState.ContainsKey("toDate") && ModelState["toDate"].Errors.Any());
        }
    }
}
