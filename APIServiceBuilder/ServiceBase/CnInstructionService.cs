
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
    public interface ICnInstructionService : IServiceBase<CnInstruction>
    {
    }

    public partial class CnInstructionService : AbstractService<CnInstruction>, ICnInstructionService
    {
        public override List<PermissionDomainEnum> LstServiceDomain => throw new NotImplementedException();

        public override bool EditorCanDelete => throw new NotImplementedException();

        public CnInstructionService(cpContext context,
            IMapper mapper,
            IUserService userservice)
        {
            _context = context;
            _userService = userservice;
            _mapper = mapper;
        }

        public override IQueryable<CnInstruction> GetEntitiesForProjectQry()
        {
            return _context.CnInstructions.Where(x => x.ContractNotice.ProjectId == ProjectId && x.Instruction.ProjectId == ProjectId);
        }

        public async Task<List<string>> DeleteCheckAsync(int Id)
        {
            List<string> lstRelatedItems = new List<string>();
            //var c = await _context.RelatedEntity.Where(x => x.CnInstructionId == Id).CountAsync();
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
                //await _context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnInstructionId ?? int.MinValue)).ForEachAsync(x => x.CnInstructionId = null);
                ////Delete links
                //_context.RelatedEntity.RemoveRange(_context.RelatedEntity.Where(x => lstIdsToDelete.Contains(x.CnInstructionId ?? int.MinValue)));
                ////Delete base objects

                _context.CnInstructions.RemoveRange(_context.CnInstructions.Where(x => lstIdsToDelete.Contains(x.CnInstructionId)));

                if (shouldCommit) await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting CnInstruction (DeleteAsync)");
                throw;
            }
        }

        public async Task<IRelatedItemList> GetRelatedItemListAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public async override Task<bool> IsEntityUniqueAsync(CnInstruction entity)
        {
            try
            {
                return (await _context.CnInstructions.CountAsync(x => x.CnId == entity.CnId &&
                  x.InstructionId == entity.InstructionId &&
                  x.UniqueId != entity.UniqueId)) == 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsEntityUniqueAsync(CnInstructionService)");
                return false;
            }
        }

        public async override Task<bool> IsProjectValidAsync(CnInstruction entity)
        {
            try
            {
                if (entity.ContractNotice != null && entity.Instruction != null) return entity.ContractNotice.ProjectId == ProjectId && entity.Instruction.ProjectId == ProjectId;
                if ((await _context.ContractNotices.Where(x => x.ConId == entity.CnId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                if ((await _context.Instructions.Where(x => x.InstructionId == entity.InstructionId && x.ProjectId == ProjectId).CountAsync()) != 1) return false;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in IsProjectValidAsync(CnInstructionService)");
                return false;
            }
        }

        public async override Task<bool> CheckIdsInCurrentProjectAsync(List<int> lstIds)
        {
            return await _context.CnInstructions.CountAsync(x => lstIds.Contains(x.CnInstructionId) && (x.ContractNotice.ProjectId != ProjectId || x.Instruction.ProjectId != ProjectId)) == 0;
        }
    }
}
