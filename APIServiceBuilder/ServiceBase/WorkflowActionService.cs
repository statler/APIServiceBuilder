
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
    public interface IWorkflowActionService : IServiceBase<WorkflowAction>
    {
    }

    public partial class WorkflowActionService : AbstractService<WorkflowAction>, IWorkflowActionService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public WorkflowActionService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<WorkflowAction> GetEntitiesForProjectQry()
        {
            return _context.WorkflowActions.Where(x => x.Workflow.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.WorkflowActionId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.WorkflowActionId ?? int.MinValue)).ForEachAsync(x => x.WorkflowActionId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.WorkflowActionId ?? int.MinValue)));
                ////Delete base objects

                _context.WorkflowActions.RemoveRange(_context.WorkflowActions.Where(x => lstIdsToDelete.Contains(x.WorkflowActionId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting WorkflowAction (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(WorkflowAction entity)
        {
            try
            {
                return (await _context.WorkflowActions.CountAsync(x => x.WorkflowId == entity.WorkflowId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(WorkflowActionService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(WorkflowAction entity)
        {
            try
            {
                if (entity.Workflow != null) return entity.Workflow.ProjectId == ProjectId;
                if ((await _context.Workflows.Where(x => x.WorkflowId == entity.WorkflowId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(WorkflowActionService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.WorkflowActions.CountAsync(x => lstIds.Contains(x.WorkflowActionId) && (x.Workflow.ProjectId != ProjectId)) == 0;
        }
    }
}