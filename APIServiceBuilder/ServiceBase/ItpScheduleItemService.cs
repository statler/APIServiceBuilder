
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
    public interface IItpScheduleItemService : IServiceBase<ItpScheduleItem>
    {
    }

    public partial class ItpScheduleItemService : AbstractService<ItpScheduleItem>, IItpScheduleItemService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ItpScheduleItemService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ItpScheduleItem> GetEntitiesForProjectQry()
        {
            return _context.ItpScheduleItems.Where(x => x.Itp.ProjectId == ProjectId && x.ScheduleItem.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ItpSchedId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ItpSchedId ?? int.MinValue)).ForEachAsync(x => x.ItpSchedId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ItpSchedId ?? int.MinValue)));
                ////Delete base objects

                _context.ItpScheduleItems.RemoveRange(_context.ItpScheduleItems.Where(x => lstIdsToDelete.Contains(x.ItpSchedId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ItpScheduleItem (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ItpScheduleItem entity)
        {
            try
            {
                return (await _context.ItpScheduleItems.CountAsync(x => x.ItpId == entity.ItpId &&
                  x.ScheduleId == entity.ScheduleId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ItpScheduleItemService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ItpScheduleItem entity)
        {
            try
            {
                if (entity.Itp != null && entity.ScheduleItem != null) return entity.Itp.ProjectId == ProjectId && entity.ScheduleItem.ProjectId == ProjectId;
                if ((await _context.Itps.Where(x => x.ItpId == entity.ItpId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ScheduleItems.Where(x => x.ScheduleId == entity.ScheduleId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ItpScheduleItemService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ItpScheduleItems.CountAsync(x => lstIds.Contains(x.ItpSchedId) && (x.Itp.ProjectId != ProjectId || x.ScheduleItem.ProjectId != ProjectId)) == 0;
        }
    }
}
