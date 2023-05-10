using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Models.SettingRoModel;
using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels;

namespace Com.DanLiris.Service.Gline.Lib.AutoMapperProfiles
{
    public class SettingRoProfile : BaseAutoMapperProfile
    {
        public SettingRoProfile()
        {
            CreateMap<SettingRo, SettingRoViewModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ReverseMap();
        }
    }
}
