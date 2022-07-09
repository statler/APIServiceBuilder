
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
    public interface IApprovalNcrService : IServiceBase<ApprovalNcr>
    {
    }

    public partial class ApprovalNcrService : AbstractService<ApprovalNcr>, IApprovalNcrService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ApprovalNcrService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalNcr> GetEntitiesForProjectQry()
        {
            return _context.ApprovalNcrs.Where(x => x.Approval.ProjectId == ProjectId && x.Ncr.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalNcrId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalNcrId ?? int.MinValue)).ForEachAsync(x => x.ApprovalNcrId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalNcrId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalNcrs.RemoveRange(_context.ApprovalNcrs.Where(x => lstIdsToDelete.Contains(x.ApprovalNcrId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalNcr (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalNcr entity)
        {
            try
            {
                return (await _context.ApprovalNcrs.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.NcrId == entity.NcrId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalNcrService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalNcr entity)
        {
            try
            {
                if (entity.Approval != null && entity.Ncr != null) return entity.Approval.ProjectId == ProjectId && entity.Ncr.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Ncrs.Where(x => x.NcrId == entity.NcrId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalNcrService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalNcrs.CountAsync(x => lstIds.Contains(x.ApprovalNcrId) && (x.Approval.ProjectId != ProjectId || x.Ncr.ProjectId != ProjectId)) == 0;
        }
    }
}