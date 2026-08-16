using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CallCenterSecure.Models;
using CallCenterSecure.Services;
using ClosedXML.Excel;
using CsvHelper;
using System.Data.Entity;

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
            EnsureUploadJobColumns();

            ViewBag.SurveyTemplates = GetTemplateSelectList(templateId);
            ViewBag.SelectedTemplateId = templateId;
            ViewBag.UploadHistory = GetUploadHistory();

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

        [HttpGet]
        public FileResult DownloadSampleTemplate()
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");
            var headers = new[]
            {
                "ClientName",
                "Gender",
                "CustomerCode",
                "MobileNumber1",
                "MobileNumber2",
                "Region",
                "Branch",
                "AreaType",
                "Location",
                "LoanProduct",
                "Age",
                "NumberOfFamilyMembers",
                "BusinessCategory",
                "ActivitiesSector",
                "LevelOfEducation",
                "IncomeLevel",
                "HouseholdAssets",
                "PovertyScore",
                "LoanCycle",
                "DisbursedAmount",
                "CustomerStatus"
            };

            for (var index = 0; index < headers.Length; index++)
            {
                worksheet.Cell(1, index + 1).Value = headers[index];
            }

            worksheet.Cell(2, 1).Value = "Daw Hla Hla";
            worksheet.Cell(2, 2).Value = "Female";
            worksheet.Cell(2, 3).Value = "CUST-00125";
            worksheet.Cell(2, 4).Value = "+95-9-123456789";
            worksheet.Cell(2, 5).Value = "+95-9-987654321";
            worksheet.Cell(2, 6).Value = "Mandalay";
            worksheet.Cell(2, 7).Value = "Chan Mya Tharsi";
            worksheet.Cell(2, 8).Value = "Rural";
            worksheet.Cell(2, 9).Value = "Sint Gai";
            worksheet.Cell(2, 10).Value = "SA Loan";
            worksheet.Cell(2, 11).Value = 35;
            worksheet.Cell(2, 12).Value = 3;
            worksheet.Cell(2, 13).Value = "Agriculture";
            worksheet.Cell(2, 14).Value = "Crop Trading";
            worksheet.Cell(2, 15).Value = "Graduate";
            worksheet.Cell(2, 16).Value = "250,000 MMK";
            worksheet.Cell(2, 17).Value = "Motorcycle, Refrigerator";
            worksheet.Cell(2, 18).Value = 45;
            worksheet.Cell(2, 19).Value = 1;
            worksheet.Cell(2, 20).Value = "5,000,000 MMK";
            worksheet.Cell(2, 21).Value = "New Customer";

            worksheet.Columns().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SurveyCustomerData_Template.xlsx");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CustomerDataUpload(HttpPostedFileBase file, int? surveyTemplateTypeId, string campaignBatch)
        {
            EnsureUploadJobColumns();

            if (!surveyTemplateTypeId.HasValue)
            {
                ModelState.AddModelError("surveyTemplateTypeId", "Please select a survey template.");
            }

            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("file", "Please select a file.");
            }
            else if (!IsCsvFile(file.FileName))
            {
                ModelState.AddModelError("file", "Only .csv or .xlsx files are allowed.");
            }

            ViewBag.SurveyTemplates = GetTemplateSelectList(surveyTemplateTypeId);
            ViewBag.SelectedTemplateId = surveyTemplateTypeId;
            ViewBag.UploadHistory = GetUploadHistory();

            if (!ModelState.IsValid)
            {
                return View(Enumerable.Empty<SurveyCustomerData>());
            }

            var customers = new List<SurveyCustomerData>();
            var validationErrors = new List<string>();
            var seenCustomerCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var requiredHeaders = new[]
            {
                "ClientName",
                "Gender",
                "CustomerCode",
                "MobileNumber1",
                "MobileNumber2",
                "Region",
                "Branch",
                "AreaType",
                "Location",
                "LoanProduct",
                "Age",
                "NumberOfFamilyMembers",
                "BusinessCategory",
                "ActivitiesSector",
                "LevelOfEducation",
                "IncomeLevel",
                "HouseholdAssets",
                "PovertyScore",
                "LoanCycle",
                "DisbursedAmount",
                "CustomerStatus"
            };

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            if (extension == ".csv")
            {
                using (var reader = new StreamReader(file.InputStream))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();

                    var headerNames = csv.HeaderRecord ?? Array.Empty<string>();
                    if (headerNames.Length < requiredHeaders.Length)
                    {
                        ModelState.AddModelError("file", "The file does not contain enough columns. Expected at least " + requiredHeaders.Length + " columns.");
                        return View(Enumerable.Empty<SurveyCustomerData>());
                    }

                    var rowNumber = 1;
                    while (csv.Read())
                    {
                        rowNumber++;
                        var rowValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        for (var index = 0; index < requiredHeaders.Length; index++)
                        {
                            rowValues[requiredHeaders[index]] = GetCsvString(csv, index);
                        }

                        if (string.IsNullOrWhiteSpace(rowValues["ClientName"])
                            && string.IsNullOrWhiteSpace(rowValues["CustomerCode"])
                            && string.IsNullOrWhiteSpace(rowValues["MobileNumber1"]))
                        {
                            continue;
                        }

                        var customerCode = (rowValues["CustomerCode"] ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(rowValues["ClientName"]))
                        {
                            validationErrors.Add("Row " + rowNumber + ": ClientName is required.");
                        }

                        if (string.IsNullOrWhiteSpace(customerCode))
                        {
                            validationErrors.Add("Row " + rowNumber + ": CustomerCode is required.");
                        }
                        else if (seenCustomerCodes.Contains(customerCode))
                        {
                            validationErrors.Add("Row " + rowNumber + ": Duplicate CustomerCode '" + customerCode + "' within file.");
                        }
                        else
                        {
                            seenCustomerCodes.Add(customerCode);
                        }

                        if (string.IsNullOrWhiteSpace(rowValues["MobileNumber1"]))
                        {
                            validationErrors.Add("Row " + rowNumber + ": MobileNumber1 is required.");
                        }

                        var customer = new SurveyCustomerData
                        {
                            ClientName = rowValues["ClientName"],
                            Gender = rowValues["Gender"],
                            CustomerCode = customerCode,
                            MobileNumber1 = rowValues["MobileNumber1"],
                            MobileNumber2 = rowValues["MobileNumber2"],
                            Region = rowValues["Region"],
                            Branch = rowValues["Branch"],
                            AreaType = rowValues["AreaType"],
                            Location = rowValues["Location"],
                            LoanProduct = rowValues["LoanProduct"],
                            Age = GetCsvInt(csv, 10),
                            NumberOfFamilyMembers = GetCsvInt(csv, 11),
                            BusinessCategory = rowValues["BusinessCategory"],
                            ActivitiesSector = rowValues["ActivitiesSector"],
                            LevelOfEducation = rowValues["LevelOfEducation"],
                            IncomeLevel = rowValues["IncomeLevel"],
                            HouseholdAssets = rowValues["HouseholdAssets"],
                            PovertyScore = GetCsvInt(csv, 16),
                            LoanCycle = GetCsvInt(csv, 17),
                            DisbursedAmount = rowValues["DisbursedAmount"],
                            CustomerStatus = rowValues["CustomerStatus"],
                            SurveyTemplateTypeId = surveyTemplateTypeId.Value
                        };

                        if (!validationErrors.Any(error => error.Contains("Row " + rowNumber + ":")))
                        {
                            customers.Add(customer);
                        }
                    }
                }
            }
            else
            {
                // Assume Excel (.xlsx/.xls)
                using (var workbook = new XLWorkbook(file.InputStream))
                {
                    var worksheet = workbook.Worksheets.First();
                    var headerRow = worksheet.Row(1);
                    var headerNames = headerRow.Cells().Select(c => (c.Value ?? string.Empty).ToString()).ToArray();
                    if (headerNames.Length < requiredHeaders.Length)
                    {
                        ModelState.AddModelError("file", "The file does not contain enough columns. Expected at least " + requiredHeaders.Length + " columns.");
                        return View(Enumerable.Empty<SurveyCustomerData>());
                    }

                    var lastRow = worksheet.LastRowUsed().RowNumber();
                    for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
                    {
                        var rowValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        for (var index = 0; index < requiredHeaders.Length; index++)
                        {
                            var cell = worksheet.Cell(rowNumber, index + 1);
                            rowValues[requiredHeaders[index]] = cell?.GetValue<string>()?.Trim();
                        }

                        if (string.IsNullOrWhiteSpace(rowValues["ClientName"])
                            && string.IsNullOrWhiteSpace(rowValues["CustomerCode"])
                            && string.IsNullOrWhiteSpace(rowValues["MobileNumber1"]))
                        {
                            continue;
                        }

                        var customerCode = (rowValues["CustomerCode"] ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(rowValues["ClientName"]))
                        {
                            validationErrors.Add("Row " + rowNumber + ": ClientName is required.");
                        }

                        if (string.IsNullOrWhiteSpace(customerCode))
                        {
                            validationErrors.Add("Row " + rowNumber + ": CustomerCode is required.");
                        }
                        else if (seenCustomerCodes.Contains(customerCode))
                        {
                            validationErrors.Add("Row " + rowNumber + ": Duplicate CustomerCode '" + customerCode + "' within file.");
                        }
                        else
                        {
                            seenCustomerCodes.Add(customerCode);
                        }

                        if (string.IsNullOrWhiteSpace(rowValues["MobileNumber1"]))
                        {
                            validationErrors.Add("Row " + rowNumber + ": MobileNumber1 is required.");
                        }

                        int? age = null;
                        int tempInt;
                        if (!string.IsNullOrWhiteSpace(rowValues["Age"]) && int.TryParse(rowValues["Age"], out tempInt)) age = tempInt;
                        int? numFamily = null;
                        if (!string.IsNullOrWhiteSpace(rowValues["NumberOfFamilyMembers"]) && int.TryParse(rowValues["NumberOfFamilyMembers"], out tempInt)) numFamily = tempInt;
                        int? povertyScore = null;
                        if (!string.IsNullOrWhiteSpace(rowValues["PovertyScore"]) && int.TryParse(rowValues["PovertyScore"], out tempInt)) povertyScore = tempInt;
                        int? loanCycle = null;
                        if (!string.IsNullOrWhiteSpace(rowValues["LoanCycle"]) && int.TryParse(rowValues["LoanCycle"], out tempInt)) loanCycle = tempInt;

                        var customer = new SurveyCustomerData
                        {
                            ClientName = rowValues["ClientName"],
                            Gender = rowValues["Gender"],
                            CustomerCode = customerCode,
                            MobileNumber1 = rowValues["MobileNumber1"],
                            MobileNumber2 = rowValues["MobileNumber2"],
                            Region = rowValues["Region"],
                            Branch = rowValues["Branch"],
                            AreaType = rowValues["AreaType"],
                            Location = rowValues["Location"],
                            LoanProduct = rowValues["LoanProduct"],
                            Age = age,
                            NumberOfFamilyMembers = numFamily,
                            BusinessCategory = rowValues["BusinessCategory"],
                            ActivitiesSector = rowValues["ActivitiesSector"],
                            LevelOfEducation = rowValues["LevelOfEducation"],
                            IncomeLevel = rowValues["IncomeLevel"],
                            HouseholdAssets = rowValues["HouseholdAssets"],
                            PovertyScore = povertyScore,
                            LoanCycle = loanCycle,
                            DisbursedAmount = rowValues["DisbursedAmount"],
                            CustomerStatus = rowValues["CustomerStatus"],
                            SurveyTemplateTypeId = surveyTemplateTypeId.Value
                        };

                        if (!validationErrors.Any(error => error.Contains("Row " + rowNumber + ":")))
                        {
                            customers.Add(customer);
                        }
                    }
                }
            }

            if (validationErrors.Any())
            {
                var message = "Import validation failed: " + string.Join(" | ", validationErrors.Take(10));
                if (validationErrors.Count > 10)
                {
                    message += " ...";
                }

                ModelState.AddModelError("file", message);
                ViewBag.ValidationErrors = validationErrors;
                TempData["ErrorMessage"] = message;
                return View(Enumerable.Empty<SurveyCustomerData>());
            }

            if (!customers.Any())
            {
                ModelState.AddModelError("file", "No customer rows were found in the uploaded file.");
                ViewBag.ValidationErrors = validationErrors;
                return View(Enumerable.Empty<SurveyCustomerData>());
            }

            // Upsert: update existing customers by CustomerCode + template, insert new ones.
            // This preserves any existing SurveyFormResponse rows that reference customers.
            var existingCustomers = _db.SurveyCustomerData
                .Where(c => c.SurveyTemplateTypeId == surveyTemplateTypeId.Value)
                .ToList()
                .ToDictionary(c => c.CustomerCode ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            foreach (var cust in customers)
            {
                if (existingCustomers.TryGetValue(cust.CustomerCode ?? string.Empty, out var existing))
                {
                    // Update fields on existing record
                    existing.ClientName = cust.ClientName;
                    existing.Gender = cust.Gender;
                    existing.MobileNumber1 = cust.MobileNumber1;
                    existing.MobileNumber2 = cust.MobileNumber2;
                    existing.Region = cust.Region;
                    existing.Branch = cust.Branch;
                    existing.AreaType = cust.AreaType;
                    existing.Location = cust.Location;
                    existing.LoanProduct = cust.LoanProduct;
                    existing.Age = cust.Age;
                    existing.NumberOfFamilyMembers = cust.NumberOfFamilyMembers;
                    existing.BusinessCategory = cust.BusinessCategory;
                    existing.ActivitiesSector = cust.ActivitiesSector;
                    existing.LevelOfEducation = cust.LevelOfEducation;
                    existing.IncomeLevel = cust.IncomeLevel;
                    existing.HouseholdAssets = cust.HouseholdAssets;
                    existing.PovertyScore = cust.PovertyScore;
                    existing.LoanCycle = cust.LoanCycle;
                    existing.DisbursedAmount = cust.DisbursedAmount;
                    existing.CustomerStatus = cust.CustomerStatus;
                }
                else
                {
                    // Insert new record
                    _db.SurveyCustomerData.Add(cust);
                }
            }

            _db.SaveChanges();

            var uploadJob = new UploadJob
            {
                FileName = file.FileName,
                FilePath = Path.GetFileName(file.FileName),
                Status = "Success",
                Message = "Imported " + customers.Count + " rows" + (!string.IsNullOrWhiteSpace(campaignBatch) ? " | BatchTag: " + campaignBatch : string.Empty),
                ProcessedRows = customers.Count,
                CreatedOn = DateTime.Now,
                StartedOn = DateTime.Now,
                CompletedOn = DateTime.Now,
                UploadedBy = User.Identity.Name ?? "System",
                BatchTag = campaignBatch,
                Source = "Survey"
            };

            _db.UploadJobs.Add(uploadJob);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Customer data uploaded successfully." + (!string.IsNullOrWhiteSpace(campaignBatch) ? " Batch: " + campaignBatch : string.Empty);
            return RedirectToAction("CustomerDataUpload", new { templateId = surveyTemplateTypeId.Value });
        }

        public ActionResult ViewUploadLog(int id)
        {
            EnsureUploadJobColumns();

            var uploadJob = _db.UploadJobs.Find(id);
            if (uploadJob == null)
            {
                return HttpNotFound();
            }

            return View(uploadJob);
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

        private List<UploadJob> GetUploadHistory()
        {
            EnsureUploadJobColumns();

            // Return only uploads originating from the Survey upload page
            return _db.UploadJobs
                .Where(job => job.Source == "Survey")
                .OrderByDescending(job => job.CreatedOn)
                .Take(20)
                .ToList();
        }

        private void EnsureUploadJobColumns()
        {
            const string sql = @"
IF COL_LENGTH('dbo.UploadJobs', 'UploadedBy') IS NULL
BEGIN
    ALTER TABLE dbo.UploadJobs ADD UploadedBy NVARCHAR(255) NULL;
END;
IF COL_LENGTH('dbo.UploadJobs', 'BatchTag') IS NULL
BEGIN
    ALTER TABLE dbo.UploadJobs ADD BatchTag NVARCHAR(100) NULL;
END;
IF COL_LENGTH('dbo.UploadJobs', 'Source') IS NULL
BEGIN
    ALTER TABLE dbo.UploadJobs ADD Source NVARCHAR(50) NULL;
END;";

            _db.Database.ExecuteSqlCommand(sql);
        }

        private static string GetCsvString(CsvReader csv, int index)
        {
            var value = csv.GetField(index);
            return value == null ? null : value.Trim();
        }

        private static int? GetCsvInt(CsvReader csv, int index)
        {
            var value = csv.GetField(index);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            int number;
            return int.TryParse(value.Trim(), out number) ? (int?)number : null;
        }

        private bool IsCsvFile(string fileName)
        {
            var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
            return extension == ".csv" || extension == ".xlsx" || extension == ".xls";
        }
    }
}
