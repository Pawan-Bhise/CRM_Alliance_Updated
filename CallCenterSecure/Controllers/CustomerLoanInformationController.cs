using CallCenter.Models;
using CallCenterSecure.Models;
using CallCenterSecure.Models.CustomerLoan;
using CallCenterSecure.Models.ViewModels;
using CallCenterSecure.Repositories;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using DocumentFormat.OpenXml.Wordprocessing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;


namespace CallCenter.Controllers
{
    public class CustomerLoanInformationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CustomerRepository customerRepository = new CustomerRepository();

        // GET: CustomerLoanInformation
        public ActionResult Index(int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var query = db.CustomerLoan.AsNoTracking().OrderBy(c => c.Id);
            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var model = new CustomerLoanIndexViewModel
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return View(model);
        }

        // GET: CustomerLoanInformation/Details/5
        public ActionResult Details(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerLoan customerLoanInformation = db.CustomerLoan.Find(id);
            if (customerLoanInformation == null)
            {
                return HttpNotFound();
            }
            return View(customerLoanInformation);
        }

        // GET: CustomerLoanInformation/Create
        public ActionResult Create()
        {
            ViewBag.AccountTypes = new SelectList(new[] { "Individual", "Joint" });
            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" });
            ViewBag.MaritalStatuses = new SelectList(new[] { "Single", "Married", "Divorced", "Widowed" });
            ViewBag.EducationalQualifications = new SelectList(new[] { "Primary", "Middle", "High School", "Graduate", "Post Graduate" });
            ViewBag.CustomerStatuses = new SelectList(new[] { "Active", "Inactive" });
            ViewBag.States = new SelectList(db.States, "StateName", "StateName");
            ViewBag.Districts = new SelectList(db.Districts, "DistrictName", "DistrictName");
            ViewBag.Cities = new SelectList(db.Cities, "CityName", "CityName");
            ViewBag.VillageTracts = new SelectList(db.VillageTracts, "VillageTractName", "VillageTractName");
            ViewBag.Villages = new SelectList(db.Villages, "VillageName", "VillageName");
            ViewBag.Areas = new SelectList(db.Areas, "AreaName", "AreaName");
            ViewBag.Branches = new SelectList(db.AllianceBranches, "BranchName", "BranchName");
            ViewBag.Products = new SelectList(db.AllianceProducts, "ProductName", "ProductName");
            return View();
        }

        // POST: CustomerLoanInformation/Upload
        //[HttpPost]
        //public ActionResult Upload(HttpPostedFileBase file)
        //{
        //    if (file == null || file.ContentLength == 0)
        //    {
        //        ModelState.AddModelError("", "Please select an Excel file");
        //        return View();
        //    }

        //    var customers = new List<CustomerLoan>();

        //    using (var workbook = new XLWorkbook(file.InputStream))
        //    {
        //        var ws = workbook.Worksheet(1);
        //        var rows = ws.RangeUsed().RowsUsed().Skip(1); // skip header

