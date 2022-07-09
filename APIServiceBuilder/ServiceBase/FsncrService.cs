
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
    public interface IFsNcrService : IServiceBase<FsNcr>
    {
    }

    public partial class FsNcrService : AbstractService<FsNcr>, IFsNcrService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public FsNcrService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<FsNcr> GetEntitiesForProjectQry()
        {
            return _context.FsNcrs.Where(x => x.FileStoreDoc.ProjectId == ProjectId && x.Ncr.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.FsNcrId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsNcrId ?? int.MinValue)).ForEachAsync(x => x.FsNcrId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsNcrId ?? int.MinValue)));
                ////Delete base objects

                _context.FsNcrs.RemoveRange(_context.FsNcrs.Where(x => lstIdsToDelete.Contains(x.FsNcrId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting FsNcr (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(FsNcr entity)
        {
            try
            {
                return (await _context.FsNcrs.CountAsync(x => x.FsId == entity.FsId &&
                  x.NcrId == entity.NcrId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(FsNcrService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(FsNcr entity)
        {
            try
            {
                if (entity.FileStoreDoc != null && entity.Ncr != null) return entity.FileStoreDoc.ProjectId == ProjectId && entity.Ncr.ProjectId == ProjectId;
                if ((await _context.FileStoreDocs.Where(x => x.FileStoreDocId == entity.FsId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Ncrs.Where(x => x.NcrId == entity.NcrId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(FsNcrService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.FsNcrs.CountAsync(x => lstIds.Contains(x.FsNcrId) && (x.FileStoreDoc.ProjectId != ProjectId || x.Ncr.ProjectId != ProjectId)) == 0;
        }
    }
}