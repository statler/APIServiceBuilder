
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
    public interface IApprovalEmailService : IServiceBase<ApprovalEmail>
    {
    }

    public partial class ApprovalEmailService : AbstractService<ApprovalEmail>, IApprovalEmailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ApprovalEmailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalEmail> GetEntitiesForProjectQry()
        {
            return _context.ApprovalEmails.Where(x => x.Approval.ProjectId == ProjectId && x.EmailLog.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalEmailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalEmailId ?? int.MinValue)).ForEachAsync(x => x.ApprovalEmailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalEmailId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalEmails.RemoveRange(_context.ApprovalEmails.Where(x => lstIdsToDelete.Contains(x.ApprovalEmailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalEmail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalEmail entity)
        {
            try
            {
                return (await _context.ApprovalEmails.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.EmailLogId == entity.EmailLogId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalEmailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalEmail entity)
        {
            try
            {
                if (entity.Approval != null && entity.EmailLog != null) return entity.Approval.ProjectId == ProjectId && entity.EmailLog.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.EmailLogs.Where(x => x.EmailLogId == entity.EmailLogId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalEmailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalEmails.CountAsync(x => lstIds.Contains(x.ApprovalEmailId) && (x.Approval.ProjectId != ProjectId || x.EmailLog.ProjectId != ProjectId)) == 0;
        }
    }
}