        //        foreach (var row in rows)
        //        {
        //            var customer = new CustomerLoan
        //            {
        //                GroupCode = ExcelHelper.GetInt(ws, row.RowNumber(), 1),
        //                COCashAccount = ExcelHelper.GetString(ws, row.RowNumber(), 2),
        //                COStaffId = ExcelHelper.GetString(ws, row.RowNumber(), 3),
        //                COName = ExcelHelper.GetString(ws, row.RowNumber(), 4),
        //                ProductCode = ExcelHelper.GetString(ws, row.RowNumber(), 5),
        //                ProductName = ExcelHelper.GetString(ws, row.RowNumber(), 6),
        //                ProductCategory = ExcelHelper.GetString(ws, row.RowNumber(), 7),
        //                CustomerCode = ExcelHelper.GetString(ws, row.RowNumber(), 8),
        //                AccountNumber = ExcelHelper.GetString(ws, row.RowNumber(), 9),
        //                BranchCode = ExcelHelper.GetInt(ws, row.RowNumber(), 10) ?? 0,
        //                BranchName = ExcelHelper.GetString(ws, row.RowNumber(), 11),
        //                ParentBranchName = ExcelHelper.GetString(ws, row.RowNumber(), 12),
        //                RegionalBranchName = ExcelHelper.GetString(ws, row.RowNumber(), 13),
        //                DateOfActOpening = ExcelHelper.GetDate(ws, row.RowNumber(), 14),
        //                Salutation = ExcelHelper.GetInt(ws, row.RowNumber(), 15) ?? 0,
        //                CustomerName = ExcelHelper.GetString(ws, row.RowNumber(), 16),
        //                Gender = ExcelHelper.GetString(ws, row.RowNumber(), 17),
        //                FatherName = ExcelHelper.GetString(ws, row.RowNumber(), 18),
        //                AreaType = ExcelHelper.GetString(ws, row.RowNumber(), 19),
        //                Area = ExcelHelper.GetString(ws, row.RowNumber(), 20),
        //                VillageWard = ExcelHelper.GetString(ws, row.RowNumber(), 21),
        //                VillageTractTown = ExcelHelper.GetString(ws, row.RowNumber(), 22),
        //                CityTownship = ExcelHelper.GetString(ws, row.RowNumber(), 23),
        //                District = ExcelHelper.GetString(ws, row.RowNumber(), 24),
        //                RegionState = ExcelHelper.GetString(ws, row.RowNumber(), 25),
        //                NRC = ExcelHelper.GetString(ws, row.RowNumber(), 26),
        //                MobileNo1 = ExcelHelper.GetString(ws, row.RowNumber(), 27),
        //                MobileNo2 = ExcelHelper.GetString(ws, row.RowNumber(), 28),
        //                CustomerStatus = ExcelHelper.GetString(ws, row.RowNumber(), 29),
        //                FreezeStatus = ExcelHelper.GetString(ws, row.RowNumber(), 30),
        //                DisbursedAmount = ExcelHelper.GetString(ws, row.RowNumber(), 31),
        //                LPFAmount = ExcelHelper.GetString(ws, row.RowNumber(), 32),
        //                Installments = ExcelHelper.GetInt(ws, row.RowNumber(), 33),
        //                InstallmentAmount = ExcelHelper.GetString(ws, row.RowNumber(), 34),
        //                PaymentFrequency = ExcelHelper.GetString(ws, row.RowNumber(), 35),
        //                PrincipleOutstanding = ExcelHelper.GetString(ws, row.RowNumber(), 36),
        //                InterestReceivable = ExcelHelper.GetString(ws, row.RowNumber(), 37),
        //                NonCreditCustomer = ExcelHelper.GetString(ws, row.RowNumber(), 38),
        //                VoluntaryDepositor = ExcelHelper.GetString(ws, row.RowNumber(), 39),
        //                PovertyScore = ExcelHelper.GetString(ws, row.RowNumber(), 40),
        //                HouseholdSurplusIncome = ExcelHelper.GetString(ws, row.RowNumber(), 41),
        //                Purpose = ExcelHelper.GetString(ws, row.RowNumber(), 42),
        //                BusinessCategory = ExcelHelper.GetString(ws, row.RowNumber(), 43),
        //                BusinessActivity = ExcelHelper.GetString(ws, row.RowNumber(), 44),
        //                AccountStatus = ExcelHelper.GetString(ws, row.RowNumber(), 45),
        //                MaturitydateLoan = ExcelHelper.GetDate(ws, row.RowNumber(), 46),
        //                PARClient = ExcelHelper.GetString(ws, row.RowNumber(), 47),
        //                DayOfOverDue = ExcelHelper.GetInt(ws, row.RowNumber(), 48),
        //                AreaStatus = ExcelHelper.GetString(ws, row.RowNumber(), 49)
        //                // CreatedOn will use default value
        //            };

        //            customers.Add(customer);
        //        }

        //        if (customers.Count > 0) {
        //            //remove prev data
        //            db.CustomerLoan.RemoveRange(db.CustomerLoan);
        //            db.SaveChanges();
        //        }

        //        // Bulk insert                
        //        db.CustomerLoan.AddRange(customers);
        //        db.SaveChanges();

        //        TempData["SuccessMessage"] = "Record uoloaded successfully!";
        //    }

        //    return RedirectToAction("Index");
        //}

        // GET: CustomerLoanInformation/Upload
        public ActionResult Upload()
        {
            var jobs = db.UploadJobs.OrderByDescending(j => j.CreatedOn).Take(20).ToList();
            return View(jobs);
        }

