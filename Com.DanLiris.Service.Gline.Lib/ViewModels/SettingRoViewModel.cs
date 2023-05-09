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
                yield return new ValidationResult("Target Jam must be greater than 0", new List<string> { "target_jam" });
            }

            if (smv <= 0)
            {
                yield return new ValidationResult("SMV must be greater than 0", new List<string> { "smv" });
            }

            if(setting_date.Date < DateTime.Now.Date)
            {
                yield return new ValidationResult($"Setting Date must be equal or greater than {DateTime.Now.Day}", new List<string> { "setting_date" });
            } 

            if(line.nama_line == 0)
            {
                yield return new ValidationResult("Line is required", new List<string> { "line" });
            }
        }
    }
}
