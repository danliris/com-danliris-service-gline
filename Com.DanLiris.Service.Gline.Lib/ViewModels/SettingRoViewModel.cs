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
        public string rono { get; set; }
        public double jam_target { get; set; }
        public double smv { get; set; }
        public string artikel { get; set; }
        public DateTime setting_date { get; set; }
        public TimeSpan setting_time { get; set; }
        public LineViewModel line { get; set; }
        public int quantity { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(rono))
            {
                yield return new ValidationResult("RONo is required", new List<string> { "nama_line" });
            }

            if (jam_target <= 0)
            {
                yield return new ValidationResult("Target must be higher than 0", new List<string> { "nama_line" });
            }

            if (smv <= 0)
            {
                yield return new ValidationResult("SMV must be higher than 0", new List<string> { "nama_line" });
            }
        }
    }
}
