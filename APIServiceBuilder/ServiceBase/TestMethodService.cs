
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
    public interface ITestMethodService : IServiceBase<TestMethod>
    {
    }

    public partial class TestMethodService : AbstractService<TestMethod>, ITestMethodService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public TestMethodService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestMethod> GetEntitiesForProjectQry()
        {
            return _context.TestMethods.Where(x => x.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestMethodId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestMethodId ?? int.MinValue)).ForEachAsync(x => x.TestMethodId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestMethodId ?? int.MinValue)));
                ////Delete base objects

                _context.TestMethods.RemoveRange(_context.TestMethods.Where(x => lstIdsToDelete.Contains(x.TestMethodId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestMethod (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestMethod entity)
        {
            try
            {
                return (await _context.TestMethods.CountAsync(x => x.UqName == entity.UqName
                    && x.ProjectId == entity.ProjectId
                    && x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestMethodService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestMethod entity)
        {
            try
            {
                return ProjectId == entity.ProjectId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestMethodService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestMethods.CountAsync(x => lstIds.Contains(x.TestMethodId) && (x.ProjectId != ProjectId)) == 0;
        }
    }
}