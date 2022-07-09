
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
    public interface IApprovalStepStatusService : IServiceBase<ApprovalStepStatus>
    {
    }

    public partial class ApprovalStepStatusService : AbstractService<ApprovalStepStatus>, IApprovalStepStatusService
    {

        public ApprovalStepStatusService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalStepStatus> GetEntitiesForProject()
        {
            return _context.ApprovalStepStatus.Where(x => x.ApprovalStatus.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalStepStatusId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepStatusId ?? int.MinValue)).ForEachAsync(x => x.ApprovalStepStatusId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepStatusId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalStepStatus.RemoveRange(_context.ApprovalStepStatus.Where(x => lstIdsToDelete.Contains(x.ApprovalStepStatusId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalStepStatus (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ApprovalStepStatusWithRelated = new ApprovalStepStatusRelatedItemListDto() { ApprovalStepStatusId = Id };
        //    ApprovalStepStatusWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ApprovalStepStatusId != null && f.ApprovalStepStatusId == Id)
        //        .ProjectTo<ApprovalStepStatusWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ApprovalStepStatusWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalStepStatus entity)
        {
            try
            {
                return (await _context.ApprovalStepStatus.CountAsync(x => x.ApprovalStatusId == entity.ApprovalStatusId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalStepStatusService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalStepStatus entity)
        {
            try
            {
                if (entity.ApprovalStatus != null) return entity.ApprovalStatus.ProjectId == ProjectId;
                if ((await _context.ApprovalStatus.Where(x => x.ApprovalStatusId == entity.ApprovalStatusId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalStepStatusService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalStepStatus.CountAsync(x => lstIds.Contains(x.ApprovalStepStatusId) && (x.ApprovalStatus.ProjectId != ProjectId)) == 0;
        }
    }
}