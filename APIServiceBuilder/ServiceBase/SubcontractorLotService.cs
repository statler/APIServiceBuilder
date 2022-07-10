
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
    public interface ISubcontractorLotService : IServiceBase<SubcontractorLot>
    {
    }

    public partial class SubcontractorLotService : AbstractService<SubcontractorLot>, ISubcontractorLotService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public SubcontractorLotService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<SubcontractorLot> GetEntitiesForProjectQry()
        {
            return _context.SubcontractorLots.Where(x => x.Lot.ProjectId == ProjectId && x.Subcontractor.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.SubcontractorLotId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.SubcontractorLotId ?? int.MinValue)).ForEachAsync(x => x.SubcontractorLotId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.SubcontractorLotId ?? int.MinValue)));
                ////Delete base objects

                _context.SubcontractorLots.RemoveRange(_context.SubcontractorLots.Where(x => lstIdsToDelete.Contains(x.SubcontractorLotId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting SubcontractorLot (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(SubcontractorLot entity)
        {
            try
            {
                return (await _context.SubcontractorLots.CountAsync(x => x.LotId == entity.LotId &&
                  x.SubcontractorId == entity.SubcontractorId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(SubcontractorLotService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(SubcontractorLot entity)
        {
            try
            {
                if (entity.Lot != null && entity.Subcontractor != null) return entity.Lot.ProjectId == ProjectId && entity.Subcontractor.ProjectId == ProjectId;
                if ((await _context.Lots.Where(x => x.LotId == entity.LotId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Subcontractors.Where(x => x.SubcontractorId == entity.SubcontractorId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(SubcontractorLotService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.SubcontractorLots.CountAsync(x => lstIds.Contains(x.SubcontractorLotId) && (x.Lot.ProjectId != ProjectId || x.Subcontractor.ProjectId != ProjectId)) == 0;
        }
    }
}
