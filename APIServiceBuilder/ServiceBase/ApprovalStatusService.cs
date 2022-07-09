
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
    public interface IApprovalStatusService : IServiceBase<ApprovalStatus>
    {
    }

    public partial class ApprovalStatusService : AbstractService<ApprovalStatus>, IApprovalStatusService
    {

        public ApprovalStatusService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalStatus> GetEntitiesForProject()
        {
            return _context.ApprovalStatus.Where(x => x.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalStatusId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStatusId ?? int.MinValue)).ForEachAsync(x => x.ApprovalStatusId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStatusId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalStatus.RemoveRange(_context.ApprovalStatus.Where(x => lstIdsToDelete.Contains(x.ApprovalStatusId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalStatus (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ApprovalStatusWithRelated = new ApprovalStatusRelatedItemListDto() { ApprovalStatusId = Id };
        //    ApprovalStatusWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ApprovalStatusId != null && f.ApprovalStatusId == Id)
        //        .ProjectTo<ApprovalStatusWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ApprovalStatusWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalStatus entity)
        {
            try
            {
                return (await _context.ApprovalStatus.CountAsync(x => x.UqName == entity.UqName
                    && x.ProjectId == entity.ProjectId
                    && x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalStatusService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalStatus entity)
        {
            try
            {
                return ProjectId == entity.ProjectId;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalStatusService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalStatus.CountAsync(x => lstIds.Contains(x.ApprovalStatusId) && (x.ProjectId != ProjectId)) == 0;
        }
    }
}