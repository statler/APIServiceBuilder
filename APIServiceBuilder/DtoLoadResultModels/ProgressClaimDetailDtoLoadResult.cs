using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cpDataORM.Dtos;

namespace cpDataASP.ControllerModels
{
    public class ProgressClaimDetailDtoLoadResult
    {
        public List<ProgressClaimDetailDto> data;
        public int totalCount;
        public int groupCount;
        public object[] summary;
    }
}