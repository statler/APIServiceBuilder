
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
    public interface IApprovalAttachmentService : IServiceBase<ApprovalAttachment>
    {
    }

    public partial class ApprovalAttachmentService : AbstractService<ApprovalAttachment>, IApprovalAttachmentService
    {

        public ApprovalAttachmentService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<ApprovalAttachment> GetEntitiesForProject()
        {
            return _context.ApprovalAttachments.Where(x => x.Approval.ProjectId == ProjectId && x.FileStoreDoc.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.ApprovalAttachmentId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalAttachmentId ?? int.MinValue)).ForEachAsync(x => x.ApprovalAttachmentId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.ApprovalAttachmentId ?? int.MinValue)));
                ////Delete base objects

                _context.ApprovalAttachments.RemoveRange(_context.ApprovalAttachments.Where(x => lstIdsToDelete.Contains(x.ApprovalAttachmentId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting ApprovalAttachment (DeleteAsync)");
                throw;
            }
        }

        //public IRelatedItemList GetRelatedItemList(int Id)
        //{
        //    var ApprovalAttachmentWithRelated = new ApprovalAttachmentRelatedItemListDto() { ApprovalAttachmentId = Id };
        //    ApprovalAttachmentWithRelated.RelatedEntity = _context.RelatedEntity.Where(f => f.ApprovalAttachmentId != null && f.ApprovalAttachmentId == Id)
        //        .ProjectTo<ApprovalAttachmentWithRelatedDto>(_mapper.ConfigurationProvider)
        //        .OrderBy(x => x.PropertyName).ToList();

        //    return ApprovalAttachmentWithRelated;
        //}

        public IRelatedItemList GetRelatedItemList(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(ApprovalAttachment entity)
        {
            try
            {
                return (await _context.ApprovalAttachments.CountAsync(x => x.ApprovalId == entity.ApprovalId &&
                  x.FileStoreId == entity.FileStoreId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(ApprovalAttachmentService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(ApprovalAttachment entity)
        {
            try
            {
                if (entity.Approval != null && entity.FileStoreDoc != null) return entity.Approval.ProjectId == ProjectId && entity.FileStoreDoc.ProjectId == ProjectId;
                if ((await _context.Approvals.Where(x => x.ApprovalId == entity.ApprovalId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.FileStoreDocs.Where(x => x.FileStoreDocId == entity.FileStoreId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(ApprovalAttachmentService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.ApprovalAttachments.CountAsync(x => lstIds.Contains(x.ApprovalAttachmentId) && (x.Approval.ProjectId != ProjectId || x.FileStoreDoc.ProjectId != ProjectId)) == 0;
        }
    }
}