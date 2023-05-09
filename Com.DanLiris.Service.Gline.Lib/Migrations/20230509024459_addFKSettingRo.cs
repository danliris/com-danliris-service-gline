using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Gline.Lib.Migrations
{
    public partial class addFKSettingRo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SettingRo_id_line",
                table: "SettingRo",
                column: "id_line");

            migrationBuilder.AddForeignKey(
                name: "FK_SettingRo_Line_id_line",
                table: "SettingRo",
                column: "id_line",
                principalTable: "Line",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettingRo_Line_id_line",
                table: "SettingRo");

            migrationBuilder.DropIndex(
                name: "IX_SettingRo_id_line",
                table: "SettingRo");
        }
    }
}
