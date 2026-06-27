using System;
using System.Linq;
using System.Web.Mvc;
using CallCenterSecure.Services;

namespace CallCenterSecure.Controllers.Survey
{
    public class SurveyResponseController : Controller
    {
        private readonly ISurveyResponseService _surveyResponseService;

        public SurveyResponseController()
            : this(new SurveyResponseService())
        {
        }

        internal SurveyResponseController(ISurveyResponseService surveyResponseService)
        {
            _surveyResponseService = surveyResponseService;
        }

        public ActionResult Index(int? templateId, int? formId, int? customerId)
        {
            var model = _surveyResponseService.GetStartModel(templateId, formId, customerId);
            return View(model);
        }

        public ActionResult Fill(int formId, int? customerId)
        {
            var model = _surveyResponseService.BuildSubmitModel(formId, customerId);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Fill(CallCenterSecure.Models.ViewModels.SurveyFormResponseSubmitViewModel model)
        {
            var rehydrated = _surveyResponseService.RehydrateSubmitModel(model);
            if (rehydrated == null)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(rehydrated);
            }

            try
            {
                _surveyResponseService.Submit(rehydrated, User != null && User.Identity != null ? User.Identity.Name : null, Server);
                TempData["SuccessMessage"] = "Survey response submitted successfully.";
                return RedirectToAction("Index", new { templateId = rehydrated.SurveyTemplateId, formId = rehydrated.SurveyFormId, customerId = rehydrated.SurveyCustomerDataId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(_surveyResponseService.RehydrateSubmitModel(rehydrated));
            }
        }

        [HttpGet]
        public JsonResult GetForms(int templateId)
        {
            var model = _surveyResponseService.GetStartModel(templateId, null, null);
            var data = model.Forms.Select(x => new { x.Id, x.Title }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCustomers(int templateId)
        {
            var model = _surveyResponseService.GetStartModel(templateId, null, null);
            var data = model.Customers.Select(x => new { x.Id, Name = x.ClientName + " (" + x.CustomerCode + ")" }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}
