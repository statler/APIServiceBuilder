
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
    public interface IPhotoNcrService : IServiceBase<PhotoNcr>
    {
    }

    public partial class PhotoNcrService : AbstractService<PhotoNcr>, IPhotoNcrService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public PhotoNcrService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<PhotoNcr> GetEntitiesForProjectQry()
        {
            return _context.PhotoNcrs.Where(x => x.Ncr.ProjectId == ProjectId && x.Photo.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.PhotoNcrId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoNcrId ?? int.MinValue)).ForEachAsync(x => x.PhotoNcrId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoNcrId ?? int.MinValue)));
                ////Delete base objects

                _context.PhotoNcrs.RemoveRange(_context.PhotoNcrs.Where(x => lstIdsToDelete.Contains(x.PhotoNcrId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting PhotoNcr (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(PhotoNcr entity)
        {
            try
            {
                return (await _context.PhotoNcrs.CountAsync(x => x.NcrId == entity.NcrId &&
                  x.PhotoId == entity.PhotoId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(PhotoNcrService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(PhotoNcr entity)
        {
            try
            {
                if (entity.Ncr != null && entity.Photo != null) return entity.Ncr.ProjectId == ProjectId && entity.Photo.ProjectId == ProjectId;
                if ((await _context.Ncrs.Where(x => x.NcrId == entity.NcrId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Photos.Where(x => x.PhotoId == entity.PhotoId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(PhotoNcrService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.PhotoNcrs.CountAsync(x => lstIds.Contains(x.PhotoNcrId) && (x.Ncr.ProjectId != ProjectId || x.Photo.ProjectId != ProjectId)) == 0;
        }
    }
}