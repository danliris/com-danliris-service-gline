using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;

namespace Com.DanLiris.Service.Gline.Lib.AutoMapperProfiles
{
    public class ProsesProfile : BaseAutoMapperProfile
    {
        public ProsesProfile()
        {
            CreateMap<Proses, ProsesViewModel>()
                .ForMember(d => d.Uid, opt => opt.MapFrom(s => s.Id))
                .ForMember(d => d._id, opt => opt.Ignore())
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
