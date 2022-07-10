
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
    public interface IWorkScheduleService : IServiceBase<WorkSchedule>
    {
    }

    public partial class WorkScheduleService : AbstractService<WorkSchedule>, IWorkScheduleService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public WorkScheduleService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<WorkSchedule> GetEntitiesForProjectQry()
        {
            return _context.WorkSchedules.Where(x => x.ScheduleItem.ProjectId == ProjectId && x.WorkType.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.WorkScheduleId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.WorkScheduleId ?? int.MinValue)).ForEachAsync(x => x.WorkScheduleId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.WorkScheduleId ?? int.MinValue)));
                ////Delete base objects

                _context.WorkSchedules.RemoveRange(_context.WorkSchedules.Where(x => lstIdsToDelete.Contains(x.WorkScheduleId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting WorkSchedule (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(WorkSchedule entity)
        {
            try
            {
                return (await _context.WorkSchedules.CountAsync(x => x.ScheduleId == entity.ScheduleId &&
                  x.WorkId == entity.WorkId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(WorkScheduleService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(WorkSchedule entity)
        {
            try
            {
                if (entity.ScheduleItem != null && entity.WorkType != null) return entity.ScheduleItem.ProjectId == ProjectId && entity.WorkType.ProjectId == ProjectId;
                if ((await _context.ScheduleItems.Where(x => x.ScheduleId == entity.ScheduleId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.WorkTypes.Where(x => x.WorkTypeId == entity.WorkId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(WorkScheduleService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.WorkSchedules.CountAsync(x => lstIds.Contains(x.WorkScheduleId) && (x.ScheduleItem.ProjectId != ProjectId || x.WorkType.ProjectId != ProjectId)) == 0;
        }
    }
}
