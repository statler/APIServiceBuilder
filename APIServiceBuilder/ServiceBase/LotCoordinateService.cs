
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
    public interface ILotCoordinateService : IServiceBase<LotCoordinate>
    {
    }

    public partial class LotCoordinateService : AbstractService<LotCoordinate>, ILotCoordinateService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotCoordinateService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotCoordinate> GetEntitiesForProjectQry()
        {
            return _context.LotCoordinates.Where(x => x.Lot.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.LotCoordinatesId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotCoordinatesId ?? int.MinValue)).ForEachAsync(x => x.LotCoordinatesId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotCoordinatesId ?? int.MinValue)));
                ////Delete base objects

                _context.LotCoordinates.RemoveRange(_context.LotCoordinates.Where(x => lstIdsToDelete.Contains(x.LotCoordinatesId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotCoordinate (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotCoordinate entity)
        {
            try
            {
                return (await _context.LotCoordinates.CountAsync(x => x.LotId == entity.LotId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotCoordinateService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotCoordinate entity)
        {
            try
            {
                if (entity.Lot != null) return entity.Lot.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotCoordinateService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotCoordinates.CountAsync(x => lstIds.Contains(x.LotCoordinatesId) && (x.Lot.ProjectId != ProjectId)) == 0;
        }
    }
}