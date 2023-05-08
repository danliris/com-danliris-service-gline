using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels
{
    public class SettingRoViewModel : BaseViewModel, IValidatableObject
    {
        public Guid Uid { get; set; }
        public string rono_view { get; set; }
        public double jam_target_view { get; set; }
        public double smv_view { get; set; }
        public string artikel_view { get; set; }
        public DateTime setting_date_view { get; set; }
        public TimeSpan setting_time_view { get; set; }
        public LineViewModel line_view { get; set; }
        public int quantity_view { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
