using Com.DanLiris.Service.Gline.Lib.Helpers;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.ReworkModel;
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
        private readonly DbSet<SummaryOperator> dbSetSummaryOperator;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";

        public TransaksiOperatorFacade(GlineDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<TransaksiOperator>();
            dbSetSummaryOperator = dbContext.Set<SummaryOperator>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<TransaksiOperator>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<TransaksiOperator> Query = dbSet;

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
            int Modified = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    model.Id = Guid.NewGuid();
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);
                    dbSet.Add(model);

                    var summaryOperatorData = dbSetSummaryOperator.Where(i => i.npk == model.npk && i.rono == model.rono && i.nama_proses == model.nama_proses).FirstOrDefault();

                    if (summaryOperatorData == null)
                    {
                        var summaryOperatorInsert = GenerateSummaryOperator(model, username);
                        dbSetSummaryOperator.Add(summaryOperatorInsert);
                       
                    }
                    else
                    {
                        if ((summaryOperatorData.jml_pass_per_ro + 1) > model.quantity)
                        {
                            return -1;
                        }
                        EntityExtension.FlagForUpdate(summaryOperatorData, username, USER_AGENT);
                        summaryOperatorData.jml_pass_per_ro++;
                        
                        dbContext.Update(summaryOperatorData);
                    }

                    Modified = await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    while (e.InnerException != null) e = e.InnerException;
                    throw e;
                }
            }

            return Modified;
        }

        private SummaryOperator GenerateSummaryOperator(TransaksiOperator transaksiOperator, string username)
        {
            var summaryOperator = new SummaryOperator();
            EntityExtension.FlagForCreate(summaryOperator, username, USER_AGENT);
            summaryOperator.npk = transaksiOperator.npk;
            summaryOperator.nama = transaksiOperator.nama;
            summaryOperator.jml_pass_per_ro = 1;
            summaryOperator.total_rework = 0;
            summaryOperator.total_waktu_pengerjaan = new TimeSpan(0, 0, 0);
            summaryOperator.id_ro = transaksiOperator.id_setting_ro;
            summaryOperator.rono = transaksiOperator.rono;
            summaryOperator.setting_date = transaksiOperator.setting_date;
            summaryOperator.id_proses = transaksiOperator.id_proses;
            summaryOperator.nama_proses = transaksiOperator.nama_proses;

            return summaryOperator;
        }



    }
}

