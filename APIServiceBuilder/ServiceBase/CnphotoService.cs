
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
    public interface ICnPhotoService : IServiceBase<CnPhoto>
    {
    }

    public partial class CnPhotoService : AbstractService<CnPhoto>, ICnPhotoService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnPhotoService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnPhoto> GetEntitiesForProjectQry()
        {
            return _context.CnPhotos.Where(x => x.ContractNotice.ProjectId == ProjectId && x.Photo.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnPhotoId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnPhotoId ?? int.MinValue)).ForEachAsync(x => x.CnPhotoId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnPhotoId ?? int.MinValue)));
                ////Delete base objects

                _context.CnPhotos.RemoveRange(_context.CnPhotos.Where(x => lstIdsToDelete.Contains(x.CnPhotoId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnPhoto (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnPhoto entity)
        {
            try
            {
                return (await _context.CnPhotos.CountAsync(x => x.CnId == entity.CnId &&
                  x.PhotoId == entity.PhotoId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnPhotoService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnPhoto entity)
        {
            try
            {
                if (entity.ContractNotice != null && entity.Photo != null) return entity.ContractNotice.ProjectId == ProjectId && entity.Photo.ProjectId == ProjectId;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Photos.Where(x => x.PhotoId == entity.PhotoId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnPhotoService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnPhotos.CountAsync(x => lstIds.Contains(x.CnPhotoId) && (x.ContractNotice.ProjectId != ProjectId || x.Photo.ProjectId != ProjectId)) == 0;
        }
    }
}