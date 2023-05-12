using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels
{
    public class RoOngoingViewModel
    {
        public string _rono { get; set; }
        public int _jam_target { get; set; }
        public double _smv { get; set; }
        public string _artikel { get; set; }
        public DateTime _setting_date { get; set; }
        public TimeSpan _setting_time { get; set; }
        public string _nama_unit { get; set; }
        public int _total_output_ro { get; set; }
        public int _total_output_hari { get; set; }
        public int _total_rework { get; set; }
        public TimeSpan _total_pengerjaan { get; set; }

        public RoOngoingViewModel(
            string rono, int jam_target, double smv, string artikel, DateTime setting_date,
            TimeSpan setting_time, string nama_unit, int total_output_ro, int total_output_hari,
            int total_rework, TimeSpan total_pengerjaan)
        {
            _rono = rono;
            _jam_target = jam_target;
            _smv = smv;
            _artikel = artikel;
            _setting_date = setting_date;
            _setting_time = setting_time;
            _nama_unit = nama_unit;
            _total_output_ro = total_output_ro;
            _total_output_hari = total_output_hari;
            _total_rework = total_rework;
            _total_pengerjaan = total_pengerjaan;
        }
    }
}
