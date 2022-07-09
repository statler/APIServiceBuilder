
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
    public interface IApprovalStepToService : IServiceBase<ApprovalStepTo>
    {
    }

    public partial class ApprovalStepToService : AbstractService<ApprovalStepTo>, IApprovalStepToService
    {

        public ApprovalStepToService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalStepTo> GetEntitiesForProject()
        {
            return ;
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalStepToId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepToId ?? int.MinValue)).ForEachAsync(x => x.ApprovalStepToId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepToId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalStepTo.RemoveRange(_context.ApprovalStepTo.Where(x => lstIdsToDelete.Contains(x.ApprovalStepToId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalStepTo (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ApprovalStepToWithRelated = new ApprovalStepToRelatedItemListDto() { ApprovalStepToId = Id };
        //    ApprovalStepToWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ApprovalStepToId != null && f.ApprovalStepToId == Id)
        //        .ProjectTo<ApprovalStepToWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ApprovalStepToWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalStepTo entity)
        {
            try
            {
                return (await _context.ApprovalStepTo.CountAsync(x => )) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalStepToService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalStepTo entity)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalStepToService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalStepTo.CountAsync(x => lstIds.Contains(x.ApprovalStepToId) && ()) == 0;
        }
    }
}