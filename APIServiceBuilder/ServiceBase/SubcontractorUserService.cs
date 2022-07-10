
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
    public interface ISubcontractorUserService : IServiceBase<SubcontractorUser>
    {
    }

    public partial class SubcontractorUserService : AbstractService<SubcontractorUser>, ISubcontractorUserService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public SubcontractorUserService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<SubcontractorUser> GetEntitiesForProjectQry()
        {
            return _context.SubcontractorUsers.Where(x => x.Subcontractor.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.SubcontractorUserId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.SubcontractorUserId ?? int.MinValue)).ForEachAsync(x => x.SubcontractorUserId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.SubcontractorUserId ?? int.MinValue)));
                ////Delete base objects

                _context.SubcontractorUsers.RemoveRange(_context.SubcontractorUsers.Where(x => lstIdsToDelete.Contains(x.SubcontractorUserId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting SubcontractorUser (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(SubcontractorUser entity)
        {
            try
            {
                return (await _context.SubcontractorUsers.CountAsync(x => x.SubcontractorId == entity.SubcontractorId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(SubcontractorUserService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(SubcontractorUser entity)
        {
            try
            {
                if (entity.Subcontractor != null) return entity.Subcontractor.ProjectId == ProjectId;
                if ((await _context.Subcontractors.Where(x => x.SubcontractorId == entity.SubcontractorId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(SubcontractorUserService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.SubcontractorUsers.CountAsync(x => lstIds.Contains(x.SubcontractorUserId) && (x.Subcontractor.ProjectId != ProjectId)) == 0;
        }
    }
}
