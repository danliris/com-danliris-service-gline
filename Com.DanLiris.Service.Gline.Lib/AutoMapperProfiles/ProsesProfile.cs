using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;

namespace Com.DanLiris.Service.Gline.Lib.AutoMapperProfiles
{
    public class ProsesProfile : BaseAutoMapperProfile
    {
        public ProsesProfile()
        {
            CreateMap<Proses, ProsesViewModel>()
                .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
                .ReverseMap();
        }
    }
}
