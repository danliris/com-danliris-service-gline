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
                .ForMember(d => d.Uid, opt => opt.MapFrom(s => s.Id))
                .ForPath(d => d.line.Uid, opt => opt.MapFrom(s => s.id_line))
                .ForPath(d => d.line.kode_unit, opt => opt.MapFrom(s => s.kode_unit))
                .ForPath(d => d.line.nama_unit, opt => opt.MapFrom(s => s.nama_unit))
                .ForPath(d => d.line.nama_gedung, opt => opt.MapFrom(s => s.nama_gedung))
                .ForPath(d => d.line.nama_line, opt => opt.MapFrom(s => s.nama_line))
                .ForMember(d => d._id, opt => opt.Ignore())
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForPath(d => d.line._id, opt => opt.Ignore())
                .ForPath(d => d.line.Id, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
