
using AutoMapper;
using AutoMapper.QueryableExtensions;
using cpDataORM.Dtos;
using cpDataORM.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cpDataServices.Services;
using cpDataServices.Models;
using cpDataServices.Exceptions;

namespace cpDataServices.Services
{
    public interface ITestRequestPropertiesService : IServiceBase<TestRequestProperties>
    {
    }

    public partial class TestRequestPropertiesService : AbstractService<TestRequestProperties>, ITestRequestPropertiesService
    {

        public TestRequestPropertiesService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestRequestProperties> GetEntitiesForProject()
        {
            return _context.TestRequestProperties.Where(x => x.TestRequest.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestRequestPropertyId == Id).CountAsync();
            //if (c > 0) lstRelatedItems.Add($" links ({c})");

            return lstRelatedItems;
        }

        public async Task DeleteAsync(List<int> lstIdsToDelete, bool shouldCommit = true)
        {
            if (!(await _userService.DoesUserHaveProjectAdminPermissionAsync())) throw new AuthorizationException("Project Admin permission is required to delete this record.");
            try
            {
                if (lstIdsToDelete.Contains(int.MinValue)) lstIdsToDelete.Remove(int.MinValue);
                ////Dereference links
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestRequestPropertyId ?? int.MinValue)).ForEachAsync(x => x.TestRequestPropertyId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestRequestPropertyId ?? int.MinValue)));
                ////Delete base objects

                _context.TestRequestProperties.RemoveRange(_context.TestRequestProperties.Where(x => lstIdsToDelete.Contains(x.TestRequestPropertyId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestRequestProperties (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var TestRequestPropertiesWithRelated = new TestRequestPropertiesRelatedItemListDto() { TestRequestPropertyId = Id };
        //    TestRequestPropertiesWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.TestRequestPropertyId != null && f.TestRequestPropertyId == Id)
        //        .ProjectTo<TestRequestPropertiesWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return TestRequestPropertiesWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestRequestProperties entity)
        {
            try
            {
                return (await _context.TestRequestProperties.CountAsync(x => x.TestRequestId == entity.TestRequestId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestRequestPropertiesService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestRequestProperties entity)
        {
            try
            {
                if (entity.TestRequest != null) return entity.TestRequest.ProjectId == ProjectId;
                if ((await _context.TestRequest.Where(x => x.TestRequestId == entity.TestRequestId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestRequestPropertiesService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestRequestProperties.CountAsync(x => lstIds.Contains(x.TestRequestPropertyId) && (x.TestRequest.ProjectId != ProjectId)) == 0;
        }
    }
}