using Com.DanLiris.Service.Gline.Lib.Helpers;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Facades.TransaksiFacades
{
    public class TransaksiOperatorFacade : ITransaksiOperatorFacade
    {
        private readonly GlineDbContext dbContext;
        private readonly DbSet<TransaksiOperator> dbSet;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";

        public TransaksiOperatorFacade(GlineDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<TransaksiOperator>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<TransaksiOperator>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<TransaksiOperator> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "nama", "rono", "nama_proses", "nama_shift"
            };

            Query = QueryHelper<TransaksiOperator>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query
               .Select(s => new TransaksiOperator
               {
                   Id = s.Id,
                   npk = s.npk,
                   nama = s.nama,
                   nama_line = s.nama_line,
                   rono = s.rono,
                   quantity = s.quantity,
                   nama_proses = s.nama_proses,
                   pass = s.pass,
                   pass_time = s.pass_time,
                   nama_shift = s.nama_shift
               });


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<TransaksiOperator>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<TransaksiOperator>.ConfigureOrder(Query, OrderDictionary);

            Pageable<TransaksiOperator> pageable = new Pageable<TransaksiOperator>(Query, Page - 1, Size);
            List<TransaksiOperator> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public async Task<int> Create(TransaksiOperator model, string username)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);
                    this.dbSet.Add(model);
                    Created = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    while (e.InnerException != null) e = e.InnerException;
                    throw e;
                }
            }

            return Created;
        }

    }
}

