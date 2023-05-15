using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Gline.Lib.Migrations
{
    public partial class CreateUniqueIndexProses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Proses_nama_proses",
                table: "Proses",
                column: "nama_proses",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Proses_nama_proses",
                table: "Proses");
        }
    }
}
