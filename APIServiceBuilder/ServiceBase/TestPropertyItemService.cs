
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
    public interface ITestPropertyItemService : IServiceBase<TestPropertyItem>
    {
    }

    public partial class TestPropertyItemService : AbstractService<TestPropertyItem>, ITestPropertyItemService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public TestPropertyItemService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestPropertyItem> GetEntitiesForProjectQry()
        {
            return _context.TestPropertyItems.Where(x => x.TestPropertyGroup.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestPropertyItemId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestPropertyItemId ?? int.MinValue)).ForEachAsync(x => x.TestPropertyItemId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestPropertyItemId ?? int.MinValue)));
                ////Delete base objects

                _context.TestPropertyItems.RemoveRange(_context.TestPropertyItems.Where(x => lstIdsToDelete.Contains(x.TestPropertyItemId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestPropertyItem (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestPropertyItem entity)
        {
            try
            {
                return (await _context.TestPropertyItems.CountAsync(x => x.TestPropertyGroupId == entity.TestPropertyGroupId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestPropertyItemService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestPropertyItem entity)
        {
            try
            {
                if (entity.TestPropertyGroup != null) return entity.TestPropertyGroup.ProjectId == ProjectId;
                if ((await _context.TestPropertyGroups.Where(x => x.TestPropertyGroupId == entity.TestPropertyGroupId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestPropertyItemService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestPropertyItems.CountAsync(x => lstIds.Contains(x.TestPropertyItemId) && (x.TestPropertyGroup.ProjectId != ProjectId)) == 0;
        }
    }
}
