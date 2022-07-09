
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
    public interface ILotItpService : IServiceBase<LotItp>
    {
    }

    public partial class LotItpService : AbstractService<LotItp>, ILotItpService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotItpService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotItp> GetEntitiesForProjectQry()
        {
            return _context.LotItps.Where(x => x.Itp.ProjectId == ProjectId && x.Lot.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.LotItpId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotItpId ?? int.MinValue)).ForEachAsync(x => x.LotItpId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotItpId ?? int.MinValue)));
                ////Delete base objects

                _context.LotItps.RemoveRange(_context.LotItps.Where(x => lstIdsToDelete.Contains(x.LotItpId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotItp (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotItp entity)
        {
            try
            {
                return (await _context.LotItps.CountAsync(x => x.ItpId == entity.ItpId &&
                  x.LotId == entity.LotId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotItpService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotItp entity)
        {
            try
            {
                if (entity.Itp != null && entity.Lot != null) return entity.Itp.ProjectId == ProjectId && entity.Lot.ProjectId == ProjectId;
                if ((await _context.Itps.Where(x => x.ItpId == entity.ItpId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotItpService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotItps.CountAsync(x => lstIds.Contains(x.LotItpId) && (x.Itp.ProjectId != ProjectId || x.Lot.ProjectId != ProjectId)) == 0;
        }
    }
}