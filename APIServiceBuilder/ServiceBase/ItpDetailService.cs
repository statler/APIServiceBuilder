
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
    public interface IItpDetailService : IServiceBase<ItpDetail>
    {
    }

    public partial class ItpDetailService : AbstractService<ItpDetail>, IItpDetailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public ItpDetailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ItpDetail> GetEntitiesForProjectQry()
        {
            return _context.ItpDetails.Where(x => x.Itp.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ItpDetailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ItpDetailId ?? int.MinValue)).ForEachAsync(x => x.ItpDetailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ItpDetailId ?? int.MinValue)));
                ////Delete base objects

                _context.ItpDetails.RemoveRange(_context.ItpDetails.Where(x => lstIdsToDelete.Contains(x.ItpDetailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ItpDetail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ItpDetail entity)
        {
            try
            {
                return (await _context.ItpDetails.CountAsync(x => x.ItpId == entity.ItpId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ItpDetailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ItpDetail entity)
        {
            try
            {
                if (entity.Itp != null) return entity.Itp.ProjectId == ProjectId;
                if ((await _context.Itps.Where(x => x.ItpId == entity.ItpId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ItpDetailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ItpDetails.CountAsync(x => lstIds.Contains(x.ItpDetailId) && (x.Itp.ProjectId != ProjectId)) == 0;
        }
    }
}
