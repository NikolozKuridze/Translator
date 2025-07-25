using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
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

            migrationBuilder.InsertData(
                table: "language",
                columns: new[] { "id", "code", "name", "unicode_range" },
                values: new object[,]
                {
                    { new Guid("043f9914-0813-6885-a59d-a0a92c8f0696"), "pa", "Gurmukhi", "0A00-0A7F" },
                    { new Guid("054fc828-c5db-ff21-a522-20ce03b16356"), "mn", "Mongolian", "1800-18AF" },
                    { new Guid("07ba433f-bf05-1e77-a5d7-4a7a2e8b40c0"), "su", "Sundanese", "1B80-1BBF" },
                    { new Guid("0dee066e-20ac-76ef-16ad-7d1d547d1b34"), "mr", "Spacing Modifier Letters", "02B0-02FF" },
                    { new Guid("0fd12294-475b-c6ec-1ebf-221545cb1420"), "syl", "Syloti Nagri", "A800-A82F" },
                    { new Guid("1273e9fe-426d-9a38-c960-82e9ca6961fa"), "tl", "Tagalog", "1700-171F" },
                    { new Guid("130867f0-0902-b369-9ff8-d85744b40400"), "ta", "Tamil", "0B80-0BFF" },
                    { new Guid("13121d03-b2bd-9831-df28-7449fdbd1afb"), "cr", "Unified Canadian Aboriginal Syllabics", "1400-167F" },
                    { new Guid("16ad1b7f-58db-70be-c295-9aab31dd5d86"), "dv", "Thaana", "0780-07BF" },
                    { new Guid("19a02285-fe10-c443-6204-1b4451ca6ad2"), "mni", "Meetei Mayek Extensions", "AAE0-AAFF" },
                    { new Guid("1af8c1c6-6646-8563-fa7d-85df7eaf8763"), "he", "Hebrew", "0590-05FF" },
                    { new Guid("1b69af13-a92b-e967-10e6-8c3dacee3db2"), "doi", "Takri", "11680-116CF" },
                    { new Guid("2343f32f-acbb-8290-6743-1c6af2da2884"), "cja", "Cham", "AA00-AA5F" },
                    { new Guid("32b3e701-0748-de76-57da-01822d0d6a76"), "peo", "Old Persian", "103A0-103DF" },
                    { new Guid("3393a5d1-e5c7-0562-fed9-bda9da34b201"), "gu", "Gujarati", "0A80-0AFF" },
                    { new Guid("33f941a4-02e0-556f-ccff-38b534f98a53"), "bo", "Tibetan", "0F00-0FFF" },
                    { new Guid("3d75c0ad-6437-d11e-17f7-b999a0ae79a0"), "ka", "Georgian", "10A0-10FF" },
                    { new Guid("4ca57f21-35c9-0415-fcde-32147bff005d"), "my", "Myanmar", "1000-109F" },
                    { new Guid("57be1faa-cc7e-ca29-7b97-0d4151dfe682"), "or", "Oriya", "0B00-0B7F" },
                    { new Guid("61200e30-f81c-9040-3229-25e1a4f72c2d"), "sat", "Ol Chiki", "1C50-1C7F" },
                    { new Guid("66643064-4a7b-779c-4180-bc4926f411e6"), "phn", "Phoenician", "10900-1091F" },
                    { new Guid("6bee55d0-420f-dc8c-46d1-088080692a29"), "mai", "Tirhuta", "11480-114DF" },
                    { new Guid("6daf947d-eacc-73d2-6bd1-d456ba3c750a"), "jv", "Javanese", "A980-A9DF" },
                    { new Guid("75a2b5f9-45bc-626b-ad13-e4a537f62a3b"), "ml", "Malayalam", "0D00-0D7F" },
                    { new Guid("84cb6da5-c243-8e5f-14b4-7b8ff1748331"), "bn", "Bengali", "0980-09FF" },
                    { new Guid("85cb2c27-ba92-e0fd-baf1-dedce0fd3ac2"), "ar", "Arabic", "0600-06FF" },
                    { new Guid("8697668a-7cc7-2982-44af-012803ce7ab5"), "ja", "Hiragana", "3040-309F" },
                    { new Guid("86c52e42-de6b-c50f-eaf6-9b3651080bd9"), "hi", "Devanagari", "0900-097F" },
                    { new Guid("888ab815-cfe6-a416-462c-1562fdcb0c19"), "te", "Telugu", "0C00-0C7F" },
                    { new Guid("89c7dd43-eabc-7741-a147-91f6c9a03b28"), "th", "Thai", "0E00-0E7F" },
                    { new Guid("8b154491-2019-84a0-e125-a1e4f7f632b2"), "ber", "Tifinagh", "2D30-2D7F" },
                    { new Guid("9131af4b-e843-7b25-c8f7-061e54525078"), "ru", "Cyrillic", "0400-04FF" },
                    { new Guid("99946b14-bc40-20d4-ae08-9773a8051903"), "cu", "Glagolitic", "2C00-2C5F" },
                    { new Guid("9d37f00c-1d50-d551-f961-ee306c842157"), "so", "Osmanya", "10480-104AF" },
                    { new Guid("a0df8c3d-a857-197c-1caa-67b1677e376c"), "chr", "Cherokee", "13A0-13FF" },
                    { new Guid("a257f3da-1a5d-cd1e-206f-3e3dc505f4c2"), "vai", "Vai", "A500-A63F" },
                    { new Guid("a2f907d3-caba-22c2-dad2-d388d32bf4b3"), "xng", "Zanabazar Square", "11A00-11A4F" },
                    { new Guid("a988ef13-d894-8657-69d2-e77a6de2f287"), "ii", "Yijing Hexagram Symbols", "4DC0-4DFF" },
                    { new Guid("aa56b1a0-8c68-b0fc-babd-6e866e766521"), "lis", "Lisu", "A4D0-A4FF" },
                    { new Guid("b394563e-9fd8-54a1-e9af-34b1fc6f0ce7"), "non", "Runic", "16A0-16FF" },
                    { new Guid("ba4e569f-9cbc-b9f0-589b-4ecf6228379a"), "km", "Khmer", "1780-17FF" },
                    { new Guid("bac3dea2-633b-dc59-fdf5-b96d4de71455"), "ko", "Hangul Jamo", "1100-11FF" },
                    { new Guid("bec6cfb4-6fc9-54cb-6504-55f269f39b71"), "syc", "Syriac", "0700-074F" },
                    { new Guid("bff4d41e-a13d-5cfa-2992-d62659dcd91f"), "hoc", "Warang Citi", "118A0-118FF" },
                    { new Guid("c0396d8b-f79f-54ec-4e5e-6e3949a29b1f"), "zh", "Hanunoo", "1720-173F" },
                    { new Guid("c63ddf57-f0b6-3cbe-0879-cfb539b61169"), "sd", "Khudawadi", "112B0-112FF" },
                    { new Guid("c76abc4b-5ede-0523-42a5-e78b4cf1614c"), "en", "Basic Latin", "0000-007F" },
                    { new Guid("c8d85589-9764-2a3a-0d58-7a064e7379b5"), "el", "Greek and Coptic", "0370-03FF" },
                    { new Guid("ca97a393-c926-5790-202f-168312e5c7d5"), "am", "Ethiopic", "1200-137F" },
                    { new Guid("d356fdd8-06ba-de72-0f66-25bfb1b4ca7b"), "lo", "Lao", "0E80-0EFF" },
                    { new Guid("d3ed97b3-7757-f6a9-0361-19e6ced96ff6"), "hy", "Armenian", "0530-058F" },
                    { new Guid("dc1f516b-8ae4-2774-4a35-e3365e36230c"), "kn", "Kannada", "0C80-0CFF" },
                    { new Guid("e254332a-0ed5-bfc4-80ba-1fbcd253506e"), "si", "Sinhala", "0D80-0DFF" },
                    { new Guid("e2f8815d-6c09-cd99-6c2a-83a1166f29ad"), "nqo", "NKo", "07C0-07FF" }
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
