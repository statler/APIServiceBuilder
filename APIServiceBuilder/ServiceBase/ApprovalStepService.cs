
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
    public interface IApprovalStepService : IServiceBase<ApprovalStep>
    {
    }

    public partial class ApprovalStepService : AbstractService<ApprovalStep>, IApprovalStepService
    {

        public ApprovalStepService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalStep> GetEntitiesForProject()
        {
            return _context.ApprovalStep.Where(x => x.Approval.ProjectId == ProjectId && x.ApprovalStatus.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalStepId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepId ?? int.MinValue)).ForEachAsync(x => x.ApprovalStepId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalStep.RemoveRange(_context.ApprovalStep.Where(x => lstIdsToDelete.Contains(x.ApprovalStepId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalStep (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ApprovalStepWithRelated = new ApprovalStepRelatedItemListDto() { ApprovalStepId = Id };
        //    ApprovalStepWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ApprovalStepId != null && f.ApprovalStepId == Id)
        //        .ProjectTo<ApprovalStepWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ApprovalStepWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalStep entity)
        {
            try
            {
                return (await _context.ApprovalStep.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.ApprovalStatusId == entity.ApprovalStatusId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalStepService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalStep entity)
        {
            try
            {
                if (entity.Approval != null && entity.ApprovalStatus != null) return entity.Approval.ProjectId == ProjectId && entity.ApprovalStatus.ProjectId == ProjectId;
                if ((await _context.Approval.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.ApprovalStatus.Where(x => x.ApprovalStatusId == entity.ApprovalStatusId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalStepService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalStep.CountAsync(x => lstIds.Contains(x.ApprovalStepId) && (x.Approval.ProjectId != ProjectId || x.ApprovalStatus.ProjectId != ProjectId)) == 0;
        }
    }
}