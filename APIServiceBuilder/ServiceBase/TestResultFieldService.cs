
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
    public interface ITestResultFieldService : IServiceBase<TestResultField>
    {
    }

    public partial class TestResultFieldService : AbstractService<TestResultField>, ITestResultFieldService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public TestResultFieldService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestResultField> GetEntitiesForProjectQry()
        {
            return _context.TestResultFields.Where(x => x.TestMethod.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestResultFieldId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestResultFieldId ?? int.MinValue)).ForEachAsync(x => x.TestResultFieldId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestResultFieldId ?? int.MinValue)));
                ////Delete base objects

                _context.TestResultFields.RemoveRange(_context.TestResultFields.Where(x => lstIdsToDelete.Contains(x.TestResultFieldId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestResultField (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestResultField entity)
        {
            try
            {
                return (await _context.TestResultFields.CountAsync(x => x.TestMethodId == entity.TestMethodId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestResultFieldService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestResultField entity)
        {
            try
            {
                if (entity.TestMethod != null) return entity.TestMethod.ProjectId == ProjectId;
                if ((await _context.TestMethods.Where(x => x.TestMethodId == entity.TestMethodId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestResultFieldService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestResultFields.CountAsync(x => lstIds.Contains(x.TestResultFieldId) && (x.TestMethod.ProjectId != ProjectId)) == 0;
        }
    }
}
