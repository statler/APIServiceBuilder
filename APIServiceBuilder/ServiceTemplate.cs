using System;
using System.Collections.Generic;
using System.Text;

namespace APIServiceBuilder
{
    public static class ServiceTemplate
    {

        public static string TemplateSource = @"
using AutoMapper;
using AutoMapper.QueryableExtensions;
using cpModel.Models;
using cpDataORM.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cpDataServices.Models;
using cpDataServices.Exceptions;
using cpModel.Enums;

namespace cpDataServices.Services
{
    public interface I{{EntityName}}Service : IServiceBase<{{EntityName}}>
    {
    }

    public partial class {{EntityName}}Service : AbstractService<{{EntityName}}>, I{{EntityName}}Service
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public {{EntityName}}Service(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<{{EntityName}}> GetEntitiesForProjectQry()
        {
            return {{EntitiesForProject}};
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.{{PrimaryKeyId}} == Id).CountAsync();
            //if (c > 0) lstRelatedItems.Add($"" links ({c})"");

            return lstRelatedItems;
        }

        public async Task DeleteAsync(List<int> lstIdsToDelete, bool shouldCommit = true)
        {
            if (!(await CanDeleteAsync())) throw new AuthorizationException(""Delete | Admin permission is required to delete this record."");
            try
            {
                if (lstIdsToDelete.Contains(int.MinValue)) lstIdsToDelete.Remove(int.MinValue);
                ////Dereference links
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.{{PrimaryKeyId}} ?? int.MinValue)).ForEachAsync(x => x.{{PrimaryKeyId}} = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.{{PrimaryKeyId}} ?? int.MinValue)));
                ////Delete base objects

                _context.{{EntityName}}s.RemoveRange(_context.{{EntityName}}s.Where(x => lstIdsToDelete.Contains(x.{{PrimaryKeyId}})));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, ""Error deleting {{EntityName}} (DeleteAsync)"");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync({{EntityName}} entity)
        {
            try
            {
                return (await _context.{{EntityName}}s.CountAsync(x => {{IsEntityUniqueAsyncString}})) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, ""Error in IsEntityUniqueAsync({{EntityName}}Service)"");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync({{EntityName}} entity)
        {
            try
            {
                {{IsProjectValidAsyncString}}
            }
            catch (Exception ex)
            {
                Log.Error(ex, ""Error in IsProjectValidAsync({{ EntityName}}Service)"");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.{{EntityName}}s.CountAsync(x => lstIds.Contains(x.{{PrimaryKeyId}}) && ({{CheckIdsInCurrentProjectAsync}})) == 0;
        }
    }
}";

        public static string projectDeleteMethod = @"
    public async Task DeleteProjectAsync(int ProjectId)
    {
        //Generated using APIServiceBuilder            
        if (!await _userService.DoesUserHaveSubscriptionAdminThisProjectAsync())
                throw new AuthorizationException(""A user must be subscription administrator on the same subscription as the project to delete it."");
        using (var _tx = _context.Database.CurrentTransaction ?? await _context.Database.BeginTransactionAsync())
        {
            try
            {
                {{lstSetNull}}
                {{lstDelete}}
                await _context.Projects.Where(x => x.ProjectId == ProjectId).DeleteAsync();
                _tx.Commit();
                await _context.Users.Where(x => x.ProjectId == ProjectId && x.Username == null).DeleteAsync();
            }
            catch (Exception ex)
            {
                if (_context.Database.CurrentTransaction!=null) _tx.Rollback();
                Log.Error(ex, ""Error deleting Project(DeleteProjectAsync)"");
                throw;
            }
        }
    }";

        public static string dataSourceModel = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cpDataORM.Dtos;

namespace cpDataASP.ControllerModels
{
    public class {{EntityDto}}LoadResult
    {
        public List<{{EntityDto}}> data;
        public int totalCount;
        public int groupCount;
        public object[] summary;
    }
}";

        public static string serviceRegister = @"using cpDataServices.Services;
