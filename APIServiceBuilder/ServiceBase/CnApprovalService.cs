
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
    public interface ICnApprovalService : IServiceBase<CnApproval>
    {
    }

    public partial class CnApprovalService : AbstractService<CnApproval>, ICnApprovalService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnApprovalService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnApproval> GetEntitiesForProjectQry()
        {
            return _context.CnApprovals.Where(x => x.Approval.ProjectId == ProjectId && x.ContractNotice.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnApprovalId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnApprovalId ?? int.MinValue)).ForEachAsync(x => x.CnApprovalId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnApprovalId ?? int.MinValue)));
                ////Delete base objects

                _context.CnApprovals.RemoveRange(_context.CnApprovals.Where(x => lstIdsToDelete.Contains(x.CnApprovalId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnApproval (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnApproval entity)
        {
            try
            {
                return (await _context.CnApprovals.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.CnId == entity.CnId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnApprovalService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnApproval entity)
        {
            try
            {
                if (entity.Approval != null && entity.ContractNotice != null) return entity.Approval.ProjectId == ProjectId && entity.ContractNotice.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnApprovalService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnApprovals.CountAsync(x => lstIds.Contains(x.CnApprovalId) && (x.Approval.ProjectId != ProjectId || x.ContractNotice.ProjectId != ProjectId)) == 0;
        }
    }
}
