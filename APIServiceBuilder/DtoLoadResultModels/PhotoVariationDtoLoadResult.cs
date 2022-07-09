using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cpDataORM.Dtos;

namespace cpDataASP.ControllerModels
{
    public class PhotoVariationDtoLoadResult
    {
        public List<PhotoVariationDto> data;
        public int totalCount;
        public int groupCount;
        public object[] summary;
    }
}