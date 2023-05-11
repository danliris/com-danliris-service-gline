using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Interfaces
{
    public interface IProsesFacade
    {
        Tuple<List<Proses>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        Proses ReadById(Guid id);
        Task<int> Create(Proses model, string username);
        Task<int> Update(Guid id, Proses model, string user);
        int Delete(Guid id, string username);
        List<string> CsvHeader { get; }
        Task<List<ProsesViewModel>> MapCsvToViewModel(List<ProsesCsvViewModel> data);
        Tuple<bool, List<object>> UploadValidate(ref List<ProsesCsvViewModel> data, List<KeyValuePair<string, StringValues>> list);
        Task<int> UploadData(List<Proses> data, string username);
    }
}
