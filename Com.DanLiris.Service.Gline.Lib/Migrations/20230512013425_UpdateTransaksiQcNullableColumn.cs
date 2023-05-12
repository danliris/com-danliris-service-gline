using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Gline.Lib.Migrations
{
    public partial class UpdateTransaksiQcNullableColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "id_proses",
                table: "TransaksiQc",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "id_proses_reject",
                table: "TransaksiQc",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nama_proses_reject",
                table: "TransaksiQc",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "id_proses_reject",
                table: "TransaksiQc");

            migrationBuilder.DropColumn(
                name: "nama_proses_reject",
                table: "TransaksiQc");

            migrationBuilder.AlterColumn<Guid>(
                name: "id_proses",
                table: "TransaksiQc",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);
        }
    }
}
