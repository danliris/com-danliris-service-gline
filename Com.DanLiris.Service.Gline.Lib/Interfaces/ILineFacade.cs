using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Interfaces
{
    public interface ILineFacade
    {
        Tuple<List<Line>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        Line ReadById(Guid id);
        Task<int> Create(Line model, string username);
        Task<int> Update(Guid id, Line model, string user);
        int Delete(Guid id, string username);
        List<string> CsvHeader { get; }
        Task<List<LineViewModel>> MapCsvToViewModel(List<LineCsvViewModel> data);
        Tuple<bool, List<object>> UploadValidate(ref List<LineCsvViewModel> data, List<KeyValuePair<string, StringValues>> list);
        Task<int> UploadData(List<Line> data, string username);
    }
}
