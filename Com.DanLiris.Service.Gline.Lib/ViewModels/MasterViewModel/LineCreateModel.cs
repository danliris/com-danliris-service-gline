using Com.DanLiris.Service.Gline.Lib.Utilities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel
{
    public class LineCreateModel : BaseCreateModel, IValidatableObject
    {
        public int nama_line { get; set; }
        public string nama_gedung { get; set; }
        public int id_unit { get; set; }
        public string kode_unit { get; set; }
        public string nama_unit { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (nama_line == 0)
            {
                yield return new ValidationResult("Nama line minimum value is more than zero", new List<string> { "nama_line" });
            }
            if (string.IsNullOrWhiteSpace(nama_gedung))
            {
                yield return new ValidationResult("Nama gedung is required", new List<string> { "nama_gedung" });
            }
            if (id_unit == 0)
            {
                yield return new ValidationResult("Unit is required", new List<string> { "id_unit" });
            }
            if (string.IsNullOrWhiteSpace(kode_unit))
            {
                yield return new ValidationResult("Unit is required", new List<string> { "kode_unit" });
            }

            if (string.IsNullOrWhiteSpace(nama_unit))
            {
                yield return new ValidationResult("Unit is required", new List<string> { "nama_unit" });
            }
        }
    }
}
