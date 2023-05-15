using Com.DanLiris.Service.Gline.Lib.Utilities;
using System;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.ReworkViewModel
{
    public class ReworkTimeCreateModel : BaseCreateModel
    {
        public TimeSpan jam_awal { get; set; }
        public TimeSpan jam_akhir { get; set; }
    }
}
