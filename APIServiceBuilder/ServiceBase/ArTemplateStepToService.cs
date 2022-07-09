
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
    public interface IArTemplateStepToService : IServiceBase<ArTemplateStepTo>
    {
    }

    public partial class ArTemplateStepToService : AbstractService<ArTemplateStepTo>, IArTemplateStepToService
    {

        public ArTemplateStepToService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ArTemplateStepTo> GetEntitiesForProject()
        {
            return ;
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ArTemplateStepToId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepToId ?? int.MinValue)).ForEachAsync(x => x.ArTemplateStepToId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepToId ?? int.MinValue)));
                ////Delete base objects

                _context.ArTemplateStepTo.RemoveRange(_context.ArTemplateStepTo.Where(x => lstIdsToDelete.Contains(x.ArTemplateStepToId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ArTemplateStepTo (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ArTemplateStepToWithRelated = new ArTemplateStepToRelatedItemListDto() { ArTemplateStepToId = Id };
        //    ArTemplateStepToWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ArTemplateStepToId != null && f.ArTemplateStepToId == Id)
        //        .ProjectTo<ArTemplateStepToWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ArTemplateStepToWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ArTemplateStepTo entity)
        {
            try
            {
                return (await _context.ArTemplateStepTo.CountAsync(x => )) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ArTemplateStepToService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ArTemplateStepTo entity)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ArTemplateStepToService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ArTemplateStepTo.CountAsync(x => lstIds.Contains(x.ArTemplateStepToId) && ()) == 0;
        }
    }
}