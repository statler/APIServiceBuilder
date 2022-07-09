
using AutoMapper;
using AutoMapper.QueryableExtensions;
using cpDataORM.Dtos;
using cpDataORM.Models;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cpDataServices.Services;
using cpDataServices.Models;
using cpDataServices.Exceptions;

namespace cpDataServices.Services
{
    public interface IChecklistUserService : IServiceBase<ChecklistUser>
    {
    }

    public partial class ChecklistUserService : AbstractService<ChecklistUser>, IChecklistUserService
    {

        public ChecklistUserService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ChecklistUser> GetEntitiesForProject()
        {
            return ;
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ChecklistUserId == Id).CountAsync();
            //if (c > 0) lstRelatedItems.Add($" links ({c})");

            return lstRelatedItems;
        }

        public async Task DeleteAsync(List<int> lstIdsToDelete, bool shouldCommit = true)
        {
            if (!(await _userService.DoesUserHaveProjectAdminPermissionAsync())) throw new AuthorizationException("Project Admin permission is required to delete this record.");
            try
            {
                if (lstIdsToDelete.Contains(int.MinValue)) lstIdsToDelete.Remove(int.MinValue);
                ////Dereference links
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ChecklistUserId ?? int.MinValue)).ForEachAsync(x => x.ChecklistUserId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ChecklistUserId ?? int.MinValue)));
                ////Delete base objects

                _context.ChecklistUser.RemoveRange(_context.ChecklistUser.Where(x => lstIdsToDelete.Contains(x.ChecklistUserId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ChecklistUser (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ChecklistUserWithRelated = new ChecklistUserRelatedItemListDto() { ChecklistUserId = Id };
        //    ChecklistUserWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ChecklistUserId != null && f.ChecklistUserId == Id)
        //        .ProjectTo<ChecklistUserWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ChecklistUserWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ChecklistUser entity)
        {
            try
            {
                return (await _context.ChecklistUser.CountAsync(x => )) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ChecklistUserService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ChecklistUser entity)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ChecklistUserService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ChecklistUser.CountAsync(x => lstIds.Contains(x.ChecklistUserId) && ()) == 0;
        }
    }
}