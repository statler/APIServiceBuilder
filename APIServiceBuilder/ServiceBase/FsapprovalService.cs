
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
    public interface IFsApprovalService : IServiceBase<FsApproval>
    {
    }

    public partial class FsApprovalService : AbstractService<FsApproval>, IFsApprovalService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public FsApprovalService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<FsApproval> GetEntitiesForProjectQry()
        {
            return _context.FsApprovals.Where(x => x.Approval.ProjectId == ProjectId && x.FileStoreDoc.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.FsApprovalId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsApprovalId ?? int.MinValue)).ForEachAsync(x => x.FsApprovalId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsApprovalId ?? int.MinValue)));
                ////Delete base objects

                _context.FsApprovals.RemoveRange(_context.FsApprovals.Where(x => lstIdsToDelete.Contains(x.FsApprovalId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting FsApproval (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(FsApproval entity)
        {
            try
            {
                return (await _context.FsApprovals.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.FsId == entity.FsId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(FsApprovalService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(FsApproval entity)
        {
            try
            {
                if (entity.Approval != null && entity.FileStoreDoc != null) return entity.Approval.ProjectId == ProjectId && entity.FileStoreDoc.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.FileStoreDocs.Where(x => x.FileStoreDocId == entity.FsId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(FsApprovalService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.FsApprovals.CountAsync(x => lstIds.Contains(x.FsApprovalId) && (x.Approval.ProjectId != ProjectId || x.FileStoreDoc.ProjectId != ProjectId)) == 0;
        }
    }
}
