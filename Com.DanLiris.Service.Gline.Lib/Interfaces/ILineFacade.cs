using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
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
    }
}
