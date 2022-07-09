
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
    public interface IProgressClaimDetailService : IServiceBase<ProgressClaimDetail>
    {
    }

    public partial class ProgressClaimDetailService : AbstractService<ProgressClaimDetail>, IProgressClaimDetailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ProgressClaimDetailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ProgressClaimDetail> GetEntitiesForProjectQry()
        {
            return _context.ProgressClaimDetails.Where(x => x.ProgressClaimVersion.ProjectId == ProjectId && x.ReportPeriod.ProjectId == ProjectId && x.ScheduleItem.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ProgressClaimDetailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ProgressClaimDetailId ?? int.MinValue)).ForEachAsync(x => x.ProgressClaimDetailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ProgressClaimDetailId ?? int.MinValue)));
                ////Delete base objects

                _context.ProgressClaimDetails.RemoveRange(_context.ProgressClaimDetails.Where(x => lstIdsToDelete.Contains(x.ProgressClaimDetailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ProgressClaimDetail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ProgressClaimDetail entity)
        {
            try
            {
                return (await _context.ProgressClaimDetails.CountAsync(x => x.ProgressClaimVersionId == entity.ProgressClaimVersionId &&
                  x.ReportPeriodId == entity.ReportPeriodId &&
                  x.ScheduleId == entity.ScheduleId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ProgressClaimDetailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ProgressClaimDetail entity)
        {
            try
            {
                if (entity.ProgressClaimVersion != null && entity.ReportPeriod != null && entity.ScheduleItem != null) return entity.ProgressClaimVersion.ProjectId == ProjectId && entity.ReportPeriod.ProjectId == ProjectId && entity.ScheduleItem.ProjectId == ProjectId;
                if ((await _context.ProgressClaimVersions.Where(x => x.ProgressClaimVersionId == entity.ProgressClaimVersionId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ReportPeriods.Where(x => x.ReportPeriodId == entity.ReportPeriodId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ScheduleItems.Where(x => x.ScheduleId == entity.ScheduleId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ProgressClaimDetailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ProgressClaimDetails.CountAsync(x => lstIds.Contains(x.ProgressClaimDetailId) && (x.ProgressClaimVersion.ProjectId != ProjectId || x.ReportPeriod.ProjectId != ProjectId || x.ScheduleItem.ProjectId != ProjectId)) == 0;
        }
    }
}