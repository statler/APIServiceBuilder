
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
    public interface ICnVariationService : IServiceBase<CnVariation>
    {
    }

    public partial class CnVariationService : AbstractService<CnVariation>, ICnVariationService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnVariationService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnVariation> GetEntitiesForProjectQry()
        {
            return _context.CnVariations.Where(x => x.ContractNotice.ProjectId == ProjectId && x.Variation.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnVariationId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnVariationId ?? int.MinValue)).ForEachAsync(x => x.CnVariationId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnVariationId ?? int.MinValue)));
                ////Delete base objects

                _context.CnVariations.RemoveRange(_context.CnVariations.Where(x => lstIdsToDelete.Contains(x.CnVariationId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnVariation (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnVariation entity)
        {
            try
            {
                return (await _context.CnVariations.CountAsync(x => x.CnId == entity.CnId &&
                  x.VariationId == entity.VariationId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnVariationService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnVariation entity)
        {
            try
            {
                if (entity.ContractNotice != null && entity.Variation != null) return entity.ContractNotice.ProjectId == ProjectId && entity.Variation.ProjectId == ProjectId;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Variations.Where(x => x.VariationId == entity.VariationId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnVariationService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnVariations.CountAsync(x => lstIds.Contains(x.CnVariationId) && (x.ContractNotice.ProjectId != ProjectId || x.Variation.ProjectId != ProjectId)) == 0;
        }
    }
}