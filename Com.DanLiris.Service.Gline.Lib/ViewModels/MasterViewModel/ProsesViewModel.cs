using Com.DanLiris.Service.Gline.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel
{
    public class ProsesViewModel : BaseViewModel
    {
        public string nama_proses { get; set; }
        public double cycle_time { get; set; }
    }
}
