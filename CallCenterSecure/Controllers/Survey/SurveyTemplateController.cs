using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CallCenterSecure.Models;
using CallCenterSecure.Services;
using ClosedXML.Excel;

namespace CallCenterSecure.Controllers.Survey
{
    public class SurveyTemplateController : Controller
    {
        private readonly ISurveyTemplateService _surveyTemplateService;
        private readonly ApplicationDbContext _db = new ApplicationDbContext();

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

        public ActionResult CustomerDataUpload(int? templateId)
        {
            ViewBag.SurveyTemplates = GetTemplateSelectList(templateId);
            ViewBag.SelectedTemplateId = templateId;

            var customers = Enumerable.Empty<SurveyCustomerData>();
            if (templateId.HasValue)
            {
                customers = _db.SurveyCustomerData
                    .Where(c => c.SurveyTemplateTypeId == templateId.Value)
                    .OrderBy(c => c.Id)
                    .ToList();
            }

            return View(customers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerDataUpload(HttpPostedFileBase file, int? surveyTemplateTypeId)
        {
            if (!surveyTemplateTypeId.HasValue)
            {
                ModelState.AddModelError("surveyTemplateTypeId", "Please select a survey template.");
            }

            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("file", "Please select an Excel file.");
            }
            else if (!IsExcelFile(file.FileName))
            {
                ModelState.AddModelError("file", "Only .xlsx and .xls files are allowed.");
            }

            ViewBag.SurveyTemplates = GetTemplateSelectList(surveyTemplateTypeId);
            ViewBag.SelectedTemplateId = surveyTemplateTypeId;

            if (!ModelState.IsValid)
            {
                return View(Enumerable.Empty<SurveyCustomerData>());
            }

            var customers = new List<SurveyCustomerData>();
            using (var workbook = new XLWorkbook(file.InputStream))
            {
                var ws = workbook.Worksheet(1);
                var rows = ws.RangeUsed()?.RowsUsed().Skip(1) ?? Enumerable.Empty<IXLRangeRow>();

                foreach (var row in rows)
                {
                    var customer = new SurveyCustomerData
                    {
                        ClientName = ExcelHelper.GetString(ws, row.RowNumber(), 1),
                        Gender = ExcelHelper.GetString(ws, row.RowNumber(), 2),
                        CustomerCode = ExcelHelper.GetString(ws, row.RowNumber(), 3),
                        MobileNumber1 = ExcelHelper.GetString(ws, row.RowNumber(), 4),
                        MobileNumber2 = ExcelHelper.GetString(ws, row.RowNumber(), 5),
                        Region = ExcelHelper.GetString(ws, row.RowNumber(), 6),
                        Branch = ExcelHelper.GetString(ws, row.RowNumber(), 7),
                        Location = ExcelHelper.GetString(ws, row.RowNumber(), 8),
                        LoanProduct = ExcelHelper.GetString(ws, row.RowNumber(), 9),
                        Age = ExcelHelper.GetInt(ws, row.RowNumber(), 10),
                        BusinessCategory = ExcelHelper.GetString(ws, row.RowNumber(), 11),
                        ActivitiesSector = ExcelHelper.GetString(ws, row.RowNumber(), 12),
                        LevelOfEducation = ExcelHelper.GetString(ws, row.RowNumber(), 13),
                        IncomeLevel = ExcelHelper.GetString(ws, row.RowNumber(), 14),
                        HouseholdAssets = ExcelHelper.GetString(ws, row.RowNumber(), 15),
                        PovertyScore = ExcelHelper.GetInt(ws, row.RowNumber(), 16),
                        SurveyTemplateTypeId = surveyTemplateTypeId.Value
                    };

                    customers.Add(customer);
                }
            }

            if (!customers.Any())
            {
                ModelState.AddModelError("file", "No customer rows were found in the uploaded file.");
                return View(Enumerable.Empty<SurveyCustomerData>());
            }

            var existingRecords = _db.SurveyCustomerData.Where(c => c.SurveyTemplateTypeId == surveyTemplateTypeId.Value);
            _db.SurveyCustomerData.RemoveRange(existingRecords);
            _db.SaveChanges();

            _db.SurveyCustomerData.AddRange(customers);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Customer data uploaded successfully.";
            return RedirectToAction("CustomerDataUpload", new { templateId = surveyTemplateTypeId.Value });
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

        private SelectList GetTemplateSelectList(int? selectedId)
        {
            return new SelectList(_surveyTemplateService.GetAll(), "Id", "Name", selectedId);
        }

        private bool IsExcelFile(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            return string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase);
        }
    }
}
