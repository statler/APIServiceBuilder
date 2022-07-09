
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
    public interface ICnIncidentService : IServiceBase<CnIncident>
    {
    }

    public partial class CnIncidentService : AbstractService<CnIncident>, ICnIncidentService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnIncidentService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnIncident> GetEntitiesForProjectQry()
        {
            return _context.CnIncidents.Where(x => x.ContractNotice.ProjectId == ProjectId && x.Incident.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnIncidentId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnIncidentId ?? int.MinValue)).ForEachAsync(x => x.CnIncidentId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnIncidentId ?? int.MinValue)));
                ////Delete base objects

                _context.CnIncidents.RemoveRange(_context.CnIncidents.Where(x => lstIdsToDelete.Contains(x.CnIncidentId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnIncident (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnIncident entity)
        {
            try
            {
                return (await _context.CnIncidents.CountAsync(x => x.CnId == entity.CnId &&
                  x.IncidentId == entity.IncidentId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnIncidentService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnIncident entity)
        {
            try
            {
                if (entity.ContractNotice != null && entity.Incident != null) return entity.ContractNotice.ProjectId == ProjectId && entity.Incident.ProjectId == ProjectId;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Incidents.Where(x => x.IncidentId == entity.IncidentId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnIncidentService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnIncidents.CountAsync(x => lstIds.Contains(x.CnIncidentId) && (x.ContractNotice.ProjectId != ProjectId || x.Incident.ProjectId != ProjectId)) == 0;
        }
    }
}