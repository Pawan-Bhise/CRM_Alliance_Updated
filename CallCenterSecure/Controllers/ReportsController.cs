using System;
using System.Web.Mvc;
using CallCenter.CustomAuthentication;
using CallCenterSecure.Services;

namespace CallCenterSecure.Controllers
{
    [CustomAuthorize(Roles = "Admin|Supervisor|Agent")]
    public class ReportsController : Controller
    {
        private readonly ReportingService _reportingService = new ReportingService();

        public ActionResult Index()
        {
            return RedirectToAction("Complaint");
        }

        public ActionResult Complaint(DateTime? fromDate, DateTime? toDate)
        {
            var model = _reportingService.GetComplaintReport(fromDate, toDate);
            return View(model);
        }

        public ActionResult Enquiry(DateTime? fromDate, DateTime? toDate)
        {
            var model = _reportingService.GetEnquiryReport(fromDate, toDate);
            return View(model);
        }
    }
}