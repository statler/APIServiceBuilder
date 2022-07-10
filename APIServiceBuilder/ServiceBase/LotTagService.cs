
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
    public interface ILotTagService : IServiceBase<LotTag>
    {
    }

    public partial class LotTagService : AbstractService<LotTag>, ILotTagService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotTagService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotTag> GetEntitiesForProjectQry()
        {
            return _context.LotTags.Where(x => x.Lot.ProjectId == ProjectId && x.TagCode.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.LotTagId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotTagId ?? int.MinValue)).ForEachAsync(x => x.LotTagId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotTagId ?? int.MinValue)));
                ////Delete base objects

                _context.LotTags.RemoveRange(_context.LotTags.Where(x => lstIdsToDelete.Contains(x.LotTagId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotTag (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotTag entity)
        {
            try
            {
                return (await _context.LotTags.CountAsync(x => x.LotId == entity.LotId &&
                  x.TagId == entity.TagId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotTagService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotTag entity)
        {
            try
            {
                if (entity.Lot != null && entity.TagCode != null) return entity.Lot.ProjectId == ProjectId && entity.TagCode.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.TagCodes.Where(x => x.TagId == entity.TagId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotTagService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotTags.CountAsync(x => lstIds.Contains(x.LotTagId) && (x.Lot.ProjectId != ProjectId || x.TagCode.ProjectId != ProjectId)) == 0;
        }
    }
}
