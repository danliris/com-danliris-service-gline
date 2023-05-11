using Com.DanLiris.Service.Gline.Lib.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels
{
    public class ShiftViewModel
    {
        public string name { get; set; }
        public TimeSpan from { get; set; }
        public TimeSpan to { get; set; }
        public ShiftGroup group { get; set; }
    }
}