        // POST: CustomerLoanInformation/Upload
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("", "Please select a CSV file");
                return Upload();
            }
            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Please upload a CSV file.");
                return Upload();
            }

            var uploadsPath = Server.MapPath("~/Uploads");
            Directory.CreateDirectory(uploadsPath);

            var uniqueFileName = Path.GetFileNameWithoutExtension(file.FileName)
                                 + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")
                                 + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            file.SaveAs(filePath);

            var uploadJob = new UploadJob
            {
                FileName = uniqueFileName,
                FilePath = filePath,
                Status = "Pending",
                CreatedOn = DateTime.UtcNow
            };

            db.UploadJobs.Add(uploadJob);
            db.SaveChanges();

            HostingEnvironment.QueueBackgroundWorkItem(async ct => await ProcessUploadJobAsync(uploadJob.UploadJobId, ct));

            TempData["SuccessMessage"] = "Upload started successfully. Current status: Processing. Please check after a few minutes.";
            return RedirectToAction("Upload");
        }

        private async Task ProcessUploadJobAsync(int uploadJobId, CancellationToken cancellationToken)
        {
            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var job = context.UploadJobs.Find(uploadJobId);
                    if (job == null)
                        return;

                    job.Status = "Processing";
                    job.StartedOn = DateTime.UtcNow;
                    context.SaveChanges();
                }

                await Task.Run(() => ProcessCsvFile(uploadJobId, cancellationToken), cancellationToken);
            }
            catch
            {
                // If starting fails, leave the job as Pending.
            }
        }

        private void ProcessCsvFile(int uploadJobId, CancellationToken cancellationToken)
        {
            using (var context = new ApplicationDbContext())
            {
                var job = context.UploadJobs.Find(uploadJobId);
                if (job == null)
                    return;

                try
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        job.Status = "Canceled";
                        job.Message = "Upload was canceled.";
                        job.CompletedOn = DateTime.UtcNow;
                        context.SaveChanges();
                        return;
                    }

                    context.Database.ExecuteSqlCommand("TRUNCATE TABLE CustomerLoans");

                    var dataTable = CreateCustomerLoanTable();
                    const int batchSize = 5000;
                    int count = 0;

                    using (var reader = new StreamReader(job.FilePath))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        csv.ReadHeader();

                        while (csv.Read())
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;

                            dataTable.Rows.Add(
                                GetNullableInt(csv, "GroupCode"),
                                GetString(csv, "COCashAccount"),
                                GetString(csv, "COStaffId"),
                                GetString(csv, "COName"),
                                GetString(csv, "ProductCode"),
                                GetString(csv, "ProductName"),
                                GetString(csv, "ProductCategory"),
                                GetString(csv, "CustomerCode"),
                                GetString(csv, "AccountNumber"),
                                GetInt(csv, "BranchCode", true),
                                GetString(csv, "BranchName"),
                                GetString(csv, "ParentBranchName"),
                                GetString(csv, "RegionalBranchName"),
                                ParseDateField(csv, "DateOfActOpening"),
                                GetInt(csv, "Salutation", true),
                                GetString(csv, "CustomerName"),
                                GetString(csv, "Gender"),
                                GetString(csv, "FatherName"),
                                GetString(csv, "AreaType"),
                                GetString(csv, "Area"),
                                GetString(csv, "VillageWard"),
                                GetString(csv, "VillageTractTown"),
                                GetString(csv, "CityTownship"),
                                GetString(csv, "District"),
                                GetString(csv, "RegionState"),
                                GetString(csv, "NRC"),
                                GetString(csv, "MobileNo1"),
                                GetString(csv, "MobileNo2"),
                                GetString(csv, "CustomerStatus"),
                                GetString(csv, "FreezeStatus"),
                                GetString(csv, "DisbursedAmount"),
                                GetString(csv, "LPFAmount"),
                                GetNullableInt(csv, "Installments"),
                                GetString(csv, "InstallmentAmount"),
                                GetString(csv, "PaymentFrequency"),
                                GetString(csv, "PrincipleOutstanding"),
                                GetString(csv, "InterestReceivable"),
                                GetString(csv, "NonCreditCustomer"),
                                GetString(csv, "VoluntaryDepositor"),
                                GetString(csv, "PovertyScore"),
                                GetString(csv, "HouseholdSurplusIncome"),
                                GetString(csv, "Purpose"),
                                GetString(csv, "BusinessCategory"),
                                GetString(csv, "BusinessActivity"),
                                GetString(csv, "AccountStatus"),
                                ParseDateField(csv, "MaturitydateLoan"),
                                GetString(csv, "PARClient"),
                                GetNullableInt(csv, "DayOfOverDue"),
                                GetString(csv, "AreaStatus"),
                                DateTime.UtcNow
                            );

                            count++;

                            if (count % batchSize == 0)
                            {
                                BulkInsert(context.Database.Connection.ConnectionString, dataTable);
                                dataTable.Clear();
                            }
                        }

                        if (dataTable.Rows.Count > 0)
                        {
                            BulkInsert(context.Database.Connection.ConnectionString, dataTable);
                        }
                    }

                    job.Status = cancellationToken.IsCancellationRequested ? "Canceled" : "Completed";
                    job.CompletedOn = DateTime.UtcNow;
                    job.ProcessedRows = count;
                    job.Message = cancellationToken.IsCancellationRequested ? "The upload was canceled before completion." : $"Processed {count} rows.";
                }
                catch (Exception ex)
                {
                    job.Status = "Failed";
                    job.Message = ex.Message;
                    job.CompletedOn = DateTime.UtcNow;
                }

                context.SaveChanges();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerLoanInformation customerLoanInformation, HttpPostedFileBase PI_CustomerPhoto)
        {
            if (ModelState.IsValid)
            {
                //if (PI_CustomerPhoto != null && PI_CustomerPhoto.ContentLength > 0)
                //{
                //    using (var reader = new System.IO.BinaryReader(PI_CustomerPhoto.InputStream))
                //    {
                //        customerLoanInformation.PI_CustomerPhoto = reader.ReadBytes(PI_CustomerPhoto.ContentLength);
                //    }
                //}
                db.CustomerLoanInformations.Add(customerLoanInformation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AccountTypes = new SelectList(new[] { "Individual", "Joint" }, customerLoanInformation.AccountType);
            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, customerLoanInformation.PI_Gender);
            ViewBag.MaritalStatuses = new SelectList(new[] { "Single", "Married", "Divorced", "Widowed" }, customerLoanInformation.PI_MaritalStatus);
            ViewBag.EducationalQualifications = new SelectList(new[] { "Primary", "Middle", "High School", "Graduate", "Post Graduate" }, customerLoanInformation.PI_EducationalQualification);
            ViewBag.CustomerStatuses = new SelectList(new[] { "Active", "Inactive" }, customerLoanInformation.CustomerStatus);
            ViewBag.States = new SelectList(db.States, "StateName", "StateName", customerLoanInformation.PI_StateRegion);
            ViewBag.Districts = new SelectList(db.Districts, "DistrictName", "DistrictName", customerLoanInformation.PI_District);
            ViewBag.Cities = new SelectList(db.Cities, "CityName", "CityName", customerLoanInformation.PI_District);
            ViewBag.VillageTracts = new SelectList(db.VillageTracts, "VillageTractName", "VillageTractName", customerLoanInformation.PI_VillageTractTown);
            ViewBag.Villages = new SelectList(db.Villages, "VillageName", "VillageName", customerLoanInformation.PI_VillageWard);
            ViewBag.Areas = new SelectList(db.Areas, "AreaName", "AreaName", customerLoanInformation.PI_Area);
            ViewBag.Branches = new SelectList(db.AllianceBranches, "BranchName", "BranchName", customerLoanInformation.Branch);
            ViewBag.Products = new SelectList(db.Products, "ProductName", "ProductName", customerLoanInformation.ProductInterested);
            return View(customerLoanInformation);
        }

        // GET: CustomerLoanInformation/Edit/5
        public ActionResult Edit(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerLoan customerLoan = db.CustomerLoan.Find(id);
            if (customerLoan == null)
            {
                return HttpNotFound();
            }

            //PopulateDropdowns();
            return View(customerLoan);
        }

        private void PopulateDropdowns()
        {
            ViewBag.AccountTypes = new SelectList(new[] { "Individual", "Joint" }, "Value", "Text");
            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, "Value", "Text");
            ViewBag.MaritalStatuses = new SelectList(new[] { "Single", "Married", "Divorced", "Widowed" }, "Value", "Text");
            ViewBag.EducationalQualifications = new SelectList(new[] { "Primary", "Middle", "High School", "Graduate", "Post Graduate" }, "Value", "Text");
            ViewBag.CustomerStatuses = new SelectList(new[] { "Active", "Inactive" }, "Value", "Text");

            ViewBag.States = new SelectList(db.States.Select(d => new { Value = d.StateCode, Text = d.StateCode }), "Value", "Text");
            ViewBag.Districts = new SelectList(db.Districts.Select(d => new { Value = d.DistrictCode, Text = d.DistrictName }), "Value", "Text");
            ViewBag.Cities = new SelectList(db.Cities.Select(c => new { Value = c.CityCode, Text = c.CityName }), "Value", "Text");
            ViewBag.VillageTracts = new SelectList(db.VillageTracts.Select(vt => new { Value = vt.VillageTractCode, Text = vt.VillageTractName }), "Value", "Text");
            ViewBag.Villages = new SelectList(db.Villages.Select(v => new { Value = v.VillageCode, Text = v.VillageName }), "Value", "Text");
            ViewBag.Branches = new SelectList(db.Branches.Select(b => new { Value = b.Id, Text = b.Name }), "Value", "Text");
            ViewBag.Products = new SelectList(db.Products.Select(p => new { Value = p.Id, Text = p.Name }), "Value", "Text");
            ViewBag.Areas = new SelectList(db.Areas.Select(p => new { Value = p.AreaName, Text = p.AreaName }), "Value", "Text");

        }

        // POST: CustomerLoanInformation/Edit/5
        // POST: CustomerLoan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerLoan model, HttpPostedFileBase PI_CustomerPhoto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existing = db.CustomerLoan.Find(model.Id);
                    if (existing == null)
                        return HttpNotFound();

                    // Optional: handle uploaded photo
                    //if (PI_CustomerPhoto != null && PI_CustomerPhoto.ContentLength > 0)
                    //{
                    //    using (var reader = new System.IO.BinaryReader(PI_CustomerPhoto.InputStream))
                    //    {
                    //        existing.PI_CustomerPhoto = reader.ReadBytes(PI_CustomerPhoto.ContentLength);
                    //    }
                    //}

                    // Update all 49 editable columns explicitly
                    existing.GroupCode = model.GroupCode;
                    existing.COCashAccount = model.COCashAccount;
                    existing.COStaffId = model.COStaffId;
                    existing.COName = model.COName;
                    existing.ProductCode = model.ProductCode;
                    existing.ProductName = model.ProductName;
                    existing.ProductCategory = model.ProductCategory;
                    existing.CustomerCode = model.CustomerCode;
                    existing.AccountNumber = model.AccountNumber;
                    existing.BranchCode = model.BranchCode;
                    existing.BranchName = model.BranchName;
                    existing.ParentBranchName = model.ParentBranchName;
                    existing.RegionalBranchName = model.RegionalBranchName;
                    existing.DateOfActOpening = model.DateOfActOpening;
                    existing.Salutation = model.Salutation;
                    existing.CustomerName = model.CustomerName;
                    existing.Gender = model.Gender;
                    existing.FatherName = model.FatherName;
                    existing.AreaType = model.AreaType;
                    existing.Area = model.Area;
                    existing.VillageWard = model.VillageWard;
                    existing.VillageTractTown = model.VillageTractTown;
                    existing.CityTownship = model.CityTownship;
                    existing.District = model.District;
                    existing.RegionState = model.RegionState;
                    existing.NRC = model.NRC;
                    existing.MobileNo1 = model.MobileNo1;
                    existing.MobileNo2 = model.MobileNo2;
                    existing.CustomerStatus = model.CustomerStatus;
                    existing.FreezeStatus = model.FreezeStatus;
                    existing.DisbursedAmount = model.DisbursedAmount;
                    existing.Installments = model.Installments;
                    existing.InstallmentAmount = model.InstallmentAmount;
                    existing.Purpose = model.Purpose;
                    existing.BusinessCategory = model.BusinessCategory;
                    existing.BusinessActivity = model.BusinessActivity;
                    existing.MaturitydateLoan = model.MaturitydateLoan;
                    existing.PARClient = model.PARClient;
                    existing.DayOfOverDue = model.DayOfOverDue;
                    existing.AreaStatus = model.AreaStatus;
                    existing.PaymentFrequency = model.PaymentFrequency;
                    existing.PrincipleOutstanding = model.PrincipleOutstanding;
                    existing.InterestReceivable = model.InterestReceivable;
                    existing.NonCreditCustomer = model.NonCreditCustomer;
                    existing.VoluntaryDepositor = model.VoluntaryDepositor;
                    existing.PovertyScore = model.PovertyScore;
                    existing.HouseholdSurplusIncome = model.HouseholdSurplusIncome;


                    db.SaveChanges();
                    TempData["SuccessMessage"] = "Record updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Something went wrong. Please try again.";
                    throw;
                }

                return RedirectToAction("Index");
            }

            // Re-populate dropdowns in case of validation errors
            //ViewBag.AccountTypes = new SelectList(new[] { "Individual", "Joint" }, model.AccountType);
            ViewBag.Genders = new SelectList(new[] { "Male", "Female", "Other" }, model.Gender);
            //ViewBag.MaritalStatuses = new SelectList(new[] { "Single", "Married", "Divorced", "Widowed" }, model.PI_MaritalStatus);
            //ViewBag.EducationalQualifications = new SelectList(new[] { "Primary", "Middle", "High School", "Graduate", "Post Graduate" }, model.PI_EducationalQualification);
            ViewBag.CustomerStatuses = new SelectList(new[] { "Active", "Inactive" }, model.CustomerStatus);
            ViewBag.FreezeStatuses = new SelectList(new[] { "Yes", "No" }, model.FreezeStatus);
            ViewBag.States = new SelectList(db.States, "StateName", "StateName", model.RegionState);
            ViewBag.Districts = new SelectList(db.Districts, "DistrictName", "DistrictName", model.District);
            ViewBag.Cities = new SelectList(db.Cities, "CityName", "CityName", model.CityTownship);
            ViewBag.VillageTracts = new SelectList(db.VillageTracts, "VillageTractName", "VillageTractName", model.VillageTractTown);
            ViewBag.Villages = new SelectList(db.Villages, "VillageName", "VillageName", model.VillageWard);
            ViewBag.Areas = new SelectList(db.Areas, "AreaName", "AreaName", model.Area);
            ViewBag.Branches = new SelectList(db.AllianceBranches, "BranchName", "BranchName", model.BranchName);
            ViewBag.Products = new SelectList(db.Products, "ProductName", "ProductName", model.ProductName);
            //ViewBag.BusinessCategories = new SelectList(db.BusinessCategories, "CategoryName", "CategoryName", model.BusinessCategory);

            return View(model);
        }


        // GET: CustomerLoanInformation/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CustomerLoanInformation customerLoanInformation = db.CustomerLoanInformations.Find(id);
            if (customerLoanInformation == null)
            {
                return HttpNotFound();
            }
            return View(customerLoanInformation);
        }

        // POST: CustomerLoanInformation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            CustomerLoanInformation customerLoanInformation = db.CustomerLoanInformations.Find(id);
            db.CustomerLoanInformations.Remove(customerLoanInformation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private DateTime? SafeDate(DateTime? dt)
        {
            if (!dt.HasValue) return null;
            if (dt.Value < new DateTime(1753, 1, 1))
                return null;  // or set to min allowed date
            return dt;
        }

        private string GetString(ExcelWorksheet ws, int row, int col)
        {
            return ws.Cells[row, col].Text?.Trim();
        }

        public static int? GetInt(ExcelWorksheet ws, int row, int col)
        {
            var val = ws.Cells[row, col].Text;
            return int.TryParse(val, out var i) ? (int?)i : null;
        }

        private DateTime? GetDate(ExcelWorksheet ws, int row, int col)
        {
            var text = ws.Cells[row, col].Text;

            if (DateTime.TryParseExact(
                text,
                "dd-MM-yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dt))
            {
                return dt;
            }
            return null;
        }

        private string GetAmount(ExcelWorksheet ws, int row, int col)
        {
            return ws.Cells[row, col].Text?
                .Replace("\u00A0", "") // remove non-breaking space
                .Trim();
        }

        private DataTable CreateCustomerLoanTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("GroupCode", typeof(int)).AllowDBNull = true;
            dt.Columns.Add("COCashAccount", typeof(string));
            dt.Columns.Add("COStaffId", typeof(string));
            dt.Columns.Add("COName", typeof(string));
            dt.Columns.Add("ProductCode", typeof(string));
            dt.Columns.Add("ProductName", typeof(string));
            dt.Columns.Add("ProductCategory", typeof(string));
            dt.Columns.Add("CustomerCode", typeof(string));
            dt.Columns.Add("AccountNumber", typeof(string));
            dt.Columns.Add("BranchCode", typeof(int));
            dt.Columns.Add("BranchName", typeof(string));
            dt.Columns.Add("ParentBranchName", typeof(string));
            dt.Columns.Add("RegionalBranchName", typeof(string));
            dt.Columns.Add("DateOfActOpening", typeof(DateTime)).AllowDBNull = true;
            dt.Columns.Add("Salutation", typeof(int));
            dt.Columns.Add("CustomerName", typeof(string));
            dt.Columns.Add("Gender", typeof(string));
            dt.Columns.Add("FatherName", typeof(string));
            dt.Columns.Add("AreaType", typeof(string));
            dt.Columns.Add("Area", typeof(string));
            dt.Columns.Add("VillageWard", typeof(string));
            dt.Columns.Add("VillageTractTown", typeof(string));
            dt.Columns.Add("CityTownship", typeof(string));
            dt.Columns.Add("District", typeof(string));
            dt.Columns.Add("RegionState", typeof(string));
            dt.Columns.Add("NRC", typeof(string));
            dt.Columns.Add("MobileNo1", typeof(string));
            dt.Columns.Add("MobileNo2", typeof(string));
            dt.Columns.Add("CustomerStatus", typeof(string));
            dt.Columns.Add("FreezeStatus", typeof(string));
            dt.Columns.Add("DisbursedAmount", typeof(string));
            dt.Columns.Add("LPFAmount", typeof(string));
            dt.Columns.Add("Installments", typeof(int)).AllowDBNull = true;
            dt.Columns.Add("InstallmentAmount", typeof(string));
            dt.Columns.Add("PaymentFrequency", typeof(string));
            dt.Columns.Add("PrincipleOutstanding", typeof(string));
            dt.Columns.Add("InterestReceivable", typeof(string));
            dt.Columns.Add("NonCreditCustomer", typeof(string));
            dt.Columns.Add("VoluntaryDepositor", typeof(string));
            dt.Columns.Add("PovertyScore", typeof(string));
            dt.Columns.Add("HouseholdSurplusIncome", typeof(string));
            dt.Columns.Add("Purpose", typeof(string));
            dt.Columns.Add("BusinessCategory", typeof(string));
            dt.Columns.Add("BusinessActivity", typeof(string));
            dt.Columns.Add("AccountStatus", typeof(string));
            dt.Columns.Add("MaturitydateLoan", typeof(DateTime)).AllowDBNull = true;
            dt.Columns.Add("PARClient", typeof(string));
            dt.Columns.Add("DayOfOverDue", typeof(int)).AllowDBNull = true;
            dt.Columns.Add("AreaStatus", typeof(string));
            dt.Columns.Add("CreatedOn", typeof(DateTime));

            return dt;
        }

        private void BulkInsert(string connectionString, DataTable dt)
        {
            using (var bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = "dbo.CustomerLoans";
                bulkCopy.BatchSize = 5000;
                bulkCopy.BulkCopyTimeout = 0;

                bulkCopy.ColumnMappings.Add("GroupCode", "GroupCode");
                bulkCopy.ColumnMappings.Add("COCashAccount", "COCashAccount");
                bulkCopy.ColumnMappings.Add("COStaffId", "COStaffId");
                bulkCopy.ColumnMappings.Add("COName", "COName");
                bulkCopy.ColumnMappings.Add("ProductCode", "ProductCode");
                bulkCopy.ColumnMappings.Add("ProductName", "ProductName");
                bulkCopy.ColumnMappings.Add("ProductCategory", "ProductCategory");
                bulkCopy.ColumnMappings.Add("CustomerCode", "CustomerCode");
                bulkCopy.ColumnMappings.Add("AccountNumber", "AccountNumber");
                bulkCopy.ColumnMappings.Add("BranchCode", "BranchCode");
                bulkCopy.ColumnMappings.Add("BranchName", "BranchName");
                bulkCopy.ColumnMappings.Add("ParentBranchName", "ParentBranchName");
                bulkCopy.ColumnMappings.Add("RegionalBranchName", "RegionalBranchName");
                bulkCopy.ColumnMappings.Add("DateOfActOpening", "DateOfActOpening");
                bulkCopy.ColumnMappings.Add("Salutation", "Salutation");
                bulkCopy.ColumnMappings.Add("CustomerName", "CustomerName");
                bulkCopy.ColumnMappings.Add("Gender", "Gender");
                bulkCopy.ColumnMappings.Add("FatherName", "FatherName");
                bulkCopy.ColumnMappings.Add("AreaType", "AreaType");
                bulkCopy.ColumnMappings.Add("Area", "Area");
                bulkCopy.ColumnMappings.Add("VillageWard", "VillageWard");
                bulkCopy.ColumnMappings.Add("VillageTractTown", "VillageTractTown");
                bulkCopy.ColumnMappings.Add("CityTownship", "CityTownship");
                bulkCopy.ColumnMappings.Add("District", "District");
                bulkCopy.ColumnMappings.Add("RegionState", "RegionState");
                bulkCopy.ColumnMappings.Add("NRC", "NRC");
                bulkCopy.ColumnMappings.Add("MobileNo1", "MobileNo1");
                bulkCopy.ColumnMappings.Add("MobileNo2", "MobileNo2");
                bulkCopy.ColumnMappings.Add("CustomerStatus", "CustomerStatus");
                bulkCopy.ColumnMappings.Add("FreezeStatus", "FreezeStatus");
                bulkCopy.ColumnMappings.Add("DisbursedAmount", "DisbursedAmount");
                bulkCopy.ColumnMappings.Add("LPFAmount", "LPFAmount");
                bulkCopy.ColumnMappings.Add("Installments", "Installments");
                bulkCopy.ColumnMappings.Add("InstallmentAmount", "InstallmentAmount");
                bulkCopy.ColumnMappings.Add("PaymentFrequency", "PaymentFrequency");
                bulkCopy.ColumnMappings.Add("PrincipleOutstanding", "PrincipleOutstanding");
                bulkCopy.ColumnMappings.Add("InterestReceivable", "InterestReceivable");
                bulkCopy.ColumnMappings.Add("NonCreditCustomer", "NonCreditCustomer");
                bulkCopy.ColumnMappings.Add("VoluntaryDepositor", "VoluntaryDepositor");
                bulkCopy.ColumnMappings.Add("PovertyScore", "PovertyScore");
                bulkCopy.ColumnMappings.Add("HouseholdSurplusIncome", "HouseholdSurplusIncome");
                bulkCopy.ColumnMappings.Add("Purpose", "Purpose");
                bulkCopy.ColumnMappings.Add("BusinessCategory", "BusinessCategory");
                bulkCopy.ColumnMappings.Add("BusinessActivity", "BusinessActivity");
                bulkCopy.ColumnMappings.Add("AccountStatus", "AccountStatus");
                bulkCopy.ColumnMappings.Add("MaturitydateLoan", "MaturitydateLoan");
                bulkCopy.ColumnMappings.Add("PARClient", "PARClient");
                bulkCopy.ColumnMappings.Add("DayOfOverDue", "DayOfOverDue");
                bulkCopy.ColumnMappings.Add("AreaStatus", "AreaStatus");
                bulkCopy.ColumnMappings.Add("CreatedOn", "CreatedOn");

                bulkCopy.WriteToServer(dt);
            }
        }
        private object ParseDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            DateTime date;
            if (DateTime.TryParse(value, out date))
                return date;

            return DBNull.Value;
        }

        private int GetInt(CsvReader csv, string columnName, bool required)
        {
            if (!csv.TryGetField(columnName, out string value))
                return 0;

            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return int.TryParse(value.Trim(), out var intValue) ? intValue : 0;
        }

        private object GetNullableInt(CsvReader csv, string columnName)
        {
            if (!csv.TryGetField(columnName, out string value))
                return DBNull.Value;

            if (string.IsNullOrWhiteSpace(value))
                return DBNull.Value;

            return int.TryParse(value.Trim(), out var intValue) ? (object)intValue : DBNull.Value;
        }

        private DateTime? ParseDateField(CsvReader csv, string columnName)
        {
            if (!csv.TryGetField(columnName, out string value))
                return null;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (DateTime.TryParse(value.Trim(), out var dateValue))
                return dateValue;

            if (DateTime.TryParseExact(value.Trim(), "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                return dateValue;

            return null;
        }

        private string GetString(CsvReader csv, string columnName)
        {
            if (!csv.TryGetField(columnName, out string value))
                return null;

            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

    }
}
