using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Gline.Lib.Migrations
{
    public partial class CreateSettingRo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettingRo",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Active = table.Column<bool>(nullable: false),
                    CreatedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    CreatedUtc = table.Column<DateTime>(nullable: false),
                    DeletedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedBy = table.Column<string>(maxLength: 255, nullable: false),
                    DeletedUtc = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    LastModifiedAgent = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedBy = table.Column<string>(maxLength: 255, nullable: false),
                    LastModifiedUtc = table.Column<DateTime>(nullable: false),
                    artikel = table.Column<string>(maxLength: 1000, nullable: false),
                    id_line = table.Column<Guid>(nullable: false),
                    jam_target = table.Column<int>(nullable: false),
                    kode_unit = table.Column<string>(maxLength: 32, nullable: false),
                    nama_gedung = table.Column<string>(maxLength: 32, nullable: false),
                    nama_line = table.Column<int>(nullable: false),
                    nama_unit = table.Column<string>(maxLength: 50, nullable: false),
                    rono = table.Column<string>(maxLength: 50, nullable: false),
                    setting_date = table.Column<DateTime>(nullable: false),
                    setting_time = table.Column<TimeSpan>(nullable: false),
                    smv = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingRo", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettingRo");
        }
    }
}
