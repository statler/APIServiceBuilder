
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
    public interface IArTemplateStepService : IServiceBase<ArTemplateStep>
    {
    }

    public partial class ArTemplateStepService : AbstractService<ArTemplateStep>, IArTemplateStepService
    {

        public ArTemplateStepService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ArTemplateStep> GetEntitiesForProject()
        {
            return _context.ArTemplateStep.Where(x => x.ArTemplate.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ArTemplateStepId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepId ?? int.MinValue)).ForEachAsync(x => x.ArTemplateStepId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepId ?? int.MinValue)));
                ////Delete base objects

                _context.ArTemplateStep.RemoveRange(_context.ArTemplateStep.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ArTemplateStep (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ArTemplateStepWithRelated = new ArTemplateStepRelatedItemListDto() { ArTemplateStepId = Id };
        //    ArTemplateStepWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ArTemplateStepId != null && f.ArTemplateStepId == Id)
        //        .ProjectTo<ArTemplateStepWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ArTemplateStepWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ArTemplateStep entity)
        {
            try
            {
                return (await _context.ArTemplateStep.CountAsync(x => x.ArTemplateId == entity.ArTemplateId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ArTemplateStepService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ArTemplateStep entity)
        {
            try
            {
                if (entity.ArTemplate != null) return entity.ArTemplate.ProjectId == ProjectId;
                if ((await _context.ArTemplate.Where(x => x.ArTemplateId == entity.ArTemplateId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ArTemplateStepService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ArTemplateStep.CountAsync(x => lstIds.Contains(x.ArTemplateStepId) && (x.ArTemplate.ProjectId != ProjectId)) == 0;
        }
    }
}