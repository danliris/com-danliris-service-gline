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

namespace Com.DanLiris.Service.Gline.Lib.Facades.LineFacades
{
    public class LineFacade : ILineFacade
    {
        private readonly GlineDbContext dbContext;
        private readonly DbSet<Line> dbSet;
        public readonly IServiceProvider serviceProvider;

        private string USER_AGENT = "Facade";

        public LineFacade(GlineDbContext dbContext, IServiceProvider serviceProvider)
        {
            this.dbContext = dbContext;
            this.dbSet = dbContext.Set<Line>();
            this.serviceProvider = serviceProvider;
        }

        public Tuple<List<Line>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
        {
            IQueryable<Line> Query = this.dbSet;

            List<string> searchAttributes = new List<string>()
            {
                "nama_gedung", "kode_unit", "nama_unit"
            };

            Query = QueryHelper<Line>.ConfigureSearch(Query, searchAttributes, Keyword);

            Query = Query
               .Select(s => new Line
               {
                   Id = s.Id,
                   nama_line = s.nama_line,
                   nama_gedung = s.nama_gedung,
                   kode_unit = s.kode_unit,
                   nama_unit = s.nama_unit
               });


            Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
            Query = QueryHelper<Line>.ConfigureFilter(Query, FilterDictionary);

            Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
            Query = QueryHelper<Line>.ConfigureOrder(Query, OrderDictionary);

            Pageable<Line> pageable = new Pageable<Line>(Query, Page - 1, Size);
            List<Line> Data = pageable.Data.ToList();
            int TotalData = pageable.TotalCount;

            return Tuple.Create(Data, TotalData, OrderDictionary);
        }

        public Line ReadById(Guid id)
        {
            var Result = dbSet.Where(m => m.Id == id).FirstOrDefault();
            return Result;
        }

        public async Task<int> Create(Line model, string username)
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

        public async Task<int> Update(Guid id, Line model, string user)
        {
            int Updated = 0;

            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    var existingModel = dbSet.AsNoTracking()
                        .SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

                    if (existingModel != null && id == model.Id)
                    {
                        EntityExtension.FlagForUpdate(model, user, USER_AGENT);

                        dbContext.Update(model);

                        Updated = await dbContext.SaveChangesAsync();
                        var updatedModel = dbSet.AsNoTracking()
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
            "Nama Gedung",
            "Kode Unit",
            "Nama Unit",
            "Nama Line"
        };

        public sealed class LineMap : CsvHelper.Configuration.ClassMap<LineCsvViewModel>
        {
            public LineMap()
            {
                Map(p => p.nama_gedung).Index(0);
                Map(p => p.kode_unit).Index(1);
                Map(p => p.nama_unit).Index(2);
                Map(p => p.nama_line).Index(3);
            }
        }

        public async Task<List<LineViewModel>> MapCsvToViewModel(List<LineCsvViewModel> data)
        {
            List<LineViewModel> items = new List<LineViewModel>();

            foreach (var i in data)
            {
                LineViewModel item = new LineViewModel
                {
                    nama_gedung = i.nama_gedung,
                    kode_unit = i.kode_unit,
                    nama_unit = i.nama_unit,
                    nama_line = string.IsNullOrWhiteSpace(i.nama_line) ? 0 : Convert.ToInt32(i.nama_line)
                };

                items.Add(item);
            }

            return items;
        }

        public Tuple<bool, List<object>> UploadValidate(ref List<LineCsvViewModel> data, List<KeyValuePair<string, StringValues>> list)
        {
            List<object> ErrorList = new List<object>();
            string ErrorMessage;
            bool Valid = true;

            IQueryable<Line> Query = this.dbSet;

            foreach (LineCsvViewModel item in data)
            {
                ErrorMessage = "";

                if (string.IsNullOrWhiteSpace(item.nama_gedung))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Nama Gedung Is Required, ");
                }

                if (string.IsNullOrWhiteSpace(item.kode_unit))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Kode Unit Is Required, ");
                }

                if (string.IsNullOrWhiteSpace(item.nama_unit))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Nama Unit Is Required, ");
                }

                int nama_line = 0;
                if (string.IsNullOrWhiteSpace(item.nama_line))
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Nama Line Is Required, ");
                }
                else
                {
                    if (!int.TryParse(item.nama_line, out nama_line))
                    {
                        ErrorMessage = string.Concat(ErrorMessage, "Nama Line Must Be Numeric, ");
                    }
                }

                var isExist = Query
                    .Where(s => s.nama_gedung == item.nama_gedung
                    && s.nama_unit == item.nama_unit
                    && s.nama_line == nama_line).ToList();

                if (isExist.Count > 0)
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Line Is Already Exist, ");
                }

                if (nama_line <= 0)
                {
                    ErrorMessage = string.Concat(ErrorMessage, "Nama Line Must Be Greater Than 0, ");
                }


                if (!string.IsNullOrEmpty(ErrorMessage))
                {
                    ErrorMessage = ErrorMessage.Remove(ErrorMessage.Length - 2);
                    var Error = new ExpandoObject() as IDictionary<string, object>;
                    Error.Add("Nama Gedung", item.nama_gedung);
                    Error.Add("Kode Unit", item.kode_unit);
                    Error.Add("Nama Unit", item.nama_unit);
                    Error.Add("Nama Line", item.nama_line);
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

        public async Task<int> UploadData(List<Line> data, string username)
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
