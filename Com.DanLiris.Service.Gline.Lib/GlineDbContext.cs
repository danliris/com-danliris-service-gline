using Com.DanLiris.Service.Gline.Lib.Models;
using Com.DanLiris.Service.Gline.Lib.Models.MasterModel;
using Com.Moonlay.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Com.DanLiris.Service.Gline.Lib
{
    public class GlineDbContext : StandardDbContext
    {
        public GlineDbContext(DbContextOptions<GlineDbContext> options) : base(options)
        {
        }

        public DbSet<Proses> Proses { get; set; }
        public DbSet<Line> Line { get; set; }
        public DbSet<SettingRo> SettingRo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}
