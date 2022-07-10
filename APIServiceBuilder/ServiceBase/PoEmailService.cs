
using AutoMapper;
using AutoMapper.QueryableExtensions;
using cpModel.Models;
using cpDataORM.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cpDataServices.Models;
using cpDataServices.Exceptions;
using cpModel.Enums;

namespace cpDataServices.Services
{
    public interface IPoEmailService : IServiceBase<PoEmail>
    {
    }

    public partial class PoEmailService : AbstractService<PoEmail>, IPoEmailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public PoEmailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<PoEmail> GetEntitiesForProjectQry()
        {
            return _context.PoEmails.Where(x => x.EmailLog.ProjectId == ProjectId && x.PurchaseOrder.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.PoEmailId == Id).CountAsync();
            //if (c > 0) lstRelatedItems.Add($" links ({c})");

            return lstRelatedItems;
        }

        public async Task DeleteAsync(List<int> lstIdsToDelete, bool shouldCommit = true)
        {
            if (!(await CanDeleteAsync())) throw new AuthorizationException("Delete | Admin permission is required to delete this record.");
            try
            {
                if (lstIdsToDelete.Contains(int.MinValue)) lstIdsToDelete.Remove(int.MinValue);
                ////Dereference links
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PoEmailId ?? int.MinValue)).ForEachAsync(x => x.PoEmailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PoEmailId ?? int.MinValue)));
                ////Delete base objects

                _context.PoEmails.RemoveRange(_context.PoEmails.Where(x => lstIdsToDelete.Contains(x.PoEmailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting PoEmail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(PoEmail entity)
        {
            try
            {
                return (await _context.PoEmails.CountAsync(x => x.EmailLogId == entity.EmailLogId &&
                  x.PurchaseOrderId == entity.PurchaseOrderId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(PoEmailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(PoEmail entity)
        {
            try
            {
                if (entity.EmailLog != null && entity.PurchaseOrder != null) return entity.EmailLog.ProjectId == ProjectId && entity.PurchaseOrder.ProjectId == ProjectId;
                if ((await _context.EmailLogs.Where(x => x.EmailLogId == entity.EmailLogId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.PurchaseOrders.Where(x => x.PurchaseOrderId == entity.PurchaseOrderId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(PoEmailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.PoEmails.CountAsync(x => lstIds.Contains(x.PoEmailId) && (x.EmailLog.ProjectId != ProjectId || x.PurchaseOrder.ProjectId != ProjectId)) == 0;
        }
    }
}
