using System;
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

        public ActionResult Index(int? templateId, int? formId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            var model = _surveyReportService.GetReport(templateId, formId, categoryId, fromDate, toDate);
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

        public ActionResult ExportCsv(int? formId, int? categoryId, DateTime? fromDate, DateTime? toDate)
        {
            if (!formId.HasValue)
            {
                return new HttpStatusCodeResult(400, "A survey form is required.");
            }

            var bytes = _surveyReportService.ExportCsv(formId, categoryId, fromDate, toDate);
            return File(bytes, "text/csv", string.Format("survey-responses-{0}.csv", formId.Value));
        }
    }
}
