using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace AspNetFileUpload.Migrations
{
    public partial class Dimensione : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Chiave",
                table: "Fotografie",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Dimensione",
                table: "Fotografie",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Chiave",
                table: "Fotografie");

            migrationBuilder.DropColumn(
                name: "Dimensione",
                table: "Fotografie");
        }
    }
}
