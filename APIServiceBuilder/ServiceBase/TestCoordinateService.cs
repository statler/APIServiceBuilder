
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
    public interface ITestCoordinateService : IServiceBase<TestCoordinate>
    {
    }

    public partial class TestCoordinateService : AbstractService<TestCoordinate>, ITestCoordinateService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public TestCoordinateService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestCoordinate> GetEntitiesForProjectQry()
        {
            return _context.TestCoordinates.Where(x => x.TestRequest.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestCoordinatesId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestCoordinatesId ?? int.MinValue)).ForEachAsync(x => x.TestCoordinatesId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestCoordinatesId ?? int.MinValue)));
                ////Delete base objects

                _context.TestCoordinates.RemoveRange(_context.TestCoordinates.Where(x => lstIdsToDelete.Contains(x.TestCoordinatesId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestCoordinate (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestCoordinate entity)
        {
            try
            {
                return (await _context.TestCoordinates.CountAsync(x => x.TestRequestId == entity.TestRequestId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestCoordinateService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestCoordinate entity)
        {
            try
            {
                if (entity.TestRequest != null) return entity.TestRequest.ProjectId == ProjectId;
                if ((await _context.TestRequests.Where(x => x.TestRequestId == entity.TestRequestId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestCoordinateService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestCoordinates.CountAsync(x => lstIds.Contains(x.TestCoordinatesId) && (x.TestRequest.ProjectId != ProjectId)) == 0;
        }
    }
}