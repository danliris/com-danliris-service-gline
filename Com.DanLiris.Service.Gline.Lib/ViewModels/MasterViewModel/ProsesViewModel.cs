using Com.DanLiris.Service.Gline.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel
{
    public class ProsesViewModel : BaseViewModel, IValidatableObject
    {
        public Guid Uid { get; set; }
        public string nama_proses { get; set; }
        public double cycle_time { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(nama_proses))
            {
                yield return new ValidationResult("Nama proses is required", new List<string> { "nama_proses_view" });
            }

            if (cycle_time == 0)
            {
                yield return new ValidationResult("Cycle time minimum value is more than zero", new List<string> { "cycle_time_view" });
            }
        }
    }
}
