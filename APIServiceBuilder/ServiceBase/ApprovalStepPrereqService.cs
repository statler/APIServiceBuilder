
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
    public interface IApprovalStepPrereqService : IServiceBase<ApprovalStepPrereq>
    {
    }

    public partial class ApprovalStepPrereqService : AbstractService<ApprovalStepPrereq>, IApprovalStepPrereqService
    {

        public ApprovalStepPrereqService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalStepPrereq> GetEntitiesForProject()
        {
            return ;
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalStepPrereqId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepPrereqId ?? int.MinValue)).ForEachAsync(x => x.ApprovalStepPrereqId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalStepPrereqId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalStepPrereq.RemoveRange(_context.ApprovalStepPrereq.Where(x => lstIdsToDelete.Contains(x.ApprovalStepPrereqId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalStepPrereq (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ApprovalStepPrereqWithRelated = new ApprovalStepPrereqRelatedItemListDto() { ApprovalStepPrereqId = Id };
        //    ApprovalStepPrereqWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ApprovalStepPrereqId != null && f.ApprovalStepPrereqId == Id)
        //        .ProjectTo<ApprovalStepPrereqWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ApprovalStepPrereqWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalStepPrereq entity)
        {
            try
            {
                return (await _context.ApprovalStepPrereq.CountAsync(x => )) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalStepPrereqService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalStepPrereq entity)
        {
            try
            {
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalStepPrereqService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalStepPrereq.CountAsync(x => lstIds.Contains(x.ApprovalStepPrereqId) && ()) == 0;
        }
    }
}