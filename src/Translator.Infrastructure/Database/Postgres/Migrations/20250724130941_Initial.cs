using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Translator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "language",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    unicode_range = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_language", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    hash = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "template_value",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    hash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template_value", x => x.id);
                    table.ForeignKey(
                        name: "fk_template_value_template_template_id",
                        column: x => x.template_id,
                        principalTable: "template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translation",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_value_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translation", x => x.id);
                    table.ForeignKey(
                        name: "fk_translation_language_language_id",
                        column: x => x.language_id,
                        principalTable: "language",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_translation_template_value_template_value_id",
                        column: x => x.template_value_id,
                        principalTable: "template_value",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_language_code",
                table: "language",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_template_hash",
                table: "template",
                column: "hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_template_value_hash",
                table: "template_value",
                column: "hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_template_value_template_id",
                table: "template_value",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ix_translation_language_id",
                table: "translation",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "ix_translation_template_value_id",
                table: "translation",
                column: "template_value_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "translation");

            migrationBuilder.DropTable(
                name: "language");

            migrationBuilder.DropTable(
                name: "template_value");

            migrationBuilder.DropTable(
                name: "template");
        }
    }
}
