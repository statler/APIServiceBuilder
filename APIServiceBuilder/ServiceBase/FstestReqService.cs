
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
    public interface IFsTestReqService : IServiceBase<FsTestReq>
    {
    }

    public partial class FsTestReqService : AbstractService<FsTestReq>, IFsTestReqService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public FsTestReqService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<FsTestReq> GetEntitiesForProjectQry()
        {
            return _context.FsTestReqs.Where(x => x.FileStoreDoc.ProjectId == ProjectId && x.TestRequest.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.FsTestReqId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsTestReqId ?? int.MinValue)).ForEachAsync(x => x.FsTestReqId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsTestReqId ?? int.MinValue)));
                ////Delete base objects

                _context.FsTestReqs.RemoveRange(_context.FsTestReqs.Where(x => lstIdsToDelete.Contains(x.FsTestReqId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting FsTestReq (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(FsTestReq entity)
        {
            try
            {
                return (await _context.FsTestReqs.CountAsync(x => x.FsId == entity.FsId &&
                  x.TestReqId == entity.TestReqId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(FsTestReqService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(FsTestReq entity)
        {
            try
            {
                if (entity.FileStoreDoc != null && entity.TestRequest != null) return entity.FileStoreDoc.ProjectId == ProjectId && entity.TestRequest.ProjectId == ProjectId;
                if ((await _context.FileStoreDocs.Where(x => x.FileStoreDocId == entity.FsId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.TestRequests.Where(x => x.TestRequestId == entity.TestReqId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(FsTestReqService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.FsTestReqs.CountAsync(x => lstIds.Contains(x.FsTestReqId) && (x.FileStoreDoc.ProjectId != ProjectId || x.TestRequest.ProjectId != ProjectId)) == 0;
        }
    }
}