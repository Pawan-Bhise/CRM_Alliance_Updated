using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CallCenter.CustomAuthentication;
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

        public ActionResult Details(int id, int? returnTemplateId, int? returnFormId, int? returnCustomerId)
        {
            var model = _surveyReportService.GetResponseDetail(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            model.ReturnTemplateId = returnTemplateId;
            model.ReturnFormId = returnFormId;
            model.ReturnCustomerId = returnCustomerId;
            return View(model);
        }

        [CustomAuthorize(Roles = "Admin|Supervisor|Agent")]
        public ActionResult DownloadAnswerFile(int answerId)
        {
            if (answerId <= 0)
            {
                return new HttpStatusCodeResult(400);
            }

            var storedFileName = _surveyReportService.GetResponseFileName(answerId);
            if (string.IsNullOrWhiteSpace(storedFileName))
            {
                return HttpNotFound();
            }

            var safeFileName = Path.GetFileName(storedFileName);
            if (!string.Equals(safeFileName, storedFileName, StringComparison.Ordinal)
                || !new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".jpg", ".jpeg", ".png" }
                    .Contains(Path.GetExtension(safeFileName), StringComparer.OrdinalIgnoreCase))
            {
                return HttpNotFound();
            }

            var uploadDirectory = Server.MapPath("~/Uploads/SurveyResponses");
            var fullPath = Path.GetFullPath(Path.Combine(uploadDirectory, safeFileName));
            var uploadDirectoryWithSeparator = Path.GetFullPath(uploadDirectory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!fullPath.StartsWith(uploadDirectoryWithSeparator, StringComparison.OrdinalIgnoreCase) || !System.IO.File.Exists(fullPath))
            {
                return HttpNotFound();
            }

            return File(fullPath, MimeMapping.GetMimeMapping(safeFileName), safeFileName);
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
