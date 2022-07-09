
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
    public interface IVariationEstimateService : IServiceBase<VariationEstimate>
    {
    }

    public partial class VariationEstimateService : AbstractService<VariationEstimate>, IVariationEstimateService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public VariationEstimateService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<VariationEstimate> GetEntitiesForProjectQry()
        {
            return _context.VariationEstimates.Where(x => x.Variation.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.VariationEstimateId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.VariationEstimateId ?? int.MinValue)).ForEachAsync(x => x.VariationEstimateId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.VariationEstimateId ?? int.MinValue)));
                ////Delete base objects

                _context.VariationEstimates.RemoveRange(_context.VariationEstimates.Where(x => lstIdsToDelete.Contains(x.VariationEstimateId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting VariationEstimate (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(VariationEstimate entity)
        {
            try
            {
                return (await _context.VariationEstimates.CountAsync(x => x.VariationId == entity.VariationId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(VariationEstimateService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(VariationEstimate entity)
        {
            try
            {
                if (entity.Variation != null) return entity.Variation.ProjectId == ProjectId;
                if ((await _context.Variations.Where(x => x.VariationId == entity.VariationId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(VariationEstimateService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.VariationEstimates.CountAsync(x => lstIds.Contains(x.VariationEstimateId) && (x.Variation.ProjectId != ProjectId)) == 0;
        }
    }
}