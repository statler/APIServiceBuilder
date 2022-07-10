
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
    public interface IProgressClaimSnapshotService : IServiceBase<ProgressClaimSnapshot>
    {
    }

    public partial class ProgressClaimSnapshotService : AbstractService<ProgressClaimSnapshot>, IProgressClaimSnapshotService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ProgressClaimSnapshotService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ProgressClaimSnapshot> GetEntitiesForProjectQry()
        {
            return _context.ProgressClaimSnapshots.Where(x => x.Lot.ProjectId == ProjectId && x.Ncr.ProjectId == ProjectId && x.ProgressClaimVersion.ProjectId == ProjectId && x.ReportPeriod.ProjectId == ProjectId && x.ScheduleItem.ProjectId == ProjectId && x.Variation.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ProgressClaimSnapshotId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ProgressClaimSnapshotId ?? int.MinValue)).ForEachAsync(x => x.ProgressClaimSnapshotId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ProgressClaimSnapshotId ?? int.MinValue)));
                ////Delete base objects

                _context.ProgressClaimSnapshots.RemoveRange(_context.ProgressClaimSnapshots.Where(x => lstIdsToDelete.Contains(x.ProgressClaimSnapshotId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ProgressClaimSnapshot (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ProgressClaimSnapshot entity)
        {
            try
            {
                return (await _context.ProgressClaimSnapshots.CountAsync(x => x.LotId == entity.LotId &&
                  x.NcrId == entity.NcrId &&
                  x.ProgressClaimVersionId == entity.ProgressClaimVersionId &&
                  x.ReportPeriodId == entity.ReportPeriodId &&
                  x.ScheduleId == entity.ScheduleId &&
                  x.VariationId == entity.VariationId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ProgressClaimSnapshotService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ProgressClaimSnapshot entity)
        {
            try
            {
                if (entity.Lot != null && entity.Ncr != null && entity.ProgressClaimVersion != null && entity.ReportPeriod != null && entity.ScheduleItem != null && entity.Variation != null) return entity.Lot.ProjectId == ProjectId && entity.Ncr.ProjectId == ProjectId && entity.ProgressClaimVersion.ProjectId == ProjectId && entity.ReportPeriod.ProjectId == ProjectId && entity.ScheduleItem.ProjectId == ProjectId && entity.Variation.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Ncrs.Where(x => x.NcrId == entity.NcrId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ProgressClaimVersions.Where(x => x.ProgressClaimVersionId == entity.ProgressClaimVersionId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ReportPeriods.Where(x => x.ReportPeriodId == entity.ReportPeriodId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ScheduleItems.Where(x => x.ScheduleId == entity.ScheduleId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Variations.Where(x => x.VariationId == entity.VariationId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ProgressClaimSnapshotService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ProgressClaimSnapshots.CountAsync(x => lstIds.Contains(x.ProgressClaimSnapshotId) && (x.Lot.ProjectId != ProjectId || x.Ncr.ProjectId != ProjectId || x.ProgressClaimVersion.ProjectId != ProjectId || x.ReportPeriod.ProjectId != ProjectId || x.ScheduleItem.ProjectId != ProjectId || x.Variation.ProjectId != ProjectId)) == 0;
        }
    }
}
