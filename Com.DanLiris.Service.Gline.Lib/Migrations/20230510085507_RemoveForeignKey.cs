using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Com.DanLiris.Service.Gline.Lib.Migrations
{
    public partial class RemoveForeignKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SettingRo_Line_id_line",
                table: "SettingRo");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiOperator_Line_id_line",
                table: "TransaksiOperator");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiOperator_Proses_id_proses",
                table: "TransaksiOperator");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiOperator_SettingRo_id_setting_ro",
                table: "TransaksiOperator");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiQc_Line_id_line",
                table: "TransaksiQc");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiQc_Proses_id_proses",
                table: "TransaksiQc");

            migrationBuilder.DropForeignKey(
                name: "FK_TransaksiQc_SettingRo_id_setting_ro",
                table: "TransaksiQc");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiQc_id_line",
                table: "TransaksiQc");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiQc_id_proses",
                table: "TransaksiQc");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiQc_id_setting_ro",
                table: "TransaksiQc");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiOperator_id_line",
                table: "TransaksiOperator");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiOperator_id_proses",
                table: "TransaksiOperator");

            migrationBuilder.DropIndex(
                name: "IX_TransaksiOperator_id_setting_ro",
                table: "TransaksiOperator");

            migrationBuilder.DropIndex(
                name: "IX_SettingRo_id_line",
                table: "SettingRo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransaksiQc_id_line",
                table: "TransaksiQc",
                column: "id_line");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksiQc_id_proses",
                table: "TransaksiQc",
                column: "id_proses");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksiQc_id_setting_ro",
                table: "TransaksiQc",
                column: "id_setting_ro");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksiOperator_id_line",
                table: "TransaksiOperator",
                column: "id_line");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksiOperator_id_proses",
                table: "TransaksiOperator",
                column: "id_proses");

            migrationBuilder.CreateIndex(
                name: "IX_TransaksiOperator_id_setting_ro",
                table: "TransaksiOperator",
                column: "id_setting_ro");

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

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiOperator_Line_id_line",
                table: "TransaksiOperator",
                column: "id_line",
                principalTable: "Line",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiOperator_Proses_id_proses",
                table: "TransaksiOperator",
                column: "id_proses",
                principalTable: "Proses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiOperator_SettingRo_id_setting_ro",
                table: "TransaksiOperator",
                column: "id_setting_ro",
                principalTable: "SettingRo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiQc_Line_id_line",
                table: "TransaksiQc",
                column: "id_line",
                principalTable: "Line",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiQc_Proses_id_proses",
                table: "TransaksiQc",
                column: "id_proses",
                principalTable: "Proses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransaksiQc_SettingRo_id_setting_ro",
                table: "TransaksiQc",
                column: "id_setting_ro",
                principalTable: "SettingRo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
