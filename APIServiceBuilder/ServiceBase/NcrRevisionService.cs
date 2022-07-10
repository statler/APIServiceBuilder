
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
    public interface INcrRevisionService : IServiceBase<NcrRevision>
    {
    }

    public partial class NcrRevisionService : AbstractService<NcrRevision>, INcrRevisionService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public NcrRevisionService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<NcrRevision> GetEntitiesForProjectQry()
        {
            return _context.NcrRevisions.Where(x => x.Ncr.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.NcrRevisionId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.NcrRevisionId ?? int.MinValue)).ForEachAsync(x => x.NcrRevisionId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.NcrRevisionId ?? int.MinValue)));
                ////Delete base objects

                _context.NcrRevisions.RemoveRange(_context.NcrRevisions.Where(x => lstIdsToDelete.Contains(x.NcrRevisionId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting NcrRevision (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(NcrRevision entity)
        {
            try
            {
                return (await _context.NcrRevisions.CountAsync(x => x.NcrId == entity.NcrId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(NcrRevisionService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(NcrRevision entity)
        {
            try
            {
                if (entity.Ncr != null) return entity.Ncr.ProjectId == ProjectId;
                if ((await _context.Ncrs.Where(x => x.NcrId == entity.NcrId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(NcrRevisionService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.NcrRevisions.CountAsync(x => lstIds.Contains(x.NcrRevisionId) && (x.Ncr.ProjectId != ProjectId)) == 0;
        }
    }
}
