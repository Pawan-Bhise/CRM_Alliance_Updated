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
                _surveyFormService.Create(model, GetCurrentUserName());
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
                _surveyFormService.Update(model, GetCurrentUserName());
                TempData["SuccessMessage"] = "Survey form updated successfully.";
                return RedirectToAction("Index", new { templateId = model.SurveyTemplateId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(_surveyFormService.PrepareBuilderModel(model));
            }
        }

        public ActionResult ExportExcel(int id)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                var workbookBytes = _surveyFormService.ExportExcel(id);
                return File(workbookBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", string.Format("survey-form-{0}.xlsx", id));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Duplicate(int id, int? templateId)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                _surveyFormService.Duplicate(id, GetCurrentUserName());
                TempData["SuccessMessage"] = "Survey form duplicated successfully.";
                return RedirectToAction("Index", new { templateId = templateId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { templateId = templateId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id, int? templateId)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                _surveyFormService.ToggleStatus(id, GetCurrentUserName());
                TempData["SuccessMessage"] = "Survey form status updated successfully.";
                return RedirectToAction("Index", new { templateId = templateId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { templateId = templateId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, int? templateId)
        {
            if (id <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            try
            {
                _surveyFormService.Delete(id, GetCurrentUserName());
                TempData["SuccessMessage"] = "Survey form deleted successfully.";
                return RedirectToAction("Index", new { templateId = templateId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", new { templateId = templateId });
            }
        }

        private string GetCurrentUserName()
        {
            return User != null && User.Identity != null ? User.Identity.Name : null;
        }
    }
}
