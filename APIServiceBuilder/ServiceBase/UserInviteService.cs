
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
    public interface IUserInviteService : IServiceBase<UserInvite>
    {
    }

    public partial class UserInviteService : AbstractService<UserInvite>, IUserInviteService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public UserInviteService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<UserInvite> GetEntitiesForProjectQry()
        {
            return _context.UserInvites.Where(x => x.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.UserInviteId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.UserInviteId ?? int.MinValue)).ForEachAsync(x => x.UserInviteId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.UserInviteId ?? int.MinValue)));
                ////Delete base objects

                _context.UserInvites.RemoveRange(_context.UserInvites.Where(x => lstIdsToDelete.Contains(x.UserInviteId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting UserInvite (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(UserInvite entity)
        {
            try
            {
                return (await _context.UserInvites.CountAsync(x => x.UqName == entity.UqName
                    && x.ProjectId == entity.ProjectId
                    && x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(UserInviteService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(UserInvite entity)
        {
            try
            {
                return ProjectId == entity.ProjectId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(UserInviteService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.UserInvites.CountAsync(x => lstIds.Contains(x.UserInviteId) && (x.ProjectId != ProjectId)) == 0;
        }
    }
}