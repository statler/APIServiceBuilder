
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
    public interface IVariationLotService : IServiceBase<VariationLot>
    {
    }

    public partial class VariationLotService : AbstractService<VariationLot>, IVariationLotService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public VariationLotService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<VariationLot> GetEntitiesForProjectQry()
        {
            return _context.VariationLots.Where(x => x.Lot.ProjectId == ProjectId && x.Variation.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.VariationLotId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.VariationLotId ?? int.MinValue)).ForEachAsync(x => x.VariationLotId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.VariationLotId ?? int.MinValue)));
                ////Delete base objects

                _context.VariationLots.RemoveRange(_context.VariationLots.Where(x => lstIdsToDelete.Contains(x.VariationLotId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting VariationLot (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(VariationLot entity)
        {
            try
            {
                return (await _context.VariationLots.CountAsync(x => x.LotId == entity.LotId &&
                  x.VariationId == entity.VariationId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(VariationLotService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(VariationLot entity)
        {
            try
            {
                if (entity.Lot != null && entity.Variation != null) return entity.Lot.ProjectId == ProjectId && entity.Variation.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Variations.Where(x => x.VariationId == entity.VariationId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(VariationLotService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.VariationLots.CountAsync(x => lstIds.Contains(x.VariationLotId) && (x.Lot.ProjectId != ProjectId || x.Variation.ProjectId != ProjectId)) == 0;
        }
    }
}