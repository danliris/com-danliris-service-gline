using Com.DanLiris.Service.Gline.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel
{
    public class CostCalculationRoViewModel : BaseViewModel
    {
        public string RO_Number { get; set; }
        public int Quantity { get; set; }
        public double SMV_Sewing { get; set; }
        public string Article { get; set; }
    }
}
