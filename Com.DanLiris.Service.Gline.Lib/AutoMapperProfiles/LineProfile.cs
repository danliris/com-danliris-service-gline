using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.AutoMapperProfiles
{
    public class LineProfile : BaseAutoMapperProfile
    {
        public LineProfile()
        {
            CreateMap<Line, LineViewModel>()
               .ForMember(d => d.Uid, opt => opt.MapFrom(s => s.Id))
               .ForMember(d => d._id, opt => opt.Ignore())
               .ForMember(d => d.Id, opt => opt.Ignore())
               .ReverseMap();
        }
    }
}
