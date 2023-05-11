using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels.TransaksiViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.AutoMapperProfiles
{
    public class TransaksiOperatorProfile : BaseAutoMapperProfile
    {
        public TransaksiOperatorProfile()
        {
            CreateMap<TransaksiOperator, TransaksiOperatorViewModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ReverseMap();

            CreateMap<TransaksiOperatorCreateModel, TransaksiOperator>()
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
