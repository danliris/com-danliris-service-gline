using AutoMapper;
using Com.DanLiris.Service.Gline.Lib.Models;
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
                .ForMember(d => d.rono_view, opt => opt.MapFrom(s => s.rono))
                .ForMember(d => d.jam_target_view, opt => opt.MapFrom(s => s.jam_target))
                .ForMember(d => d.smv_view, opt => opt.MapFrom(s => s.smv))
                .ForMember(d => d.artikel_view, opt => opt.MapFrom(s => s.artikel))
                .ForMember(d => d.setting_date_view, opt => opt.MapFrom(s => s.setting_date))
                .ForMember(d => d.setting_time_view, opt => opt.MapFrom(s => s.setting_time))
                .ForMember(d => d.quantity_view, opt => opt.MapFrom(s => s.quantity))
                .ForPath(d => d.line_view.Uid, opt => opt.MapFrom(s => s.id_line))
                .ForPath(d => d.line_view.kode_unit_view, opt => opt.MapFrom(s => s.kode_unit))
                .ForPath(d => d.line_view.nama_unit_view, opt => opt.MapFrom(s => s.nama_line))
                .ForPath(d => d.line_view.nama_gedung_view, opt => opt.MapFrom(s => s.nama_gedung))
                .ForMember(d => d._id, opt => opt.Ignore())
                .ForMember(d => d.Id, opt => opt.Ignore())
                .ForPath(d => d.line_view._id, opt => opt.Ignore())
                .ForPath(d => d.line_view.Id, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
