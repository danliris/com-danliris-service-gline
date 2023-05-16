using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Interfaces
{
    public interface ITransaksiQcFacade
    {
        Tuple<List<TransaksiQc>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}");
        Task<dynamic> Create(TransaksiQc model, string username);
    }
}
