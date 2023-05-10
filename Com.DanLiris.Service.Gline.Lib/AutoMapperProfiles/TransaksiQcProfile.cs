using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.TransaksiViewModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.AutoMapperProfiles
{
    public class TransaksiQcProfile : Profile
    {
        public TransaksiQcProfile()
        {
            CreateMap<TransaksiQc, TransaksiQcViewModel>()
               .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
               .ReverseMap();

            CreateMap<TransaksiQc, TransaksiQcCreateModel>()
               .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
               .ReverseMap();
        }
    }
}
