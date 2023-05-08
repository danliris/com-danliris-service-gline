using Com.DanLiris.Service.Gline.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel
{
    public class LineViewModel : BaseViewModel, IValidatableObject
    {
        public Guid Uid { get; set; }
        public int nama_line_view { get; set; }
        public string nama_gedung_view { get; set; }
        public string kode_unit_view { get; set; }
        public string nama_unit_view { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (nama_line_view == 0)
            {
                yield return new ValidationResult("Nama Line is required", new List<string> { "nama_line_view" });
            }

            if (string.IsNullOrWhiteSpace(nama_gedung_view))
            {
                yield return new ValidationResult("Nama Gedung is required", new List<string> { "nama_gedung_view" });
            }

            if (string.IsNullOrWhiteSpace(kode_unit_view))
            {
                yield return new ValidationResult("Kode Unit is required", new List<string> { "kode_unit_view" });
            }

            if (string.IsNullOrWhiteSpace(nama_unit_view))
            {
                yield return new ValidationResult("Nama Unit is required", new List<string> { "nama_unit_view" });
            }
        }
    }
}
