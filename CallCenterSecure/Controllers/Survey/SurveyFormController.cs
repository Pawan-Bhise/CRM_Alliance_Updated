using System;
using System.Net;
using System.Web.Mvc;
using CallCenterSecure.Models.ViewModels;
using CallCenterSecure.Services;

namespace CallCenterSecure.Controllers.Survey
{
    public class SurveyFormController : Controller
    {
        private readonly ISurveyFormService _surveyFormService;

        public SurveyFormController()
            : this(new SurveyFormService())
        {
        }

        internal SurveyFormController(ISurveyFormService surveyFormService)
        {
            _surveyFormService = surveyFormService;
        }

        public ActionResult Index(int? templateId)
        {
            var model = _surveyFormService.GetIndexModel(templateId);
            return View(model);
        }

        public ActionResult Create(int? templateId)
        {
            var model = _surveyFormService.GetCreateModel(templateId);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SurveyFormBuilderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(_surveyFormService.PrepareBuilderModel(model));
            }

            try
            {
                _surveyFormService.Create(model, User != null && User.Identity != null ? User.Identity.Name : null);
                TempData["SuccessMessage"] = "Survey form created successfully.";
                return RedirectToAction("Index", new { templateId = model.SurveyTemplateId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(_surveyFormService.PrepareBuilderModel(model));
            }
        }

        public ActionResult Edit(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = _surveyFormService.GetEditModel(id);
            if (model == null)
            {
                return HttpNotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, SurveyFormBuilderViewModel model)
        {
            if (id != model.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (!ModelState.IsValid)
            {
                return View(_surveyFormService.PrepareBuilderModel(model));
            }

            try
            {
                _surveyFormService.Update(model, User != null && User.Identity != null ? User.Identity.Name : null);
                TempData["SuccessMessage"] = "Survey form updated successfully.";
                return RedirectToAction("Index", new { templateId = model.SurveyTemplateId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(_surveyFormService.PrepareBuilderModel(model));
            }
        }
    }
}
