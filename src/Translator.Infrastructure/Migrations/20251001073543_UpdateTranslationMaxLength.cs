using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Translator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTranslationMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "translation_value",
                table: "translations",
                type: "character varying(2500)",
                maxLength: 2500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "hash",
                table: "values",
                type: "character varying(2500)",
                maxLength: 2500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "languages",
                keyColumn: "id",
                keyValue: new Guid("383a4678-5a4a-faa4-d5fa-73e2f506ecfc"),
                column: "is_active",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "translation_value",
                table: "translations",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2500)",
                oldMaxLength: 2500);

            migrationBuilder.AlterColumn<string>(
                name: "hash",
                table: "values",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2500)",
                oldMaxLength: 2500);

            migrationBuilder.UpdateData(
                table: "languages",
                keyColumn: "id",
                keyValue: new Guid("383a4678-5a4a-faa4-d5fa-73e2f506ecfc"),
                column: "is_active",
                value: false);
        }
    }
}
