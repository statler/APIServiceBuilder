
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
    public interface ICnControlledDocService : IServiceBase<CnControlledDoc>
    {
    }

    public partial class CnControlledDocService : AbstractService<CnControlledDoc>, ICnControlledDocService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnControlledDocService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnControlledDoc> GetEntitiesForProjectQry()
        {
            return _context.CnControlledDocs.Where(x => x.ContractNotice.ProjectId == ProjectId && x.CpDocument.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnContDocId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnContDocId ?? int.MinValue)).ForEachAsync(x => x.CnContDocId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnContDocId ?? int.MinValue)));
                ////Delete base objects

                _context.CnControlledDocs.RemoveRange(_context.CnControlledDocs.Where(x => lstIdsToDelete.Contains(x.CnContDocId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnControlledDoc (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnControlledDoc entity)
        {
            try
            {
                return (await _context.CnControlledDocs.CountAsync(x => x.CnId == entity.CnId &&
                  x.ContDocId == entity.ContDocId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnControlledDocService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnControlledDoc entity)
        {
            try
            {
                if (entity.ContractNotice != null && entity.CpDocument != null) return entity.ContractNotice.ProjectId == ProjectId && entity.CpDocument.ProjectId == ProjectId;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.CpDocuments.Where(x => x.DocumentId == entity.ContDocId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnControlledDocService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnControlledDocs.CountAsync(x => lstIds.Contains(x.CnContDocId) && (x.ContractNotice.ProjectId != ProjectId || x.CpDocument.ProjectId != ProjectId)) == 0;
        }
    }
}