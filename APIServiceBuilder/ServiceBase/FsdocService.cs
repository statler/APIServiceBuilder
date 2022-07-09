
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
    public interface IFsDocService : IServiceBase<FsDoc>
    {
    }

    public partial class FsDocService : AbstractService<FsDoc>, IFsDocService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public FsDocService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<FsDoc> GetEntitiesForProjectQry()
        {
            return _context.FsDocs.Where(x => x.CpDocument.ProjectId == ProjectId && x.FileStoreDoc.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.FsDocId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsDocId ?? int.MinValue)).ForEachAsync(x => x.FsDocId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.FsDocId ?? int.MinValue)));
                ////Delete base objects

                _context.FsDocs.RemoveRange(_context.FsDocs.Where(x => lstIdsToDelete.Contains(x.FsDocId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting FsDoc (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(FsDoc entity)
        {
            try
            {
                return (await _context.FsDocs.CountAsync(x => x.DocId == entity.DocId &&
                  x.FsId == entity.FsId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(FsDocService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(FsDoc entity)
        {
            try
            {
                if (entity.CpDocument != null && entity.FileStoreDoc != null) return entity.CpDocument.ProjectId == ProjectId && entity.FileStoreDoc.ProjectId == ProjectId;
                if ((await _context.CpDocuments.Where(x => x.DocumentId == entity.DocId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.FileStoreDocs.Where(x => x.FileStoreDocId == entity.FsId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(FsDocService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.FsDocs.CountAsync(x => lstIds.Contains(x.FsDocId) && (x.CpDocument.ProjectId != ProjectId || x.FileStoreDoc.ProjectId != ProjectId)) == 0;
        }
    }
}