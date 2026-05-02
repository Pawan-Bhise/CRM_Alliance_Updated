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
    }
}
