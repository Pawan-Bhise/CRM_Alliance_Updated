using CallCenter.CustomAuthentication;
using CallCenter.Models;
using CallCenterSecure.Models;
using CallCenterSecure.Models.Outbound;
using CallCenterSecure.Repositories;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace CallCenter.Controllers
{
    public class AllianceOutboundController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private CustomerRepository customerRepository = new CustomerRepository();

        // GET: AllianceOutbound/Create
        public ActionResult Create(string phoneNumber = null)
        {
            PopulateDropdowns();
            var model = new AllianceOutbound
            {
                PrimaryMobileNumber = phoneNumber // null is OK
            };

            return View(model);
        }

        // POST: AllianceOutbound/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AllianceOutbound allianceOutbound)
        {
            allianceOutbound.AgentName = ((CustomPrincipal)User).FirstName + " " + ((CustomPrincipal)User).LastName;
            allianceOutbound.CallStartDateTime = DateTime.Now;
            allianceOutbound.CreatedOn = DateTime.Now;
            string ticketID = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            allianceOutbound.TicketID = ticketID;
            if (ModelState.IsValid)
            {
                db.AllianceOutbounds.Add(allianceOutbound);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            PopulateDropdowns();
            return View(allianceOutbound);
        }

        // GET: AllianceOutbound/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AllianceOutbound allianceOutbound = db.AllianceOutbounds.Find(id);
            if (allianceOutbound == null)
            {
                return HttpNotFound();
            }

            PopulateDropdowns();

            //Get ward  from code
            var wards = db.WardVillages.Where(x => x.VillageTractCode == allianceOutbound.VillageTractTown)
            .Select(x => new SelectListItem
            {
                Value = x.Ward_PCode,
                Text = x.WardEnglishName
            }).ToList();

            ViewBag.Villages = new SelectList(wards, "Value", "Text", allianceOutbound.VillageWard);

            return View(allianceOutbound);
        }

        // POST: AllianceOutbound/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AllianceOutbound allianceOutbound)
        {
            allianceOutbound.ModifiedOn = DateTime.Now;
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(allianceOutbound.Prev_TicketId))
                    {
                        allianceOutbound.CallStartDateTime = DateTime.Now;
                        string ticketID = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                        allianceOutbound.TicketID = ticketID;

                        //Blank prev ticket for new 
                        allianceOutbound.Prev_TicketId = allianceOutbound.Prev_TicketId;

                        db.Entry(allianceOutbound).State = EntityState.Added;
                        db.AllianceOutbounds.Add(allianceOutbound);
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "New ticket created successfully!";
                    }
                    else
                    {
                        db.Entry(allianceOutbound).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["SuccessMessage"] = "Record updated successfully!";
                    }

                    //db.Entry(allianceOutbound).State = EntityState.Modified;
                    //db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
            }
            PopulateDropdowns();
            return View(allianceOutbound);
        }

        private void PopulateDropdowns()
        {
            ViewBag.CallObjectives = new SelectList(db.CallObjectives.OrderBy(o => o.Name).Select(c => new { Value = c.Id, Text = c.Name }), "Value", "Text");
            ViewBag.Regions = new SelectList(db.Regions.OrderBy(o => o.Name).Select(r => new { Value = r.Id, Text = r.Name }), "Value", "Text");
            ViewBag.Branches = new SelectList(db.AllianceBranches.OrderBy(o => o.BranchName).Select(b => new { Value = b.BranchCode, Text = b.BranchName }), "Value", "Text");
            ViewBag.Origins = new SelectList(db.Origins.OrderBy(o => o.Name).Select(o => new { Value = o.Id, Text = o.Name }), "Value", "Text");
            ViewBag.Products = new SelectList(db.Products.OrderBy(o => o.Name).Select(p => new { Value = p.Id, Text = p.Name }), "Value", "Text");
            ViewBag.TicketTypes = new SelectList(db.TicketTypes.OrderBy(o => o.Name).Select(t => new { Value = t.Id, Text = t.Name }), "Value", "Text");
            ViewBag.TicketStatuses = new SelectList(db.TicketStatuses.OrderBy(o => o.Name).Select(ts => new { Value = ts.Id, Text = ts.Name }), "Value", "Text");

            // Additional dropdowns for dependent fields
            ViewBag.States = new SelectList(db.States.OrderBy(o => o.StateName).Select(d => new { Value = d.StateCode, Text = d.StateName }), "Value", "Text");
            ViewBag.Districts = new SelectList(db.Districts.OrderBy(o => o.DistrictName).Select(d => new { Value = d.DistrictCode, Text = d.DistrictName }), "Value", "Text");
            ViewBag.Cities = new SelectList(db.Cities.OrderBy(o => o.CityName).Select(c => new { Value = c.CityCode, Text = c.CityName }), "Value", "Text");
            ViewBag.VillageTracts = new SelectList(db.VillageTracts.OrderBy(o => o.VillageTractName).Select(vt => new { Value = vt.VillageTractCode, Text = vt.VillageTractName }), "Value", "Text");
            //ViewBag.WardVillages = new SelectList(db.Villages.Select(v => new { Value = v.VillageCode, Text = v.VillageName }), "Value", "Text");
            ViewBag.Designations = new SelectList(db.AllianceDesignations.OrderBy(o => o.Designation).Select(d => new { Value = d.DesignationID, Text = d.Designation }), "Value", "Text");

            ViewBag.CitizenList = new SelectList(
                db.Citizen
                  .Select(c => new
                  {
                      Value = c.Code,
                      Text = c.Code + " - " + c.Reference
                  })
                  .ToList(),
                "Value",
                "Text"
            );

            ViewBag.StateDivisionList = new SelectList(
                db.StateDivisionNRC
                  .Select(s => new
                  {
                      Value = s.Id, // or s.StateDivisionCode if you prefer
                      Text = s.StateDivisionCode + " - " + s.StateDivisionName
                  })
                  .ToList(),
                "Value",
                "Text"
            );

            ViewBag.StateDivisionRegion = new SelectList(
                db.StateDivision
                  .Select(s => new
                  {
                      Value = s.StateCode,
                      Text = s.StateDivisionName
                  })
                  .ToList(),
                "Value",
                "Text"
            );

            //Empty list
            ViewBag.Villages = new SelectList(Enumerable.Empty<SelectListItem>());

        }

        // GET: AllianceOutbound/Index
        public ActionResult Index()
        {
            return View(new AllianceOutbound
            {
                AllianceOutboundList = new List<AllianceOutbound>()
            });
        }

        [HttpPost]
        public JsonResult GetPagedOutbound(int draw, int start, int length, AllianceOutbound allianceOutbound)
        {
            if (length <= 0)
            {
                length = 10;
            }

            var orderColumn = Request.Form["order[0][column]"];
            var orderDirection = Request.Form["order[0][dir]"];

            var totalRecords = db.AllianceOutbounds.Count();
            var query = ApplyOutboundFilters(db.AllianceOutbounds.AsNoTracking(), allianceOutbound);
            var filteredRecords = query.Count();

            IOrderedQueryable<AllianceOutbound> orderedQuery;
            if (orderColumn == "0")
            {
                // Sr. No maps to row order, which is based on the primary id.
                orderedQuery = string.Equals(orderDirection, "asc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderBy(o => o.AllianceOutboundId)
                    : query.OrderByDescending(o => o.AllianceOutboundId);
            }
            else
            {
                orderedQuery = query.OrderByDescending(o => o.AllianceOutboundId);
            }

            var outBound = orderedQuery.Skip(start).Take(length).ToList();

            PopulateOutboundLookupNames(outBound);

            var data = outBound.Select(item => new
            {
                item.AllianceOutboundId,
                item.TicketID,
                item.CallStatus,
                item.CallType,
                item.Priority,
                DateTime = item.DateTime.HasValue ? item.DateTime.Value.ToString("dd-MM-yyyy") : string.Empty,
                item.BranchName,
                item.CustomerNameEnglish,
                item.PrimaryMobileNumber,
                item.ProductInterestedName,
                item.DetailConversation,
                item.Prev_TicketId
            }).ToList();

            return Json(new
            {
                draw,
                recordsTotal = totalRecords,
                recordsFiltered = filteredRecords,
                data
            }, JsonRequestBehavior.AllowGet);
        }

        private IQueryable<AllianceOutbound> ApplyOutboundFilters(IQueryable<AllianceOutbound> query, AllianceOutbound allianceOutbound)
        {
            if (allianceOutbound == null)
            {
                return query;
            }

            if (allianceOutbound.FromDate != null)
                query = query.Where(tl => tl.DateTime != null && DbFunctions.TruncateTime(tl.DateTime) >= DbFunctions.TruncateTime(allianceOutbound.FromDate));
            if (allianceOutbound.ToDate != null)
                query = query.Where(tl => tl.DateTime != null && DbFunctions.TruncateTime(tl.DateTime) <= DbFunctions.TruncateTime(allianceOutbound.ToDate));

            if (!string.IsNullOrEmpty(allianceOutbound.CustomerCode))
                query = query.Where(tl => tl.CustomerCode == allianceOutbound.CustomerCode);
            if (!string.IsNullOrEmpty(allianceOutbound.CallType))
                query = query.Where(tl => tl.CallType == allianceOutbound.CallType);
            if (!string.IsNullOrEmpty(allianceOutbound.CallStatus))
                query = query.Where(tl => tl.CallStatus == allianceOutbound.CallStatus);
            if (!string.IsNullOrEmpty(allianceOutbound.PrimaryMobileNumberSearch))
                query = query.Where(tl => tl.PrimaryMobileNumber.Contains(allianceOutbound.PrimaryMobileNumberSearch));
            if (!string.IsNullOrEmpty(allianceOutbound.TicketID))
                query = query.Where(tl => tl.TicketID == allianceOutbound.TicketID);

            return query;
        }

        private void PopulateOutboundLookupNames(List<AllianceOutbound> outBound)
        {
            var branchCodes = outBound.Where(x => !string.IsNullOrEmpty(x.Branch)).Select(x => x.Branch).Distinct().ToList();
            var stateCodes = outBound.Where(x => !string.IsNullOrEmpty(x.StateRegion)).Select(x => x.StateRegion).Distinct().ToList();
            var districtCodes = outBound.Where(x => !string.IsNullOrEmpty(x.District)).Select(x => x.District).Distinct().ToList();
            var cityCodes = outBound.Where(x => !string.IsNullOrEmpty(x.CityTownship)).Select(x => x.CityTownship).Distinct().ToList();
            var villageTractCodes = outBound.Where(x => !string.IsNullOrEmpty(x.VillageTractTown)).Select(x => x.VillageTractTown).Distinct().ToList();
            var productIds = outBound
                .Select(x =>
                {
                    int parsed;
                    return int.TryParse(x.ProductInterested, out parsed) ? (int?)parsed : null;
                })
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            var branchMap = db.AllianceBranches
                .Where(x => branchCodes.Contains(x.BranchCode))
                .ToDictionary(x => x.BranchCode, x => x.BranchName);
            var stateMap = db.States
                .Where(x => stateCodes.Contains(x.StateCode))
                .ToDictionary(x => x.StateCode, x => x.StateName);
            var districtMap = db.Districts
                .Where(x => districtCodes.Contains(x.DistrictCode))
                .ToDictionary(x => x.DistrictCode, x => x.DistrictName);
            var cityMap = db.Cities
                .Where(x => cityCodes.Contains(x.CityCode))
                .ToDictionary(x => x.CityCode, x => x.CityName);
            var villageTractMap = db.VillageTracts
                .Where(x => villageTractCodes.Contains(x.VillageTractCode))
                .ToDictionary(x => x.VillageTractCode, x => x.VillageTractName);
            var productMap = db.Products
                .Where(x => productIds.Contains(x.Id))
                .ToDictionary(x => x.Id, x => x.Name);

            foreach (var item in outBound)
            {
                item.BranchName = !string.IsNullOrEmpty(item.Branch) && branchMap.ContainsKey(item.Branch) ? branchMap[item.Branch] : null;
                item.StateRegionName = !string.IsNullOrEmpty(item.StateRegion) && stateMap.ContainsKey(item.StateRegion) ? stateMap[item.StateRegion] : null;
                item.DistrictName = !string.IsNullOrEmpty(item.District) && districtMap.ContainsKey(item.District) ? districtMap[item.District] : null;
                item.CityTownshipName = !string.IsNullOrEmpty(item.CityTownship) && cityMap.ContainsKey(item.CityTownship) ? cityMap[item.CityTownship] : null;
                item.VillageTractTownName = !string.IsNullOrEmpty(item.VillageTractTown) && villageTractMap.ContainsKey(item.VillageTractTown) ? villageTractMap[item.VillageTractTown] : null;

                int productId;
                if (int.TryParse(item.ProductInterested, out productId) && productMap.ContainsKey(productId))
                {
                    item.ProductInterestedName = productMap[productId];
                }
            }
        }

        // GET: AllianceOutbound/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AllianceOutbound allianceOutnbound = db.AllianceOutbounds.Find(id);
            if (allianceOutnbound == null)
            {
                return HttpNotFound();
            }
            allianceOutnbound.Branch = db.AllianceBranches.Where(x => x.BranchCode == allianceOutnbound.Branch).FirstOrDefault()?.BranchName;

            allianceOutnbound.StateRegion = db.StateDivision.Where(x => x.StateCode == allianceOutnbound.StateRegion).FirstOrDefault()?.StateDivisionName;

            allianceOutnbound.District = db.Districts.Where(x => x.DistrictCode == allianceOutnbound.District).FirstOrDefault()?.DistrictName;

            allianceOutnbound.CityTownship = db.Cities.Where(x => x.CityCode == allianceOutnbound.CityTownship).FirstOrDefault()?.CityName;

            allianceOutnbound.VillageTractTown = db.VillageTracts.Where(x => x.VillageTractCode == allianceOutnbound.VillageTractTown).FirstOrDefault()?.VillageTractName;

            allianceOutnbound.VillageWard = db.WardVillages.Where(x => x.Ward_PCode == allianceOutnbound.VillageWard).FirstOrDefault()?.WardEnglishName;

            int.TryParse(allianceOutnbound.ProductInterested, out int productInterestedId);
            if (productInterestedId > 0)
            {
                allianceOutnbound.ProductInterested = db.Products.Where(x => x.Id == productInterestedId).FirstOrDefault()?.Name;
            }

            return View(allianceOutnbound);
        }

        // GET: AllianceOutbound/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AllianceOutbound allianceOutbound = db.AllianceOutbounds.Find(id);
            if (allianceOutbound == null)
            {
                return HttpNotFound();
            }
            return View(allianceOutbound);
        }

        //// POST: AllianceInbound/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    AllianceOutbound allianceOutbound = db.AllianceOutbounds.Find(id);
        //    db.AllianceInbounds.Remove(allianceOutbound);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}


        public ActionResult GetDistricts(string stateCode)
        {
            var districts = db.Districts
       .Where(d => d.StateCode == stateCode).OrderBy(o => o.DistrictName)
       .Select(d => new
       {
           DistrictCode = d.DistrictCode,
           DistrictName = d.DistrictName
       })
       .ToList();

            return Json(districts, JsonRequestBehavior.AllowGet);
        }

        // Get CityTownship by district id
        public ActionResult GetCityTownship(string districtCode)
        {
            var villages = db.Cities.Where(v => v.DistrictCode == districtCode).OrderBy(o => o.CityName)
                .Select(d => new
                {
                    CityCode = d.CityCode,
                    CityName = d.CityName
                })
                .ToList();
            return Json(villages, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVillageTrackTown(string cityCode)
        {
            var villagesTrack = db.VillageTracts.Where(v => v.CityCode == cityCode).OrderBy(o => o.VillageTractName)
                .Select(d => new
                {
                    VillageTractCode = d.VillageTractCode,
                    VillageTractName = d.VillageTractName
                })
                .ToList();
            return Json(villagesTrack, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWard(string villageTractCode)
        {
            var villagesTrack = db.WardVillages.Where(v => v.VillageTractCode == villageTractCode).OrderBy(o => o.WardEnglishName)
                .Select(d => new
                {
                    Ward_PCode = d.Ward_PCode,
                    WardEnglishName = d.WardEnglishName
                })
                .ToList();
            return Json(villagesTrack, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportAllianceOutboundCsv(AllianceOutbound allianceOutbound)
        {

            var data = customerRepository.GetDataOutboudAllExcel();

            if (allianceOutbound.FromDate != null)
                data = data.Where(tl => tl.DateTime != null && Convert.ToDateTime(tl.DateTime).Date >= Convert.ToDateTime(allianceOutbound.FromDate).Date).ToList();
            if (allianceOutbound.ToDate != null)
                data = data.Where(tl => tl.DateTime != null && Convert.ToDateTime(tl.DateTime).Date <= Convert.ToDateTime(allianceOutbound.ToDate).Date).ToList();

            if (!string.IsNullOrEmpty(allianceOutbound.CustomerCode))
                data = data.Where(tl => tl.CustomerCode == allianceOutbound.CustomerCode).ToList();
            if (!string.IsNullOrEmpty(allianceOutbound.CallType))
                data = data.Where(tl => tl.CallType == allianceOutbound.CallType).ToList();
            if (!string.IsNullOrEmpty(allianceOutbound.CallStatus))
                data = data.Where(tl => tl.CallStatus == allianceOutbound.CallStatus).ToList();
            if (!string.IsNullOrEmpty(allianceOutbound.PrimaryMobileNumberSearch))
                data = data.Where(tl => tl.PrimaryMobileNumber.Contains(allianceOutbound.PrimaryMobileNumberSearch)).ToList();
            if (!string.IsNullOrEmpty(allianceOutbound.TicketID))
                data = data.Where(tl => tl.TicketID == allianceOutbound.TicketID).ToList();

            var sb = new StringBuilder();

            // ✅ FIX: correct type
            var props = typeof(AllianceOutboundExcelModel).GetProperties();

            // Header
            sb.AppendLine(string.Join(",", props.Select(p => p.Name)));

            // Rows
            foreach (var row in data)
            {
                sb.AppendLine(string.Join(",", props.Select(p =>
                    $"\"{(p.GetValue(row)?.ToString() ?? string.Empty).Replace("\"", "\"\"")}\""
                )));
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=AllianceOutbound.csv");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.SuppressContent = false;

            return new EmptyResult();
        }

        /// <summary>
        /// NRC
        /// </summary>
        /// <param name="cityTownshipCode"></param>
        /// <returns></returns>
        public JsonResult GetTownshipsByStateDivision(int stateDivisionCode)
        {
            var townships = db.Township
                              .Where(t => t.StateDivisionCode == stateDivisionCode)
                              .Select(t => new
                              {
                                  Value = t.TownshipCode,
                                  Text = t.TownshipCode + " - " + t.TownshipName
                              })
                              .ToList();

            return Json(townships, JsonRequestBehavior.AllowGet);
        }

    }
}
