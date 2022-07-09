
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
    public interface IWorkflowStepService : IServiceBase<WorkflowStep>
    {
    }

    public partial class WorkflowStepService : AbstractService<WorkflowStep>, IWorkflowStepService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public WorkflowStepService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<WorkflowStep> GetEntitiesForProjectQry()
        {
            return _context.WorkflowSteps.Where(x => x.Workflow.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.WorkflowStepId == Id).CountAsync();
            //if (c > 0) lstRelatedItems.Add($" links ({c})");

            return lstRelatedItems;
        }

        public async Task DeleteAsync(List<int> lstIdsToDelete, bool shouldCommit = true)
        {
            if (!(await CanDeleteAsync())) throw new AuthorizationException("Delete | Admin permission is required to delete this record.");
            try
            {
                if (lstIdsToDelete.Contains(int.MinValue)) lstIdsToDelete.Remove(int.MinValue);
                ////Dereference links
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.WorkflowStepId ?? int.MinValue)).ForEachAsync(x => x.WorkflowStepId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.WorkflowStepId ?? int.MinValue)));
                ////Delete base objects

                _context.WorkflowSteps.RemoveRange(_context.WorkflowSteps.Where(x => lstIdsToDelete.Contains(x.WorkflowStepId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting WorkflowStep (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(WorkflowStep entity)
        {
            try
            {
                return (await _context.WorkflowSteps.CountAsync(x => x.WorkflowId == entity.WorkflowId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(WorkflowStepService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(WorkflowStep entity)
        {
            try
            {
                if (entity.Workflow != null) return entity.Workflow.ProjectId == ProjectId;
                if ((await _context.Workflows.Where(x => x.WorkflowId == entity.WorkflowId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(WorkflowStepService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.WorkflowSteps.CountAsync(x => lstIds.Contains(x.WorkflowStepId) && (x.Workflow.ProjectId != ProjectId)) == 0;
        }
    }
}