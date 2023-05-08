using Com.DanLiris.Service.Gline.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Gline.Lib.Models;
using Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Interfaces
{
    public interface ISettingRoFacade
    {
        Tuple<List<SettingRo>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        SettingRo ReadById(Guid id);
        Task<int> Create(SettingRo model, string username);
        Task<int> Update(Guid id, SettingRo model, string user);
        int Delete(Guid id, string username);
        ReadResponse<object> GetRoLoader(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
    }
}
