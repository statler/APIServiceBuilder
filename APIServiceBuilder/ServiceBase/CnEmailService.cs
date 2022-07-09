
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
    public interface ICnEmailService : IServiceBase<CnEmail>
    {
    }

    public partial class CnEmailService : AbstractService<CnEmail>, ICnEmailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnEmailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnEmail> GetEntitiesForProjectQry()
        {
            return _context.CnEmails.Where(x => x.ContractNotice.ProjectId == ProjectId && x.EmailLog.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnEmailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnEmailId ?? int.MinValue)).ForEachAsync(x => x.CnEmailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnEmailId ?? int.MinValue)));
                ////Delete base objects

                _context.CnEmails.RemoveRange(_context.CnEmails.Where(x => lstIdsToDelete.Contains(x.CnEmailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnEmail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnEmail entity)
        {
            try
            {
                return (await _context.CnEmails.CountAsync(x => x.ContractNoticeId == entity.ContractNoticeId &&
                  x.EmailLogId == entity.EmailLogId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnEmailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnEmail entity)
        {
            try
            {
                if (entity.ContractNotice != null && entity.EmailLog != null) return entity.ContractNotice.ProjectId == ProjectId && entity.EmailLog.ProjectId == ProjectId;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.ContractNoticeId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.EmailLogs.Where(x => x.EmailLogId == entity.EmailLogId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnEmailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnEmails.CountAsync(x => lstIds.Contains(x.CnEmailId) && (x.ContractNotice.ProjectId != ProjectId || x.EmailLog.ProjectId != ProjectId)) == 0;
        }
    }
}