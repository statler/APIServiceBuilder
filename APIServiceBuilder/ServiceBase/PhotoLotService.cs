
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
    public interface IPhotoLotService : IServiceBase<PhotoLot>
    {
    }

    public partial class PhotoLotService : AbstractService<PhotoLot>, IPhotoLotService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public PhotoLotService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<PhotoLot> GetEntitiesForProjectQry()
        {
            return _context.PhotoLots.Where(x => x.Lot.ProjectId == ProjectId && x.Photo.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.PhotoLotId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoLotId ?? int.MinValue)).ForEachAsync(x => x.PhotoLotId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PhotoLotId ?? int.MinValue)));
                ////Delete base objects

                _context.PhotoLots.RemoveRange(_context.PhotoLots.Where(x => lstIdsToDelete.Contains(x.PhotoLotId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting PhotoLot (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(PhotoLot entity)
        {
            try
            {
                return (await _context.PhotoLots.CountAsync(x => x.LotId == entity.LotId &&
                  x.PhotoId == entity.PhotoId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(PhotoLotService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(PhotoLot entity)
        {
            try
            {
                if (entity.Lot != null && entity.Photo != null) return entity.Lot.ProjectId == ProjectId && entity.Photo.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Photos.Where(x => x.PhotoId == entity.PhotoId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(PhotoLotService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.PhotoLots.CountAsync(x => lstIds.Contains(x.PhotoLotId) && (x.Lot.ProjectId != ProjectId || x.Photo.ProjectId != ProjectId)) == 0;
        }
    }
}
