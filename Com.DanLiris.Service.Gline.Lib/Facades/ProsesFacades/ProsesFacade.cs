using Com.DanLiris.Service.Gline.Lib.Helpers;
using Com.DanLiris.Service.Gline.Lib.Interfaces;
using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
    }
}
