
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
    public interface ISiteDiaryCostCodeService : IServiceBase<SiteDiaryCostCode>
    {
    }

    public partial class SiteDiaryCostCodeService : AbstractService<SiteDiaryCostCode>, ISiteDiaryCostCodeService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public SiteDiaryCostCodeService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<SiteDiaryCostCode> GetEntitiesForProjectQry()
        {
            return _context.SiteDiaryCostCodes.Where(x => x.CostCode.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.SiteDiaryCostCodeId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.SiteDiaryCostCodeId ?? int.MinValue)).ForEachAsync(x => x.SiteDiaryCostCodeId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.SiteDiaryCostCodeId ?? int.MinValue)));
                ////Delete base objects

                _context.SiteDiaryCostCodes.RemoveRange(_context.SiteDiaryCostCodes.Where(x => lstIdsToDelete.Contains(x.SiteDiaryCostCodeId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting SiteDiaryCostCode (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(SiteDiaryCostCode entity)
        {
            try
            {
                return (await _context.SiteDiaryCostCodes.CountAsync(x => x.CostCodeId == entity.CostCodeId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(SiteDiaryCostCodeService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(SiteDiaryCostCode entity)
        {
            try
            {
                if (entity.CostCode != null) return entity.CostCode.ProjectId == ProjectId;
                if ((await _context.CostCodes.Where(x => x.CostCodeId == entity.CostCodeId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(SiteDiaryCostCodeService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.SiteDiaryCostCodes.CountAsync(x => lstIds.Contains(x.SiteDiaryCostCodeId) && (x.CostCode.ProjectId != ProjectId)) == 0;
        }
    }
}
