
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
    public interface IArTemplateStepPrereqService : IServiceBase<ArTemplateStepPrereq>
    {
    }

    public partial class ArTemplateStepPrereqService : AbstractService<ArTemplateStepPrereq>, IArTemplateStepPrereqService
    {

        public ArTemplateStepPrereqService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ArTemplateStepPrereq> GetEntitiesForProject()
        {
            return ;
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ArTemplateStepPrereqId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepPrereqId ?? int.MinValue)).ForEachAsync(x => x.ArTemplateStepPrereqId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepPrereqId ?? int.MinValue)));
                ////Delete base objects

                _context.ArTemplateStepPrereq.RemoveRange(_context.ArTemplateStepPrereq.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepPrereqId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ArTemplateStepPrereq (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ArTemplateStepPrereqWithRelated = new ArTemplateStepPrereqRelatedItemListDto() { ArTemplateStepPrereqId = Id };
        //    ArTemplateStepPrereqWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ArTemplateStepPrereqId != null && f.ArTemplateStepPrereqId == Id)
        //        .ProjectTo<ArTemplateStepPrereqWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ArTemplateStepPrereqWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ArTemplateStepPrereq entity)
        {
            try
            {
                return (await _context.ArTemplateStepPrereq.CountAsync(x => )) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ArTemplateStepPrereqService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ArTemplateStepPrereq entity)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ArTemplateStepPrereqService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ArTemplateStepPrereq.CountAsync(x => lstIds.Contains(x.ArTemplateStepPrereqId) && ()) == 0;
        }
    }
}