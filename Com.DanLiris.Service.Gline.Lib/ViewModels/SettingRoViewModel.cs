using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels
{
    public class SettingRoViewModel : BaseViewModel, IValidatableObject
    {
        public string rono { get; set; }
        public double jam_target { get; set; }
        public double smv { get; set; }
        public string artikel { get; set; }
        public DateTime setting_date { get; set; }
        public TimeSpan setting_time { get; set; }
        public Guid id_line { get; set; }
        public int nama_line { get; set; }
        public string nama_gedung { get; set; }
        public string kode_unit { get; set; }
        public string nama_unit { get; set; }
        public int quantity { get; set; }
        public bool isEdit { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            GlineDbContext dbContext = validationContext == null ? null : (GlineDbContext)validationContext.GetService(typeof(GlineDbContext));

            if (string.IsNullOrWhiteSpace(rono))
            {
                yield return new ValidationResult("RONo is required", new List<string> { "rono" });
            }

            if (jam_target <= 0)
            {
                yield return new ValidationResult("Target Jam must be greater than 0", new List<string> { "target_jam" });
            }

            if (smv <= 0)
            {
                yield return new ValidationResult("SMV must be greater than 0", new List<string> { "smv" });
            }

            if (nama_line == 0)
            {
                yield return new ValidationResult("Line is required", new List<string> { "line" });
            }

            if (setting_date.Date < DateTime.Now.Date)
            {
                yield return new ValidationResult($"Setting Date must be equal or greater than {DateTime.Now.Date}", new List<string> { "setting_date" });
            }

            var duplicateRONo = dbContext.SettingRo.Where(x => x.rono == rono && x.id_line == id_line).Count();
            if (duplicateRONo > 0 && !isEdit)
            {
                yield return new ValidationResult("RONo is already exist", new List<string> { "rono" });
            }

            if(quantity == 0)
            {
                yield return new ValidationResult("Quantity must be greater than 0", new List<string> { "quantity" });
            }

        }
    }
}
