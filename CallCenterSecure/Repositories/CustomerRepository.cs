using CallCenter.Models;
using CallCenterSecure.Models;
using CallCenterSecure.Models.Inbound;
using CallCenterSecure.Models.Outbound;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallCenterSecure.Repositories
{
    public class CustomerRepository
    {
        private readonly string _conn;
        public CustomerRepository()
        {
            _conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }

        public IEnumerable<AllianceInbound> GetData(int id)
        {
            using (SqlConnection con = new SqlConnection(_conn))
            {
                con.Open();

                string sql = @"
                select ai.AllianceInboundId,ai.DateTime,ai.TicketID,
                co.Name AS CallObjective,reg.Region AS Region,brn.BranchName Branch,ai.ClientName,ai.PhoneNumber,
                ai.Address,
                o.Name as Origin,
                p.Name AS [Product], ai.DetailConversation, ai.Response,
                tt.Name AS TicketType,ai.FollowUpCallBackSchedule,
                ts.Name as TicketStatus, ai.AgentName,

                ai.Cmp_CustomerCode,ai.Cmp_CustomerName,ai.Cmp_PhoneNumber,
                rbrb.Region AS Cmp_Region,rbrbb.BranchName AS Cmp_Branch,dsc.Designation AS Cmp_ComplainToDesignation,ai.Cmp_ComplainTo,
                dscc.Designation AS cmp_complainCCDesignation,
                ai.Cmp_ComplainCC,ai.Cmp_NatureOfComplaint,ai.Cmp_CaseDetail,ai.Cmp_ComplainStatus,ai.FileName,
                
                ai.Lead_CustomerName,lb.[Name] AS Lead_Branch,sd.StateDivisionName AS Lead_StateRegion,
                ds.DistrictName AS Lead_District,lc.CityName AS Lead_CityTownship,
                vt.VillageTractName AS Lead_VillageTractTown,ai.Lead_VillageWard,ai.Lead_Address,ai.Lead_PrimaryMobileNumber,ai.Lead_AlternateMobileNumber,
                lp.Name AS Lead_ProductInterested,ai.Lead_Latitude,ai.Lead_Longitude,ai.Lead_NRC,ai.Lead_DateOfBirth,ai.Lead_Age,
                ai.Lead_Gender,ai.Lead_MaritalStatus,ai.Lead_SpouseName,ai.Lead_ClientOfficerName,ai.Lead_LeadStatus,
                ai.Prev_TicketId,cds.Description as Cmp_Designation, ncs.ComplaintsDescrption as Cmp_NatureOfComplaint,

                ndisp.Name AS Na_Disposition, ai.NRC, cmpdisp.Name AS cmp_Disposition
                from AllianceInbounds ai
                LEFT JOIN CallObjectives co on co.Id=ai.CallObjective
                LEFT JOIN Products p on p.Id=ai.[Product]
                LEFT JOIN Origins o on o.Id=ai.Origin
                LEFT JOIN TicketTypes tt on tt.Id=ai.TicketType
                LEFT JOIN TicketStatus ts on ts.Id=ai.TicketStatus
                LEFT JOIN Designations dsc on dsc.DesignationId=TRY_CAST(ai.Cmp_ComplainToDesignation AS INT)
                LEFT JOIN Designations dscc on dscc.DesignationId=ai.cmp_complainCCDesignation

                LEFT JOIN Branches lb on ai.Lead_Branch=lb.Id
                LEFT JOIN StateDivisions sd on ai.Lead_StateRegion=sd.StateCode
                LEFT JOIN Products lp on ai.Lead_ProductInterested=lp.Id
                LEFT JOIN Districts ds on ai.Lead_District=ds.DistrictCode
				LEFT JOIN ComplaintDesignations cds on cds.ComplaintDesignationId = ai.Cmp_Designation
				LEFT JOIN NatureOfComplaints ncs on ncs.ComplaintId = ai.Cmp_NatureOfComplaint 
                LEFT JOIN NaDispositions ndisp ON ndisp.Id=ai.Na_Disposition
                LEFT JOIN CmpDispositions cmpdisp ON cmpdisp.Id=ai.cmp_Disposition
                LEFT JOIN RegionBranches reg ON reg.Id=TRY_CAST(ai.Region AS INT)
                LEFT JOIN RegionBranches brn on brn.Id=TRY_CAST(ai.branch AS INT)
                LEFT JOIN RegionBranches rbrb on rbrb.id=ai.Cmp_Region
                LEFT JOIN RegionBranches rbrbb on rbrbb.id=ai.Cmp_Branch
                LEFT JOIN Cities lc on lc.CityCode=ai.Lead_CityTownship
                LEFT JOIN VillageTracts vt on vt.VillageTractCode=ai.Lead_VillageTractTown                
                where ai.AllianceInboundId=@id;";

                return con.Query<AllianceInbound>(sql, new { Id = id });
            }
        }
        public IEnumerable<InboundGridData> GetDataAll()
        {
            using (SqlConnection con = new SqlConnection(_conn))
            {
                con.Open();

                string sql = @"
                select ai.AllianceInboundId,
                ai.TicketID,
                tt.Name AS TicketType,
                ai.TicketType AS TicketTypeId,
                ts.Name as TicketStatus,
                ai.TicketStatus AS TicketStatusId,
                ai.DateTime,
                co.Name AS CallObjective,
                ab.BranchName AS Branch,
                ai.ClientName,
                ai.PhoneNumber,
                p.Name AS [Product],
                ai.Response,
                ai.FollowUpCallBackSchedule,
                ai.Prev_TicketId
                
                from AllianceInbounds ai
                LEFT JOIN CallObjectives co on co.Id=ai.CallObjective
                LEFT JOIN Products p on p.Id=ai.[Product]                
                LEFT JOIN TicketTypes tt on tt.Id=ai.TicketType
                LEFT JOIN TicketStatus ts on ts.Id=ai.TicketStatus
                LEFT JOIN RegionBranches ab on ab.Id=ai.Branch
                ORDER BY ai.AllianceInboundId DESC;";

                return con.Query<InboundGridData>(sql);
            }
        }

        public IEnumerable<AllianceInboundExcelModel> GetDataAllExcel()
        {
            using (SqlConnection con = new SqlConnection(_conn))
            {
                con.Open();

                string sql = @"
                select ai.AllianceInboundId,ai.DateTime,ai.TicketID,
                co.Name AS CallObjective,rb.Region AS Region,rbb.BranchName AS Branch,ai.ClientName,ai.PhoneNumber,
                ai.Address,
                o.Name as Origin,
                p.Name AS [Product], ai.DetailConversation, ai.Response,tt.id AS TicketTypeId,
                tt.Name AS TicketType,ai.FollowUpCallBackSchedule,
                ts.Name as TicketStatus, ai.AgentName,ts.Id AS TicketStatusId,

                ai.Cmp_CustomerCode,ai.Cmp_CustomerName,ai.Cmp_PhoneNumber,
                ai.Cmp_Region,ai.Cmp_Branch,dsc.Designation AS Cmp_ComplainToDesignation,ai.Cmp_ComplainTo,
                dscc.Designation AS cmp_complainCCDesignation,ai.cmp_Designation,ai.ComplainResolve,
                ai.Cmp_ComplainCC,ai.Cmp_NatureOfComplaint,ai.Cmp_CaseDetail,ai.Cmp_ComplainStatus,ai.FileName,
                
                ai.Lead_CustomerName,lb.[Name] AS Lead_Branch,sd.StateDivisionName AS Lead_StateRegion,
                ds.DistrictName AS Lead_District,ct.CityName AS Lead_CityTownship,
                vt.VillageTractName AS VillageTractTown,ai.Lead_VillageWard,ai.Lead_Address,ai.Lead_PrimaryMobileNumber,ai.Lead_AlternateMobileNumber,
                lp.Name AS Lead_ProductInterested,ai.Lead_Latitude,ai.Lead_Longitude,ai.Lead_NRC,ai.Lead_DateOfBirth,ai.Lead_Age,
                ai.Lead_Gender,ai.Lead_MaritalStatus,ai.Lead_SpouseName,ai.Lead_ClientOfficerName,ai.Lead_LeadStatus,
                ai.Prev_TicketId,nd.Name AS Na_Disposition
                from AllianceInbounds ai
                LEFT JOIN CallObjectives co on co.Id=ai.CallObjective
                LEFT JOIN Products p on p.Id=ai.[Product]
                LEFT JOIN Origins o on o.Id=ai.Origin
                LEFT JOIN TicketTypes tt on tt.Id=ai.TicketType
                LEFT JOIN TicketStatus ts on ts.Id=ai.TicketStatus
                LEFT JOIN Designations dsc on dsc.DesignationId=TRY_CAST(ai.Cmp_ComplainToDesignation AS INT)
                LEFT JOIN Designations dscc on dscc.DesignationId=ai.cmp_complainCCDesignation

                LEFT JOIN Branches lb on ai.Lead_Branch=lb.Id
                LEFT JOIN StateDivisions sd on ai.Lead_StateRegion=sd.StateCode
                LEFT JOIN Products lp on ai.Lead_ProductInterested=lp.Id
                LEFT JOIN Districts ds on ds.DistrictCode=ai.Lead_District
                LEFT JOIN Cities ct on ct.CityCode =ai.Lead_CityTownship
                LEFT JOIN NaDispositions nd on nd.Id=ai.Na_Disposition
                LEFT JOIN AllianceBranches ab on ab.Branchcode=ai.Branch
                LEFT JOIN RegionBranches rb on rb.Id = ai.Region
                LEFT JOIN RegionBranches rbb on rbb.id=ai.Branch
                LEFT join VillageTracts vt on vt.VillageTractCode=ai.Lead_VillageTractTown
                ORDER BY ai.AllianceInboundId DESC";

                return con.Query<AllianceInboundExcelModel>(sql);
            }
        }
        public bool DeletePreviousData()
        {
            using (SqlConnection con = new SqlConnection(_conn))
            {
                con.Open();

                string sql = @"
                TRUNCATE TABLE CustomerLoanInformations;";

                return con.Execute(sql) > 0 ? true : false;
            }
        }

        public IEnumerable<AllianceOutboundExcelModel> GetDataOutboudAllExcel()
        {
            using (SqlConnection con = new SqlConnection(_conn))
            {
                con.Open();

                string sql = @"Select ao.AllianceOutboundId,ao.DateTime,ao.TicketID,ao.CustomerCode,ao.CustomerNameEnglish,
                        b.[Name] AS Branch,sd.StateDivisionName AS StateRegion,COALESCE(ds.DistrictName,ao.District) AS District,c.CityName AS CityTownship,
                        vt.VillageTractName AS VillageTractTown,

                       wv.WardEnglishName AS VillageWard,ao.PrimaryMobileNumber, p.[Name] AS ProductInterested,
                        ao.Latitude,ao.Longitude,ao.NRC,ao.DateOfBirth,ao.Age,ao.Gender,ao.MaritalStatus,ao.SpouseName,ao.[Priority],
                        ao.ClientOfficerName,ao.CallStatus,ao.CallType,ao.AgentName,ao.Prev_TicketId,ao.DetailConversation

                        from AllianceOutbounds ao
                        Left join Branches b on ao.Branch=b.Id
                        Left join Regions rg on rg.Id=TRY_CAST(ao.StateRegion AS INT)
                        Left join Districts ds on ds.DistrictCode=ao.District
                        LEFT join Townships ts on ts.TownshipCode=ao.CityTownship
                        LEFT join VillageTracts vt on vt.VillageTractCode=ao.VillageTractTown
                        LEFT join Products p on p.Id=ao.ProductInterested
                        LEFT join StateDivisions sd on sd.StateCode=ao.StateRegion
                        LEFT JOIN Cities c on c.CityCode=ao.CityTownship
                        LEFT JOIN WardVillages wv on wv.Ward_PCode=ao.VillageWard
                        ORDER By ao.AllianceOutboundId DESC";


                return con.Query<AllianceOutboundExcelModel>(sql);
            }
        }
    }
}
