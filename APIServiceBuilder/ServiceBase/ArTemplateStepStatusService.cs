
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
    public interface IArTemplateStepStatusService : IServiceBase<ArTemplateStepStatus>
    {
    }

    public partial class ArTemplateStepStatusService : AbstractService<ArTemplateStepStatus>, IArTemplateStepStatusService
    {

        public ArTemplateStepStatusService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ArTemplateStepStatus> GetEntitiesForProject()
        {
            return _context.ArTemplateStepStatus.Where(x => x.ApprovalStatus.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ArTemplateStepStatusId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepStatusId ?? int.MinValue)).ForEachAsync(x => x.ArTemplateStepStatusId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepStatusId ?? int.MinValue)));
                ////Delete base objects

                _context.ArTemplateStepStatus.RemoveRange(_context.ArTemplateStepStatus.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepStatusId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ArTemplateStepStatus (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ArTemplateStepStatusWithRelated = new ArTemplateStepStatusRelatedItemListDto() { ArTemplateStepStatusId = Id };
        //    ArTemplateStepStatusWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ArTemplateStepStatusId != null && f.ArTemplateStepStatusId == Id)
        //        .ProjectTo<ArTemplateStepStatusWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ArTemplateStepStatusWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ArTemplateStepStatus entity)
        {
            try
            {
                return (await _context.ArTemplateStepStatus.CountAsync(x => x.ApprovalStatusId == entity.ApprovalStatusId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ArTemplateStepStatusService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ArTemplateStepStatus entity)
        {
            try
            {
                if (entity.ApprovalStatus != null) return entity.ApprovalStatus.ProjectId == ProjectId;
                if ((await _context.ApprovalStatus.Where(x => x.ApprovalStatusId == entity.ApprovalStatusId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ArTemplateStepStatusService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ArTemplateStepStatus.CountAsync(x => lstIds.Contains(x.ArTemplateStepStatusId) && (x.ApprovalStatus.ProjectId != ProjectId)) == 0;
        }
    }
}