using cpDataServices.Services.TemplateServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace cpDataServices
{
    public static class ServiceCompiler
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            {{lstService}}
            return services;
        }
    }
}";

        public static List<string> LstScopedServices = new List<string>()
        {

        };

        public static List<DeleteEntityInfo> LstExplicitPriorityProjectDeleteClasses = new List<DeleteEntityInfo>() {
            new DeleteEntityInfo("LotItpTest","",1, "LotItpDetail"),
            new DeleteEntityInfo("ItpTesting","",1, "ItpDetail"),
            new DeleteEntityInfo("ItpDetail","",2, "Itp"),
            new DeleteEntityInfo("LotItpDetail","",2, "LotItp"),
            new DeleteEntityInfo("LotItpTest","",5, "LotItpDetail"),
            new DeleteEntityInfo("ItpTesting","",5, "ItpDetail")
        };


        public static List<string> LstExplicitExcludeProjectDeleteClasses = new List<string>() {
            "ApprovalCategory",
            "Division",
            "ForecastMethod",
            "Supplier",
            "IncidentType",
            "InsuranceType",
            "KeyDateType",
            "LotQtyStatus",
            "Permission",
            "Position",
            "Project",
            "RiskCategory",
            "Role",
            "RolePermission",
            "Subscriber",
            "SystemControl",
            "SystemSubscriberControl",
            "TemplateType",
            "Token",
            "User",
            "VrnStatus"
        };

        public static List<DeleteEntityInfo> LstExplicitOverrideProjectDeleteClasses = new List<DeleteEntityInfo>() {
        new DeleteEntityInfo("ChecklistUser","x => x.LotItp.Lot.ProjectId == ProjectId",5),
        new DeleteEntityInfo("ForecastCashflow","x => x.ForecastDetail.ForecastVersion.ProjectId == ProjectId",5),
        new DeleteEntityInfo("ForecastCostEstimate","x => x.ForecastDetail.ForecastVersion.ProjectId == ProjectId",5),
        new DeleteEntityInfo("LotItpQty","x => x.LotItp.Lot.ProjectId == ProjectId",5),
        new DeleteEntityInfo("RiskHazard","x => x.RiskActivityStep.RiskActivity.ProjectId == ProjectId",5),
        new DeleteEntityInfo("TestResult","x => x.TestRequestTest.TestRequest.ProjectId == ProjectId",5),
        new DeleteEntityInfo("WorkflowActionPoint","x => x.WorkflowAction.Workflow.ProjectId == ProjectId",5),
        new DeleteEntityInfo("WorkflowActionRole","x => x.WorkflowAction.Workflow.ProjectId == ProjectId",5),
        new DeleteEntityInfo("WorkflowActionUser","x => x.WorkflowAction.Workflow.ProjectId == ProjectId",5)
    };


        public static List<string> LstExplicitIncludeClasses = new List<string>() {
            "Approval",
            "ApprovalCategory",
            "ApprovalCc",
            "ApprovalEmail",
            "ApprovalItpDetail",
            "ApprovalLotItpDetail",
            "ApprovalLotQty",
            "ApprovalNcr",
            "ApprovalTo",
            "ApprovalWorkflow",
            "AreaCode",
            "Atp",
            "AtpLot",
"CnApproval",
"CnControlledDoc",
"CnEmail",
"CnIncident",
"CnInstruction",
"CnItp",
"CnLot",
"CnNotice",
"CnPhoto",
"CnResponse",
"CnTo",
"CnVariation",
            "ContractNotice",
            "ControlLine",
            "ControlLinePoint",
            "CostCode",
            "EmailLog",
            "FileStoreDoc",
            "FsApproval",
            "FsDoc",
            "FsEmail",
            "FsLot",
            "FsNcr",
            "FsNotice",
            "FsTestReq",
            "FsVariation",
            "FsWorkflowLog",
            "ImageLayer",
            "ImageLayerPoint",
            "Incident",
            "IncidentPerson",
            "IncidentType",
            "Instruction",
            "Itp",
            "ItpDetail",
            "ItpScheduleItem",
            "ItpTesting",
            "KeyDate",
            "KeyDateType",
            "Lot",
            "LotCoordinate",
            "LotItp",
            "LotItpDetail",
            "LotItpTest",
            "LotItpQty",
            "LotQuantity",
            "LotRelation",
            "LotTag",
            "LotUser",
            "Ncr",
            "NcrLot",
            "Photo",
            "PhotoApproval",
            "PhotoChecklistItem",
            "PhotoLot",
            "PhotoNcr",
            "PhotoVariation",
            "ProgressClaimDetail",
            "ProgressClaimSnapshot",
            "ProgressClaimVersion",
            "Project",
            "PurchaseOrder",
            "PurchaseOrderDetail",
            "ReportPeriod",
            "Resource",
            "Role",
            "RolePermission",
            "ScheduleItem",
            "SiteDiary",
            "SiteDiaryCost",
            "SiteDiaryCostCode",
            "Subscriber",
            "Supplier",
            "SupplierLink",
            "TagCode",
            "Template",
            "TemplateType",
            "TestCoordinate",
            "TestMethod",
            "TestPropertyGroup",
            "TestPropertyItem",
            "TestReqEmail",
            "TestRequest",
            "TestRequestProperty",
            "TestRequestTest",
            "TestResult",
            "TestResultField",
            "UserRole",
            "UserInvite",
            "Unit",
            "Variation",
            "VariationEstimate",
            "VariationLot",
            "VariationSchedule",
            "VrnStatus",
            "VrnWaypoint",
            "WorkSchedule",
            "WorkType",
            "Workflow",
            "WorkflowAction",
            "WorkflowActionPoint",
            "WorkflowActionRole",
            "WorkflowActionUser",
            "WorkflowLog",
            "WorkflowStep",
            "ContractNoticeTemplate",
"CustomRegister",
"CustomRegisterItem",
"FsPurchaseOrder",
"LotCustomRegItem",
"NcrRevision",
"PoEmail",
"ProjectReport",
"Subcontractor",
"SubcontractorLot",
"SubcontractorUser"
        };
    }
}
