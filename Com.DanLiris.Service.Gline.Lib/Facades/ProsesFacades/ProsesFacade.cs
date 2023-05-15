using Com.DanLiris.Service.Gline.Lib.Helpers;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.IntegrationViewModel;
using Com.DanLiris.Service.Gline.Lib.ViewModels.MasterViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Gline.Lib.Facades.ProsesFacades
{
    public class ProsesFacade : IProsesFacade
    {
        private readonly GlineDbContext dbContext;
        private readonly DbSet<Proses> dbSet;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";

        public ProsesFacade(GlineDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<Proses>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<Proses>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<Proses> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "nama_proses"
            };

            Query = QueryHelper<Proses>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query
               .Select(s => new Proses
               {
                   Id = s.Id,
                   nama_proses = s.nama_proses,
                   cycle_time = s.cycle_time
               });


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<Proses>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<Proses>.ConfigureOrder(Query, OrderDictionary);

            Pageable<Proses> pageable = new Pageable<Proses>(Query, Page - 1, Size);
            List<Proses> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public Proses ReadById(Guid id)
        {
            var Result = dbSet.Where(m => m.Id == id).FirstOrDefault();
            return Result;
        }

        public async Task<int> Create(Proses model, string username)
        {
            int Created = 0;

            using (var transaction = this.dbContext.Database.BeginTransaction())
            {
                try
                {
                    model.Id = Guid.NewGuid();
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

        public async Task<int> Update(Guid id, Proses model, string user)
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

        public List<string> CsvHeader { get; } = new List<string>()
        {
            "Proses",
            "Cycle Time"
        };

        public sealed class ProsesMap : CsvHelper.Configuration.ClassMap<ProsesCsvViewModel>
        {
            public ProsesMap()
            {
                Map(p => p.nama_proses).Index(0);
                Map(p => p.cycle_time).Index(1);
            }
        }

        public async Task<List<ProsesViewModel>> MapCsvToViewModel(List<ProsesCsvViewModel> data)
        {
            List<ProsesViewModel> items = new List<ProsesViewModel>();

            foreach (var i in data)
            {
                ProsesViewModel item = new ProsesViewModel
                {
                    nama_proses = i.nama_proses,
                    cycle_time = string.IsNullOrWhiteSpace(i.cycle_time) ? 0 : Convert.ToDouble(i.cycle_time)
                };

                items.Add(item);
            }

            return items;
        }

        public Tuple<bool, List<object>> UploadValidate(ref List<ProsesCsvViewModel> data, List<KeyValuePair<string, StringValues>> list)
        {
            List<object> ErrorList = new List<object>();
            string ErrorMessage;
            bool Valid = true;

            IQueryable<Proses> Query = this.dbSet;

            foreach (ProsesCsvViewModel item in data)
            {
                ErrorMessage = "";

                var isExist = Query.Where(s => s.nama_proses == item.nama_proses).ToList();

                if (isExist.Count > 0)
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Proses Is Already Exist, ");
                }

                if (string.IsNullOrWhiteSpace(item.nama_proses))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Nama Proses Is Required, ");
                }

                double cycle_time = 0;
                if (string.IsNullOrWhiteSpace(item.cycle_time))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Cycle Time Is Required, ");
                }
                else
                {
                    if (!double.TryParse(item.cycle_time, out cycle_time))
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Cycle Time Must Be Numeric, ");
                    }
                }

                if (cycle_time <= 0)
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Cycle Time Must Be Greater Than 0, ");
                }


                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    ErrorMessage = ErrorMessage.Remove(ErrorMessage.Length - 2);
                    var Error = new ExpandoObject() as IDictionary<string, object>;
                    Error.Add("Proses", item.nama_proses);
                    Error.Add("Cycle Time", item.cycle_time);
                    Error.Add("Error", ErrorMessage);

                    ErrorList.Add(Error);
                }
            }

            if (ErrorList.Count > 0)
            {
                Valid = false;
            }

            return Tuple.Create(Valid, ErrorList);
        }

        public async Task<int> UploadData(List<Proses> data, string username)
        {
            foreach (var i in data)
            {
                EntityExtension.FlagForCreate(i, username, USER_AGENT);
                dbSet.Add(i);
            }

            var result = await dbContext.SaveChangesAsync();

            return result;
        }
    }
}
