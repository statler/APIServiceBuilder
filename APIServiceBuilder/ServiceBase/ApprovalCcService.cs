
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
    public interface IApprovalCcService : IServiceBase<ApprovalCc>
    {
    }

    public partial class ApprovalCcService : AbstractService<ApprovalCc>, IApprovalCcService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ApprovalCcService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalCc> GetEntitiesForProjectQry()
        {
            return _context.ApprovalCcs.Where(x => x.Approval.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.AprovalCcId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.AprovalCcId ?? int.MinValue)).ForEachAsync(x => x.AprovalCcId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.AprovalCcId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalCcs.RemoveRange(_context.ApprovalCcs.Where(x => lstIdsToDelete.Contains(x.AprovalCcId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalCc (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalCc entity)
        {
            try
            {
                return (await _context.ApprovalCcs.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalCcService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalCc entity)
        {
            try
            {
                if (entity.Approval != null) return entity.Approval.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalCcService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalCcs.CountAsync(x => lstIds.Contains(x.AprovalCcId) && (x.Approval.ProjectId != ProjectId)) == 0;
        }
    }
}