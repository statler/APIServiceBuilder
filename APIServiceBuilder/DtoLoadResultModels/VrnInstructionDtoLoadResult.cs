using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cpDataORM.Dtos;

namespace cpDataASP.ControllerModels
{
    public class VrnInstructionDtoLoadResult
    {
        public List<VrnInstructionDto> data;
        public int totalCount;
        public int groupCount;
        public object[] summary;
    }
}