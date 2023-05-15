using Com.DanLiris.Service.Gline.Lib.Models.ReworkModel;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Interfaces
{
    public interface ITransaksiOperatorFacade
    {
        Tuple<List<TransaksiOperator>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        Task<dynamic> Create(TransaksiOperator model, string username);
        Task<int> DoRework(ReworkTime model, string username, string npk, Guid id_ro, Guid id_line, Guid id_proses);
    }
}
