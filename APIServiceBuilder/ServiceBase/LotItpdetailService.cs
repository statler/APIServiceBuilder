
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
    public interface ILotItpDetailService : IServiceBase<LotItpDetail>
    {
    }

    public partial class LotItpDetailService : AbstractService<LotItpDetail>, ILotItpDetailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotItpDetailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotItpDetail> GetEntitiesForProjectQry()
        {
            return _context.LotItpDetails.Where(x => x.ApprovalAtp.ProjectId == ProjectId && x.Approval.ProjectId == ProjectId && x.ApprovalLot.ProjectId == ProjectId && x.Ncr.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.LotItpDetailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotItpDetailId ?? int.MinValue)).ForEachAsync(x => x.LotItpDetailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotItpDetailId ?? int.MinValue)));
                ////Delete base objects

                _context.LotItpDetails.RemoveRange(_context.LotItpDetails.Where(x => lstIdsToDelete.Contains(x.LotItpDetailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotItpDetail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotItpDetail entity)
        {
            try
            {
                return (await _context.LotItpDetails.CountAsync(x => x.ApprovalAtpId == entity.ApprovalAtpId &&
                  x.ApprovalId == entity.ApprovalId &&
                  x.ApprovalLotId == entity.ApprovalLotId &&
                  x.NcrId == entity.NcrId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotItpDetailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotItpDetail entity)
        {
            try
            {
                if (entity.ApprovalAtp != null && entity.Approval != null && entity.ApprovalLot != null && entity.Ncr != null) return entity.ApprovalAtp.ProjectId == ProjectId && entity.Approval.ProjectId == ProjectId && entity.ApprovalLot.ProjectId == ProjectId && entity.Ncr.ProjectId == ProjectId;
                if ((await _context.Atps.Where(x => x.AtpId == entity.ApprovalAtpId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Lots.Where(x => x.LotId == entity.ApprovalLotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Ncrs.Where(x => x.NcrId == entity.NcrId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotItpDetailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotItpDetails.CountAsync(x => lstIds.Contains(x.LotItpDetailId) && (x.ApprovalAtp.ProjectId != ProjectId || x.Approval.ProjectId != ProjectId || x.ApprovalLot.ProjectId != ProjectId || x.Ncr.ProjectId != ProjectId)) == 0;
        }
    }
}
