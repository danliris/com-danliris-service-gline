using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Gline.Lib.Migrations
{
    public partial class AddProsesColumnToRework : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id_proses",
                table: "Rework",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "nama_proses",
                table: "Rework",
                maxLength: 255,
                nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id_proses",
                table: "Rework");

            migrationBuilder.DropColumn(
                name: "nama_proses",
                table: "Rework");
        }
    }
}
