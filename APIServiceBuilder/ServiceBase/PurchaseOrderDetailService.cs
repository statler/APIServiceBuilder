
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
    public interface IPurchaseOrderDetailService : IServiceBase<PurchaseOrderDetail>
    {
    }

    public partial class PurchaseOrderDetailService : AbstractService<PurchaseOrderDetail>, IPurchaseOrderDetailService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public PurchaseOrderDetailService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<PurchaseOrderDetail> GetEntitiesForProjectQry()
        {
            return _context.PurchaseOrderDetails.Where(x => x.CostCode.ProjectId == ProjectId && x.PurchaseOrder.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.PoDetailId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PoDetailId ?? int.MinValue)).ForEachAsync(x => x.PoDetailId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.PoDetailId ?? int.MinValue)));
                ////Delete base objects

                _context.PurchaseOrderDetails.RemoveRange(_context.PurchaseOrderDetails.Where(x => lstIdsToDelete.Contains(x.PoDetailId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting PurchaseOrderDetail (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(PurchaseOrderDetail entity)
        {
            try
            {
                return (await _context.PurchaseOrderDetails.CountAsync(x => x.CostCodeId == entity.CostCodeId &&
                  x.PurchaseOrderId == entity.PurchaseOrderId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(PurchaseOrderDetailService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(PurchaseOrderDetail entity)
        {
            try
            {
                if (entity.CostCode != null && entity.PurchaseOrder != null) return entity.CostCode.ProjectId == ProjectId && entity.PurchaseOrder.ProjectId == ProjectId;
                if ((await _context.CostCodes.Where(x => x.CostCodeId == entity.CostCodeId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.PurchaseOrders.Where(x => x.PurchaseOrderId == entity.PurchaseOrderId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(PurchaseOrderDetailService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.PurchaseOrderDetails.CountAsync(x => lstIds.Contains(x.PoDetailId) && (x.CostCode.ProjectId != ProjectId || x.PurchaseOrder.ProjectId != ProjectId)) == 0;
        }
    }
}
