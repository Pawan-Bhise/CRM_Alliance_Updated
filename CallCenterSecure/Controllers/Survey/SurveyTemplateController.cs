using System;
using System.Net;
using System.Web.Mvc;
using CallCenterSecure.Models;
using CallCenterSecure.Services;


namespace CallCenterSecure.Controllers.Survey
{
    public class SurveyTemplateController : Controller
    {
        private readonly ISurveyTemplateService _surveyTemplateService;

        public SurveyTemplateController()
            : this(new SurveyTemplateService())
        {
        }

        internal SurveyTemplateController(ISurveyTemplateService surveyTemplateService)
        {
            _surveyTemplateService = surveyTemplateService;
        }

        public ActionResult Index()
        {
            var templates = _surveyTemplateService.GetAll();
            ViewBag.CanCreate = _surveyTemplateService.CanCreate();
            return View(templates);
        }

        public ActionResult Edit(int id)
        {
            var template = _surveyTemplateService.GetById(id);
            if (template == null)
            {
                return HttpNotFound();
            }

            var model = new SurveyTemplateViewModel
            {
                Id = template.Id,
                Name = template.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, SurveyTemplateViewModel model)
        {
            if (id != model.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var template = _surveyTemplateService.GetById(id);
            if (template == null)
            {
                return HttpNotFound();
            }

            template.Name = model.Name.Trim();
            _surveyTemplateService.Update(template);
            TempData["SuccessMessage"] = "Template updated successfully.";

            return RedirectToAction("Index");
        }

        public ActionResult Create()
        {
            if (!_surveyTemplateService.CanCreate())
            {
                TempData["ErrorMessage"] = "Cannot create more than 3 survey templates. Please delete one first.";
                return RedirectToAction("Index");
            }

            return View(new SurveyTemplateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SurveyTemplateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (!_surveyTemplateService.CanCreate())
            {
                TempData["ErrorMessage"] = "Cannot create more than 3 survey templates. Please delete one first.";
                return View(model);
            }

            var template = new SurveyTemplateType
            {
                Name = model.Name.Trim()
            };

            _surveyTemplateService.Create(template);
            TempData["SuccessMessage"] = "Template created successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                _surveyTemplateService.Delete(id);
                TempData["SuccessMessage"] = "Template deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
