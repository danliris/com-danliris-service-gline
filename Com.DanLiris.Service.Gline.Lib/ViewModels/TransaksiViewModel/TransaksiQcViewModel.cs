using Com.DanLiris.Service.Gline.Lib.Utilities;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.TransaksiViewModel
{
    public class TransaksiQcViewModel : BaseViewModel, IValidatableObject
    {
        public string npk { get; set; }
        public string nama { get; set; }
        public Guid id_line { get; set; }
        public int nama_line { get; set; }
        public Guid id_setting_ro { get; set; }
        public string rono { get; set; }
        public int quantity { get; set; }
        public Guid? id_proses { get; set; }
        public string nama_proses { get; set; }
        public bool? pass { get; set; }
        public TimeSpan? pass_time { get; set; }
        public bool? reject { get; set; }
        public TimeSpan? reject_time { get; set; }
        public string npk_reject { get; set; }
        public string nama_reject { get; set; }
        public Guid? id_proses_reject { get; set; }
        public string nama_proses_reject { get; set; }
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

            if (string.IsNullOrWhiteSpace(nama_proses) && (bool)pass)
            {
                yield return new ValidationResult("Nama proses is required", new List<string> { "nama_proses" });
            }

            if (string.IsNullOrWhiteSpace(nama_proses_reject) && (bool)reject)
            {
                yield return new ValidationResult("Nama proses is required", new List<string> { "nama_proses" });
            }
        }
    }
}
