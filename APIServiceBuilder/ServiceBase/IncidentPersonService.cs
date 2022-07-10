
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
    public interface IIncidentPersonService : IServiceBase<IncidentPerson>
    {
    }

    public partial class IncidentPersonService : AbstractService<IncidentPerson>, IIncidentPersonService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public IncidentPersonService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<IncidentPerson> GetEntitiesForProjectQry()
        {
            return _context.IncidentPersons.Where(x => x.Incident.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.IncidentPersonId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.IncidentPersonId ?? int.MinValue)).ForEachAsync(x => x.IncidentPersonId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.IncidentPersonId ?? int.MinValue)));
                ////Delete base objects

                _context.IncidentPersons.RemoveRange(_context.IncidentPersons.Where(x => lstIdsToDelete.Contains(x.IncidentPersonId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting IncidentPerson (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(IncidentPerson entity)
        {
            try
            {
                return (await _context.IncidentPersons.CountAsync(x => x.IncidentId == entity.IncidentId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(IncidentPersonService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(IncidentPerson entity)
        {
            try
            {
                if (entity.Incident != null) return entity.Incident.ProjectId == ProjectId;
                if ((await _context.Incidents.Where(x => x.IncidentId == entity.IncidentId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(IncidentPersonService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.IncidentPersons.CountAsync(x => lstIds.Contains(x.IncidentPersonId) && (x.Incident.ProjectId != ProjectId)) == 0;
        }
    }
}
