using Com.DanLiris.Service.Gline.Lib.Helpers;
using Com.DanLiris.Service.Gline.Lib.Helpers.ReadResponse;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.ReworkModel;
using Com.DanLiris.Service.Gline.Lib.Models.SettingRoModel;
using Com.DanLiris.Service.Gline.Lib.Models.TransaksiModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels;
using Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Facades.SettingRoFacades
{
    public class SettingRoFacade : ISettingRoFacade
    {
        private readonly GlineDbContext dbContext;
        private ISalesDbContext salesDbContext;
        private readonly DbSet<SettingRo> dbSet;
        private readonly DbSet<TransaksiOperator> dbSetTransaksiOperator;
        private readonly DbSet<SummaryOperator> dbSetSummaryOperator;
        private readonly DbSet<Rework> dbSetRework;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";

        public SettingRoFacade(GlineDbContext dbContext, ISalesDbContext salesDbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.salesDbContext = salesDbContext;
            this.dbSet = dbContext.Set<SettingRo>();
            this.dbSetTransaksiOperator = dbContext.Set<TransaksiOperator>();
            this.dbSetSummaryOperator = dbContext.Set<SummaryOperator>();
            this.dbSetRework = dbContext.Set<Rework>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<SettingRo>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<SettingRo> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "rono", "nama_gedung", "nama_unit"
            };

            Query = QueryHelper<SettingRo>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query
               .Select(s => new SettingRo
               {
                   Id = s.Id,
                   rono = s.rono,
                   artikel = s.artikel,
                   smv = s.smv,
                   quantity = s.quantity,
                   nama_gedung = s.nama_gedung,
                   nama_line = s.nama_line,
               });


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<SettingRo>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<SettingRo>.ConfigureOrder(Query, OrderDictionary);

            Pageable<SettingRo> pageable = new Pageable<SettingRo>(Query, Page - 1, Size);
            List<SettingRo> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public SettingRo ReadById(Guid id)
        {
            var Result = dbSet.Where(m => m.Id == id).FirstOrDefault();
            return Result;
        }

        public async Task<int> Create(SettingRo model, string username)
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

        public async Task<int> Update(Guid id, SettingRo model, string user)
        {
            int Updated = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var existingModel = this.dbSet.AsNoTracking()
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    if (existingModel != null && id == model.Id)
                    {
                        EntityExtension.FlagForUpdate(model, user, USER_AGENT);

                        this.dbContext.Update(model);

                        Updated = await dbContext.SaveChangesAsync();
                        var updatedModel = this.dbSet.AsNoTracking()
                           .SingleOrDefault(pr => pr.Id == model.Id && !pr.IsDeleted);
                        transaction.Commit();
                    }
                    else
                    {
                        throw new Exception("Invalid Id");
                    }
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    while (e.InnerException != null) e = e.InnerException;
                    throw e;
                }
            }

            return Updated;
        }

        public int Delete(Guid id, string username)
        {
            int Deleted = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    var model = this.dbSet
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    EntityExtension.FlagForDelete(model, username, USER_AGENT);

                    Deleted = dbContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    while (e.InnerException != null) e = e.InnerException;
                    throw e;
                }
            }

            return Deleted;
        }

        public ReadResponse<object> GetRoLoader(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            string cmd = "";
            List<CostCalculationRoViewModel> data = new List<CostCalculationRoViewModel>();
            List<SqlParameter> param = new List<SqlParameter>();

            cmd = $"SELECT TOP {Size} RO_Number, Quantity, SMV_Sewing, Article FROM CostCalculationGarments WHERE RO_Number LIKE @key AND IsDeleted = 0";

            param.Add(new SqlParameter("key", "%" + Keyword + "%"));
            var reader = salesDbContext.ExecuteReader(cmd, param);

            while (reader.Read())
            {
                data.Add(new CostCalculationRoViewModel
                {
                    RO_Number = reader["RO_Number"].ToString(),
                    Quantity = Convert.ToInt32(reader["Quantity"].ToString()),
                    SMV_Sewing = Convert.ToDouble(reader["SMV_Sewing"].ToString()),
                    Article = reader["Article"].ToString()
                });
            }

            Pageable<CostCalculationRoViewModel> pageable = new Pageable<CostCalculationRoViewModel>(data, Page - 1, Size);
            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            List<CostCalculationRoViewModel> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            List<object> ListData = new List<object>();

            ListData.AddRange(Data.Select(s => new
            {
                s.RO_Number,
                s.Quantity,
                s.SMV_Sewing,
                s.Article,
            }));


            return new ReadResponse<object>(ListData, TotalData, OrderDictionary);
        }

        public Tuple<List<RoOngoingViewModel>, int> GetRoOngoingOp(string keyword = null, string Filter = "{}")
        {
            IQueryable<SettingRo> Query = dbSet;
            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);

            Guid id_line = Guid.Empty;
            string npk = "";
            Guid id_proses = Guid.Empty;
            int totalPerHari = 0;

            bool hasIdLineFilter = FilterDictionary.ContainsKey("id_line") && Guid.TryParse(FilterDictionary["id_line"], out id_line);
            bool hasNpkFilter = FilterDictionary.ContainsKey("npk") && !String.IsNullOrWhiteSpace(FilterDictionary["npk"]);
            bool hasIdProsesFilter = FilterDictionary.ContainsKey("id_proses") && Guid.TryParse(FilterDictionary["id_proses"], out id_proses);

            npk = hasNpkFilter ? FilterDictionary["npk"] : "";

            if (hasIdLineFilter && hasNpkFilter)
            {
                totalPerHari = TotalPerHariCount(id_line, npk);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
                Query = Query.Where(entity => entity.rono.Contains(keyword));

            var readForRoOngoingOp = Query.Where(x =>
                    (!hasIdLineFilter ? true : x.id_line == id_line) &&
                    x.IsDeleted == false
                ).ToList();

            var readForSummaryOperator = dbSetSummaryOperator.Where(x =>
                    (!hasNpkFilter ? true : x.npk == npk) &&
                    (!hasIdProsesFilter ? true : x.id_proses == id_proses) &&
                    x.IsDeleted == false
                ).ToList();

            if (readForSummaryOperator.Count > 0)
            {
                var queryJoin =
                    from settingRo in readForRoOngoingOp
                    join summaryOperator in readForSummaryOperator
                    on settingRo.Id equals summaryOperator.id_ro into finalData
                    from resultData in finalData.DefaultIfEmpty()
                    where resultData.jml_pass_per_ro < settingRo.quantity
                    || resultData.total_rework >= 1
                    select new RoOngoingViewModel
                    (
                        settingRo.rono,
                        settingRo.jam_target,
                        settingRo.smv,
                        settingRo.artikel,
                        settingRo.setting_date,
                        settingRo.setting_time,
                        settingRo.nama_unit,
                        resultData.jml_pass_per_ro,
                        totalPerHari,
                        resultData.total_rework,
                        resultData.total_waktu_pengerjaan.TotalSeconds
                    );

                var result = queryJoin.ToList();

                return Tuple.Create(result, result.Count);
            }

            var query = readForRoOngoingOp.Select(x => new RoOngoingViewModel(
                    x.rono,
                    x.jam_target,
                    x.smv,
                    x.artikel,
                    x.setting_date,
                    x.setting_time,
                    x.nama_unit,
                    0,
                    0,
                    0,
                    0
                )).ToList();

            return Tuple.Create(query, query.Count);
        }

        private int TotalPerHariCount (Guid id_line, string npk)
        {
            return dbSetTransaksiOperator.Where(x => x.id_line == id_line && x.npk == npk && x.CreatedUtc.Date == DateTime.Now.Date).ToList().Count;
        }
    }
}
