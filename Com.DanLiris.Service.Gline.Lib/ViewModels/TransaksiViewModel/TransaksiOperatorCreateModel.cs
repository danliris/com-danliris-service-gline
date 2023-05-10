using Com.DanLiris.Service.Gline.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.TransaksiViewModel
{
    public class TransaksiOperatorCreateModel : BaseViewModel, IValidatableObject
    {
        public string npk { get; set; }
        public string nama { get; set; }
        public string employee_role { get; set; }
        public Guid id_line { get; set; }
        public int nama_line { get; set; }
        public Guid id_setting_ro { get; set; }
        public string rono { get; set; }
        public DateTime setting_date { get; set; }
        public TimeSpan setting_time { get; set; }
        public int quantity { get; set; }
        public Guid id_proses { get; set; }
        public string nama_proses { get; set; }
        public double cycle_time { get; set; }
        public bool pass { get; set; }
        public TimeSpan pass_time { get; set; }
        public Guid id_shift { get; set; }
        public string nama_shift { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(npk))
            {
                yield return new ValidationResult("Npk is required", new List<string> { "npk" });
            }

            if (string.IsNullOrWhiteSpace(nama))
            {
                yield return new ValidationResult("Nama is required", new List<string> { "nama" });
            }

            if (nama_line == 0)
            {
                yield return new ValidationResult("Nama line is required", new List<string> { "nama_line" });
            }

            if (string.IsNullOrWhiteSpace(rono))
            {
                yield return new ValidationResult("Rono is required", new List<string> { "rono" });
            }

            if (string.IsNullOrWhiteSpace(nama_proses))
            {
                yield return new ValidationResult("Nama proses is required", new List<string> { "nama_proses" });
            }
        }
    }
}
