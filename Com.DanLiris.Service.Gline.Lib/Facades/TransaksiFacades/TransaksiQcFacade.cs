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
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Facades.TransaksiFacades
{
    public class TransaksiQcFacade : ITransaksiQcFacade
    {
        private readonly GlineDbContext dbContext;
        private readonly DbSet<TransaksiQc> dbSet;
        private readonly DbSet<SummaryQc> dbSetSummaryQc;
        private readonly DbSet<SummaryOperator> dbSetSummaryOpt;
        private readonly DbSet<Rework> dbSetRework;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";

        public TransaksiQcFacade(GlineDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            dbSet = dbContext.Set<TransaksiQc>();
            dbSetRework = dbContext.Set<Rework>();
            dbSetSummaryQc = dbContext.Set<SummaryQc>();
            dbSetSummaryOpt = dbContext.Set<SummaryOperator>();

            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<TransaksiQc>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<TransaksiQc> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "nama", "rono", "nama_proses", "npk_reject", "nama_reject", "nama_shift"
            };

            Query = QueryHelper<TransaksiQc>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query
               .Select(s => new TransaksiQc
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
                   reject = s.reject,
                   reject_time = s.reject_time,
                   npk_reject = s.npk_reject,
                   nama_reject = s.nama_reject,
                   nama_shift = s.nama_shift
               });


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<TransaksiQc>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<TransaksiQc>.ConfigureOrder(Query, OrderDictionary);

            Pageable<TransaksiQc> pageable = new Pageable<TransaksiQc>(Query, Page - 1, Size);
            List<TransaksiQc> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public async Task<int> Create(TransaksiQc model, string username)
        {
            int Modified = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    model.Id = Guid.NewGuid();
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);
                    this.dbSet.Add(model);

                    var summaryQcData = dbSetSummaryQc.Where(i => i.rono == model.rono).FirstOrDefault();
                    var summaryOptData = dbSetSummaryOpt.Where(i => i.rono == model.rono).ToList();

                    if(summaryOptData != null)
                    {
                        //var lowestQty = summaryOptData.OrderBy(x => x.jml_pass_per_ro).FirstOrDefault();
                        var summaryPerOpt = summaryOptData.Where(x => x.npk == model.npk_reject && x.id_proses == model.id_proses_reject).FirstOrDefault();

                        //if (lowestQty.jml_pass_per_ro > 0) 
                        //{
                            if (summaryQcData == null)
                            {
                                if ((bool)model.reject)
                                {
                                    if(summaryPerOpt != null)
                                    {
                                        var reworkInsert = GenerateRework(model, username);
                                        dbSetRework.Add(reworkInsert);

                                        summaryPerOpt.total_rework++;
                                        EntityExtension.FlagForUpdate(summaryPerOpt, username, USER_AGENT);
                                        dbContext.Update(summaryPerOpt);
                                    }
                                    else
                                    {
                                        return -1;
                                    }
                                }

                                var summaryQcInsert = GenerateSummaryQc(model, username);
                                EntityExtension.FlagForCreate(summaryQcInsert, username, USER_AGENT);

                                dbSetSummaryQc.Add(summaryQcInsert); 

                            }
                            else
                            {
                                if ((summaryQcData.total_pass + 1) > model.quantity)
                                {
                                    return -1;
                                }

                                EntityExtension.FlagForUpdate(summaryQcData, username, USER_AGENT);

                                if ((bool)model.pass)
                                {
                                    summaryQcData.total_pass++;
                                    dbContext.Update(summaryQcData);
                                }
                                else
                                {
                                    if (summaryPerOpt != null)
                                    {
                                        summaryQcData.total_reject++;
                                        var reworkOperator = dbSetRework.Where(i => i.npk == model.npk_reject && i.rono == model.rono && i.id_line == model.id_line && i.id_proses == model.id_proses_reject).FirstOrDefault();
                                        if (reworkOperator != null)
                                        {
                                            reworkOperator.qty_rework++;
                                            EntityExtension.FlagForUpdate(reworkOperator, username, USER_AGENT);
                                            dbContext.Update(reworkOperator);
                                        }
                                        else
                                        {
                                            var reworkInsert = GenerateRework(model, username);
                                            dbSetRework.Add(reworkInsert);
                                        }

                                        summaryPerOpt.total_rework++;
                                        dbContext.Update(summaryPerOpt);
                                        EntityExtension.FlagForUpdate(summaryPerOpt, username, USER_AGENT);
                                    }
                                    else
                                    {
                                        return -1;
                                    }

                                    dbContext.Update(summaryQcData);
                                }
                            }
                        //}
                        //else
                        //{
                        //    return -1;
                        //}
                    }
                    else
                    {
                        return -1;
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

        private SummaryQc GenerateSummaryQc(TransaksiQc transaksiQc, string username)
        {
            var summaryQc = new SummaryQc();
            EntityExtension.FlagForCreate(summaryQc, username, USER_AGENT);

            summaryQc.nama = transaksiQc.nama;
            summaryQc.npk = transaksiQc.npk;
            summaryQc.total_pass = (bool)transaksiQc.pass ? 1 : 0;
            summaryQc.total_reject = (bool)transaksiQc.reject ? 1 : 0;
            summaryQc.rono = transaksiQc.rono;
            summaryQc.setting_date = transaksiQc.setting_date;
            summaryQc.setting_time = transaksiQc.setting_time;

            return summaryQc;
        }

        private Rework GenerateRework(TransaksiQc transaksiQc, string username)
        {
            var rework = new Rework();
            EntityExtension.FlagForCreate(rework, username, USER_AGENT);

            rework.id_line = transaksiQc.id_line;
            rework.id_ro = transaksiQc.id_setting_ro;
            rework.nama_line = transaksiQc.nama_line;
            rework.nama_operator = transaksiQc.nama_reject;
            rework.npk = transaksiQc.npk_reject;
            rework.id_proses = (Guid)transaksiQc.id_proses_reject;
            rework.nama_proses = transaksiQc.nama_proses_reject;
            rework.qty_rework = 1;
            rework.rono = transaksiQc.rono;

            return rework;
        }

    }
}

