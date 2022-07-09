
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
    public interface ITestRequestService : IServiceBase<TestRequest>
    {
    }

    public partial class TestRequestService : AbstractService<TestRequest>, ITestRequestService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public TestRequestService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestRequest> GetEntitiesForProjectQry()
        {
            return _context.TestRequests.Where(x => x.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestRequestId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestRequestId ?? int.MinValue)).ForEachAsync(x => x.TestRequestId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestRequestId ?? int.MinValue)));
                ////Delete base objects

                _context.TestRequests.RemoveRange(_context.TestRequests.Where(x => lstIdsToDelete.Contains(x.TestRequestId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestRequest (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestRequest entity)
        {
            try
            {
                return (await _context.TestRequests.CountAsync(x => x.UqName == entity.UqName
                    && x.ProjectId == entity.ProjectId
                    && x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestRequestService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestRequest entity)
        {
            try
            {
                return ProjectId == entity.ProjectId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestRequestService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestRequests.CountAsync(x => lstIds.Contains(x.TestRequestId) && (x.ProjectId != ProjectId)) == 0;
        }
    }
}