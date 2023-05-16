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
using System.Dynamic;
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

        public async Task<dynamic> Create(TransaksiQc model, string username)
        {
            dynamic returnResult = new ExpandoObject();
            returnResult.roOverflow = false;
            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    model.Id = Guid.NewGuid();
                    EntityExtension.FlagForCreate(model, username, USER_AGENT);
                    dbSet.Add(model);

                    var summaryQcData = dbSetSummaryQc.Where(i => i.rono == model.rono).FirstOrDefault();
                    var summaryOptData = dbSetSummaryOpt.Where(i => i.rono == model.rono).ToList();

                    if(summaryOptData != null && summaryOptData.Count > 0)
                    {
                        var summaryPerOpt = summaryOptData.Where(x => x.npk == model.npk_reject && x.id_proses == model.id_proses_reject).FirstOrDefault();

                        if (summaryQcData == null)
                        {
                            if ((bool)model.reject)
                            {
                                if(summaryPerOpt != null)
                                {
                                    var reworkInsert = GenerateRework(model, username);
                                    dbSetRework.Add(reworkInsert);

                                    returnResult.total_pass_per_hari = 0;
                                    returnResult.total_reject_per_hari = reworkInsert.qty_rework;

                                    summaryPerOpt.total_rework++;
                                    EntityExtension.FlagForUpdate(summaryPerOpt, username, USER_AGENT);
                                    dbContext.Update(summaryPerOpt);
                                }
                                else
                                {
                                    returnResult.roOverflow = true;
                                    return returnResult;
                                }
                            } 
                            else
                            {
                                returnResult.total_pass_per_hari = 1;
                                returnResult.total_reject_per_hari = 0;
                            }

                            var summaryQcInsert = GenerateSummaryQc(model, username);
                            EntityExtension.FlagForCreate(summaryQcInsert, username, USER_AGENT);

                            dbSetSummaryQc.Add(summaryQcInsert); 

                        }
                        else
                        {
                            if ((summaryQcData.total_pass + 1) > model.quantity)
                            {
                                returnResult.roOverflow = true;
                                return returnResult;
                            }

                            EntityExtension.FlagForUpdate(summaryQcData, username, USER_AGENT);

                            if ((bool)model.pass)
                            {
                                summaryQcData.total_pass++;
                                EntityExtension.FlagForUpdate(summaryQcData, username, USER_AGENT);
                                dbContext.Update(summaryQcData);
                                
                                returnResult.total_pass_per_hari = TotalPassPerHariQc(model.id_line, model.rono);
                                returnResult.total_reject_per_hari = TotalRejectPerHariQc(model.id_line, model.rono);

                            }
                            else
                            {
                                if (summaryPerOpt != null)
                                {
                                    summaryQcData.total_reject++;
                                    EntityExtension.FlagForUpdate(summaryQcData, username, USER_AGENT);

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
                                        EntityExtension.FlagForCreate(reworkInsert, username, USER_AGENT);
                                        dbSetRework.Add(reworkInsert);
                                    }

                                    summaryPerOpt.total_rework++;
                                    EntityExtension.FlagForUpdate(summaryPerOpt, username, USER_AGENT);
                                    dbContext.Update(summaryPerOpt);

                                    returnResult.total_pass_per_hari = TotalPassPerHariQc(model.id_line, model.rono);
                                    returnResult.total_reject_per_hari = TotalRejectPerHariQc(model.id_line, model.rono);
                                }
                                else
                                {
                                    returnResult.roOverflow = true;
                                    return returnResult;
                                }

                                dbContext.Update(summaryQcData);
                            }
                        }
                    }
                    else
                    {
                        returnResult.roOverflow = true;
                        return returnResult;
                    }

                    await dbContext.SaveChangesAsync();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    while (e.InnerException != null) e = e.InnerException;
                    throw e;
                }
            }

            return returnResult;
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

        private int TotalPassPerHariQc(Guid id_line, string ro)
        {
            return dbSet.Where(x => x.id_line == id_line && x.rono == ro && x.CreatedUtc.Date == DateTime.Now.Date && x.pass == true).ToList().Count;
        }

        private int TotalRejectPerHariQc(Guid id_line, string ro)
        {
            return dbSet.Where(x => x.id_line == id_line && x.rono == ro && x.CreatedUtc.Date == DateTime.Now.Date && x.reject == true).ToList().Count;
        }

    }
}

