
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
    public interface IPhotoApprovalService : IServiceBase<PhotoApproval>
    {
    }

    public partial class PhotoApprovalService : AbstractService<PhotoApproval>, IPhotoApprovalService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public PhotoApprovalService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<PhotoApproval> GetEntitiesForProjectQry()
        {
            return _context.PhotoApprovals.Where(x => x.Approval.ProjectId == ProjectId && x.Photo.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.PhotoApprovalId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoApprovalId ?? int.MinValue)).ForEachAsync(x => x.PhotoApprovalId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoApprovalId ?? int.MinValue)));
                ////Delete base objects

                _context.PhotoApprovals.RemoveRange(_context.PhotoApprovals.Where(x => lstIdsToDelete.Contains(x.PhotoApprovalId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting PhotoApproval (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(PhotoApproval entity)
        {
            try
            {
                return (await _context.PhotoApprovals.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.PhotoId == entity.PhotoId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(PhotoApprovalService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(PhotoApproval entity)
        {
            try
            {
                if (entity.Approval != null && entity.Photo != null) return entity.Approval.ProjectId == ProjectId && entity.Photo.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Photos.Where(x => x.PhotoId == entity.PhotoId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(PhotoApprovalService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.PhotoApprovals.CountAsync(x => lstIds.Contains(x.PhotoApprovalId) && (x.Approval.ProjectId != ProjectId || x.Photo.ProjectId != ProjectId)) == 0;
        }
    }
}