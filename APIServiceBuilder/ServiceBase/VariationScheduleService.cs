
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
    public interface IVariationScheduleService : IServiceBase<VariationSchedule>
    {
    }

    public partial class VariationScheduleService : AbstractService<VariationSchedule>, IVariationScheduleService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public VariationScheduleService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<VariationSchedule> GetEntitiesForProjectQry()
        {
            return _context.VariationSchedules.Where(x => x.ScheduleItem.ProjectId == ProjectId && x.Variation.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.VariationScheduleId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.VariationScheduleId ?? int.MinValue)).ForEachAsync(x => x.VariationScheduleId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.VariationScheduleId ?? int.MinValue)));
                ////Delete base objects

                _context.VariationSchedules.RemoveRange(_context.VariationSchedules.Where(x => lstIdsToDelete.Contains(x.VariationScheduleId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting VariationSchedule (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(VariationSchedule entity)
        {
            try
            {
                return (await _context.VariationSchedules.CountAsync(x => x.ScheduleId == entity.ScheduleId &&
                  x.VariationId == entity.VariationId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(VariationScheduleService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(VariationSchedule entity)
        {
            try
            {
                if (entity.ScheduleItem != null && entity.Variation != null) return entity.ScheduleItem.ProjectId == ProjectId && entity.Variation.ProjectId == ProjectId;
                if ((await _context.ScheduleItems.Where(x => x.ScheduleId == entity.ScheduleId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Variations.Where(x => x.VariationId == entity.VariationId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(VariationScheduleService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.VariationSchedules.CountAsync(x => lstIds.Contains(x.VariationScheduleId) && (x.ScheduleItem.ProjectId != ProjectId || x.Variation.ProjectId != ProjectId)) == 0;
        }
    }
}
