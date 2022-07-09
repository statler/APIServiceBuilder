
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
    public interface ICnNoticeService : IServiceBase<CnNotice>
    {
    }

    public partial class CnNoticeService : AbstractService<CnNotice>, ICnNoticeService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnNoticeService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnNotice> GetEntitiesForProjectQry()
        {
            return _context.CnNotices.Where(x => x.ContractNotice_CnId1.ProjectId == ProjectId && x.ContractNotice_CnId2.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnNoticeId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnNoticeId ?? int.MinValue)).ForEachAsync(x => x.CnNoticeId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnNoticeId ?? int.MinValue)));
                ////Delete base objects

                _context.CnNotices.RemoveRange(_context.CnNotices.Where(x => lstIdsToDelete.Contains(x.CnNoticeId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnNotice (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnNotice entity)
        {
            try
            {
                return (await _context.CnNotices.CountAsync(x => x.CnId1 == entity.CnId1 &&
                  x.CnId2 == entity.CnId2 &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnNoticeService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnNotice entity)
        {
            try
            {
                if (entity.ContractNotice_CnId1 != null && entity.ContractNotice_CnId2 != null) return entity.ContractNotice_CnId1.ProjectId == ProjectId && entity.ContractNotice_CnId2.ProjectId == ProjectId;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId1 && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId2 && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnNoticeService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnNotices.CountAsync(x => lstIds.Contains(x.CnNoticeId) && (x.ContractNotice_CnId1.ProjectId != ProjectId || x.ContractNotice_CnId2.ProjectId != ProjectId)) == 0;
        }
    }
}