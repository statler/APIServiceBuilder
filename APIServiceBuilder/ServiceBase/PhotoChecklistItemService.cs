
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
    public interface IPhotoChecklistItemService : IServiceBase<PhotoChecklistItem>
    {
    }

    public partial class PhotoChecklistItemService : AbstractService<PhotoChecklistItem>, IPhotoChecklistItemService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public PhotoChecklistItemService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<PhotoChecklistItem> GetEntitiesForProjectQry()
        {
            return _context.PhotoChecklistItems.Where(x => x.Photo.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.PhotoCheckId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoCheckId ?? int.MinValue)).ForEachAsync(x => x.PhotoCheckId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoCheckId ?? int.MinValue)));
                ////Delete base objects

                _context.PhotoChecklistItems.RemoveRange(_context.PhotoChecklistItems.Where(x => lstIdsToDelete.Contains(x.PhotoCheckId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting PhotoChecklistItem (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(PhotoChecklistItem entity)
        {
            try
            {
                return (await _context.PhotoChecklistItems.CountAsync(x => x.PhotoId == entity.PhotoId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(PhotoChecklistItemService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(PhotoChecklistItem entity)
        {
            try
            {
                if (entity.Photo != null) return entity.Photo.ProjectId == ProjectId;
                if ((await _context.Photos.Where(x => x.PhotoId == entity.PhotoId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(PhotoChecklistItemService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.PhotoChecklistItems.CountAsync(x => lstIds.Contains(x.PhotoCheckId) && (x.Photo.ProjectId != ProjectId)) == 0;
        }
    }
}