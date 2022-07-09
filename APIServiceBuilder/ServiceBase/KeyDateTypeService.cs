
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
    public interface IKeyDateTypeService : IServiceBase<KeyDateType>
    {
    }

    public partial class KeyDateTypeService : AbstractService<KeyDateType>, IKeyDateTypeService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public KeyDateTypeService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<KeyDateType> GetEntitiesForProjectQry()
        {
            return ;
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.KeyDateTypeId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.KeyDateTypeId ?? int.MinValue)).ForEachAsync(x => x.KeyDateTypeId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.KeyDateTypeId ?? int.MinValue)));
                ////Delete base objects

                _context.KeyDateTypes.RemoveRange(_context.KeyDateTypes.Where(x => lstIdsToDelete.Contains(x.KeyDateTypeId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting KeyDateType (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(KeyDateType entity)
        {
            try
            {
                return (await _context.KeyDateTypes.CountAsync(x => )) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(KeyDateTypeService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(KeyDateType entity)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(KeyDateTypeService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.KeyDateTypes.CountAsync(x => lstIds.Contains(x.KeyDateTypeId) && ()) == 0;
        }
    }
}