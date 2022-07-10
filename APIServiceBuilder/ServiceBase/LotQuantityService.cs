
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
    public interface ILotQuantityService : IServiceBase<LotQuantity>
    {
    }

    public partial class LotQuantityService : AbstractService<LotQuantity>, ILotQuantityService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotQuantityService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotQuantity> GetEntitiesForProjectQry()
        {
            return _context.LotQuantitys.Where(x => x.Lot.ProjectId == ProjectId && x.Ncr.ProjectId == ProjectId && x.ScheduleItem.ProjectId == ProjectId && x.Variation.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.QuantityId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.QuantityId ?? int.MinValue)).ForEachAsync(x => x.QuantityId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.QuantityId ?? int.MinValue)));
                ////Delete base objects

                _context.LotQuantitys.RemoveRange(_context.LotQuantitys.Where(x => lstIdsToDelete.Contains(x.QuantityId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotQuantity (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotQuantity entity)
        {
            try
            {
                return (await _context.LotQuantitys.CountAsync(x => x.LotId == entity.LotId &&
                  x.NcrId == entity.NcrId &&
                  x.ScheduleId == entity.ScheduleId &&
                  x.VariationId == entity.VariationId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotQuantityService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotQuantity entity)
        {
            try
            {
                if (entity.Lot != null && entity.Ncr != null && entity.ScheduleItem != null && entity.Variation != null) return entity.Lot.ProjectId == ProjectId && entity.Ncr.ProjectId == ProjectId && entity.ScheduleItem.ProjectId == ProjectId && entity.Variation.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Ncrs.Where(x => x.NcrId == entity.NcrId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ScheduleItems.Where(x => x.ScheduleId == entity.ScheduleId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Variations.Where(x => x.VariationId == entity.VariationId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotQuantityService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotQuantitys.CountAsync(x => lstIds.Contains(x.QuantityId) && (x.Lot.ProjectId != ProjectId || x.Ncr.ProjectId != ProjectId || x.ScheduleItem.ProjectId != ProjectId || x.Variation.ProjectId != ProjectId)) == 0;
        }
    }
}
