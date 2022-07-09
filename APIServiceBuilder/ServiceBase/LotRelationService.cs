
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
    public interface ILotRelationService : IServiceBase<LotRelation>
    {
    }

    public partial class LotRelationService : AbstractService<LotRelation>, ILotRelationService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotRelationService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotRelation> GetEntitiesForProjectQry()
        {
            return _context.LotRelations.Where(x => x.Lot1.ProjectId == ProjectId && x.Lot2.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.RelLotId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.RelLotId ?? int.MinValue)).ForEachAsync(x => x.RelLotId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.RelLotId ?? int.MinValue)));
                ////Delete base objects

                _context.LotRelations.RemoveRange(_context.LotRelations.Where(x => lstIdsToDelete.Contains(x.RelLotId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotRelation (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotRelation entity)
        {
            try
            {
                return (await _context.LotRelations.CountAsync(x => x.LotId1 == entity.LotId1 &&
                  x.LotId2 == entity.LotId2 &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotRelationService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotRelation entity)
        {
            try
            {
                if (entity.Lot1 != null && entity.Lot2 != null) return entity.Lot1.ProjectId == ProjectId && entity.Lot2.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId1 && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId2 && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotRelationService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotRelations.CountAsync(x => lstIds.Contains(x.RelLotId) && (x.Lot1.ProjectId != ProjectId || x.Lot2.ProjectId != ProjectId)) == 0;
        }
    }
}