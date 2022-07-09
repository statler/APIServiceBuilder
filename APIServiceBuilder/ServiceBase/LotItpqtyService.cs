
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
    public interface ILotItpQtyService : IServiceBase<LotItpQty>
    {
    }

    public partial class LotItpQtyService : AbstractService<LotItpQty>, ILotItpQtyService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public LotItpQtyService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<LotItpQty> GetEntitiesForProjectQry()
        {
            return ;
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.LotItpQtyId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotItpQtyId ?? int.MinValue)).ForEachAsync(x => x.LotItpQtyId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.LotItpQtyId ?? int.MinValue)));
                ////Delete base objects

                _context.LotItpQtys.RemoveRange(_context.LotItpQtys.Where(x => lstIdsToDelete.Contains(x.LotItpQtyId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting LotItpQty (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(LotItpQty entity)
        {
            try
            {
                return (await _context.LotItpQtys.CountAsync(x => )) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(LotItpQtyService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(LotItpQty entity)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(LotItpQtyService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.LotItpQtys.CountAsync(x => lstIds.Contains(x.LotItpQtyId) && ()) == 0;
        }
    }
}