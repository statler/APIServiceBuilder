
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
    public interface ITestReqEmailService : IServiceBase<TestReqEmail>
    {
    }

    public partial class TestReqEmailService : AbstractService<TestReqEmail>, ITestReqEmailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public TestReqEmailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<TestReqEmail> GetEntitiesForProjectQry()
        {
            return _context.TestReqEmails.Where(x => x.EmailLog.ProjectId == ProjectId && x.TestRequest.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.TestReqEmailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestReqEmailId ?? int.MinValue)).ForEachAsync(x => x.TestReqEmailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.TestReqEmailId ?? int.MinValue)));
                ////Delete base objects

                _context.TestReqEmails.RemoveRange(_context.TestReqEmails.Where(x => lstIdsToDelete.Contains(x.TestReqEmailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting TestReqEmail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(TestReqEmail entity)
        {
            try
            {
                return (await _context.TestReqEmails.CountAsync(x => x.EmailLogId == entity.EmailLogId &&
                  x.TestRequestId == entity.TestRequestId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(TestReqEmailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(TestReqEmail entity)
        {
            try
            {
                if (entity.EmailLog != null && entity.TestRequest != null) return entity.EmailLog.ProjectId == ProjectId && entity.TestRequest.ProjectId == ProjectId;
                if ((await _context.EmailLogs.Where(x => x.EmailLogId == entity.EmailLogId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.TestRequests.Where(x => x.TestRequestId == entity.TestRequestId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(TestReqEmailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.TestReqEmails.CountAsync(x => lstIds.Contains(x.TestReqEmailId) && (x.EmailLog.ProjectId != ProjectId || x.TestRequest.ProjectId != ProjectId)) == 0;
        }
    }
}