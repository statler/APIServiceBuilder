
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
    public interface ITestRequestTestService : IServiceBase<TestRequestTest>
    {
    }

    public partial class TestRequestTestService : AbstractService<TestRequestTest>, ITestRequestTestService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public TestRequestTestService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestRequestTest> GetEntitiesForProjectQry()
        {
            return _context.TestRequestTests.Where(x => x.ControlLine.ProjectId == ProjectId && x.ScheduleItem.ProjectId == ProjectId && x.TestMethod.ProjectId == ProjectId && x.TestRequest.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestRequestTestId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestRequestTestId ?? int.MinValue)).ForEachAsync(x => x.TestRequestTestId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestRequestTestId ?? int.MinValue)));
                ////Delete base objects

                _context.TestRequestTests.RemoveRange(_context.TestRequestTests.Where(x => lstIdsToDelete.Contains(x.TestRequestTestId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestRequestTest (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestRequestTest entity)
        {
            try
            {
                return (await _context.TestRequestTests.CountAsync(x => x.ControlLineId == entity.ControlLineId &&
                  x.ScheduleId == entity.ScheduleId &&
                  x.TestMethodId == entity.TestMethodId &&
                  x.TestRequestId == entity.TestRequestId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestRequestTestService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestRequestTest entity)
        {
            try
            {
                if (entity.ControlLine != null && entity.ScheduleItem != null && entity.TestMethod != null && entity.TestRequest != null) return entity.ControlLine.ProjectId == ProjectId && entity.ScheduleItem.ProjectId == ProjectId && entity.TestMethod.ProjectId == ProjectId && entity.TestRequest.ProjectId == ProjectId;
                if ((await _context.ControlLines.Where(x => x.ControlLineId == entity.ControlLineId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ScheduleItems.Where(x => x.ScheduleId == entity.ScheduleId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.TestMethods.Where(x => x.TestMethodId == entity.TestMethodId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.TestRequests.Where(x => x.TestRequestId == entity.TestRequestId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestRequestTestService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestRequestTests.CountAsync(x => lstIds.Contains(x.TestRequestTestId) && (x.ControlLine.ProjectId != ProjectId || x.ScheduleItem.ProjectId != ProjectId || x.TestMethod.ProjectId != ProjectId || x.TestRequest.ProjectId != ProjectId)) == 0;
        }
    }
}
