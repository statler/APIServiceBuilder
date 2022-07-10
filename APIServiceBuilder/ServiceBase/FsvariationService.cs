
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
    public interface IFsVariationService : IServiceBase<FsVariation>
    {
    }

    public partial class FsVariationService : AbstractService<FsVariation>, IFsVariationService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public FsVariationService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<FsVariation> GetEntitiesForProjectQry()
        {
            return _context.FsVariations.Where(x => x.FileStoreDoc.ProjectId == ProjectId && x.Variation.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.FsVariationId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsVariationId ?? int.MinValue)).ForEachAsync(x => x.FsVariationId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsVariationId ?? int.MinValue)));
                ////Delete base objects

                _context.FsVariations.RemoveRange(_context.FsVariations.Where(x => lstIdsToDelete.Contains(x.FsVariationId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting FsVariation (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(FsVariation entity)
        {
            try
            {
                return (await _context.FsVariations.CountAsync(x => x.FsId == entity.FsId &&
                  x.VariationId == entity.VariationId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(FsVariationService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(FsVariation entity)
        {
            try
            {
                if (entity.FileStoreDoc != null && entity.Variation != null) return entity.FileStoreDoc.ProjectId == ProjectId && entity.Variation.ProjectId == ProjectId;
                if ((await _context.FileStoreDocs.Where(x => x.FileStoreDocId == entity.FsId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Variations.Where(x => x.VariationId == entity.VariationId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(FsVariationService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.FsVariations.CountAsync(x => lstIds.Contains(x.FsVariationId) && (x.FileStoreDoc.ProjectId != ProjectId || x.Variation.ProjectId != ProjectId)) == 0;
        }
    }
}
