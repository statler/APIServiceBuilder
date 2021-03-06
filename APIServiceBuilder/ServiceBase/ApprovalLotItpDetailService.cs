
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
    public interface IApprovalLotItpDetailService : IServiceBase<ApprovalLotItpDetail>
    {
    }

    public partial class ApprovalLotItpDetailService : AbstractService<ApprovalLotItpDetail>, IApprovalLotItpDetailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ApprovalLotItpDetailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalLotItpDetail> GetEntitiesForProjectQry()
        {
            return _context.ApprovalLotItpDetails.Where(x => x.Approval.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalLotItpDetailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalLotItpDetailId ?? int.MinValue)).ForEachAsync(x => x.ApprovalLotItpDetailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalLotItpDetailId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalLotItpDetails.RemoveRange(_context.ApprovalLotItpDetails.Where(x => lstIdsToDelete.Contains(x.ApprovalLotItpDetailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalLotItpDetail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalLotItpDetail entity)
        {
            try
            {
                return (await _context.ApprovalLotItpDetails.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalLotItpDetailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalLotItpDetail entity)
        {
            try
            {
                if (entity.Approval != null) return entity.Approval.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalLotItpDetailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalLotItpDetails.CountAsync(x => lstIds.Contains(x.ApprovalLotItpDetailId) && (x.Approval.ProjectId != ProjectId)) == 0;
        }
    }
}
