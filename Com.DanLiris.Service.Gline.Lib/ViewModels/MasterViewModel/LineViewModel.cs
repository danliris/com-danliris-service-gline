using Com.DanLiris.Service.Gline.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel
{
    public class LineViewModel : BaseViewModel
    {
        public int nama_line { get; set; }
        public string nama_gedung { get; set; }
        public string kode_unit { get; set; }
        public string nama_unit { get; set; }
    }
}
