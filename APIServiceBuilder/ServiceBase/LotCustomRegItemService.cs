
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
    public interface ILotCustomRegItemService : IServiceBase<LotCustomRegItem>
    {
    }

    public partial class LotCustomRegItemService : AbstractService<LotCustomRegItem>, ILotCustomRegItemService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotCustomRegItemService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotCustomRegItem> GetEntitiesForProjectQry()
        {
            return _context.LotCustomRegItems.Where(x => x.Lot.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.LotCustomRegItemId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotCustomRegItemId ?? int.MinValue)).ForEachAsync(x => x.LotCustomRegItemId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotCustomRegItemId ?? int.MinValue)));
                ////Delete base objects

                _context.LotCustomRegItems.RemoveRange(_context.LotCustomRegItems.Where(x => lstIdsToDelete.Contains(x.LotCustomRegItemId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotCustomRegItem (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotCustomRegItem entity)
        {
            try
            {
                return (await _context.LotCustomRegItems.CountAsync(x => x.LotId == entity.LotId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotCustomRegItemService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotCustomRegItem entity)
        {
            try
            {
                if (entity.Lot != null) return entity.Lot.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotCustomRegItemService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotCustomRegItems.CountAsync(x => lstIds.Contains(x.LotCustomRegItemId) && (x.Lot.ProjectId != ProjectId)) == 0;
        }
    }
}
