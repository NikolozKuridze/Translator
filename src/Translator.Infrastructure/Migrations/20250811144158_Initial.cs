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
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_categories_parent_id",
                        column: x => x.parent_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "languages",
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
                    table.PrimaryKey("pk_languages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hash = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_values", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateValues",
                columns: table => new
                {
                    templates_id = table.Column<Guid>(type: "uuid", nullable: false),
                    values_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_template_values", x => new { x.templates_id, x.values_id });
                    table.ForeignKey(
                        name: "fk_template_values_templates_templates_id",
                        column: x => x.templates_id,
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_template_values_values_values_id",
                        column: x => x.values_id,
                        principalTable: "values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_value_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_id = table.Column<Guid>(type: "uuid", nullable: false),
                    translation_value = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_translations_languages_language_id",
                        column: x => x.language_id,
                        principalTable: "languages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_translations_values_template_value_id",
                        column: x => x.template_value_id,
                        principalTable: "values",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "languages",
                columns: new[] { "id", "code", "name", "unicode_range" },
                values: new object[,]
                {
                    { new Guid("03f71d70-4f90-4f62-f641-904e870adb8a"), "si", "Singhalese", "0D80-0DFF" },
                    { new Guid("079217fc-0092-b705-dca8-5323ceacf5eb"), "ms", "Malay", "0000-007F" },
                    { new Guid("07ba433f-bf05-1e77-a5d7-4a7a2e8b40c0"), "su", "Sundanese", "1B80-1BBF" },
                    { new Guid("0f1a1741-36cd-e92c-8b5f-0f11398713b6"), "sv", "Swedish", "0000-007F" },
                    { new Guid("1273e9fe-426d-9a38-c960-82e9ca6961fa"), "tl", "Tagalog", "0000-007F" },
                    { new Guid("130867f0-0902-b369-9ff8-d85744b40400"), "ta", "Tamil", "0B80-0BFF" },
                    { new Guid("1531bc86-4eeb-7398-ac96-904a4a68e19e"), "de", "German", "0000-007F" },
                    { new Guid("15ac3458-7ad3-d4e7-8a51-83fd5d5020ab"), "ceb", "Cebuano", "0000-007F" },
                    { new Guid("1b704476-a4a4-fc9e-8d45-7334dd1480cf"), "ps", "Pashto", "0600-06FF" },
                    { new Guid("1e17c345-93f1-4bc7-402f-407fba9d955b"), "hi", "Hindi", "0900-097F" },
                    { new Guid("2069bade-06e7-4015-1385-fe1fb5a379ec"), "ru", "Russian", "0400-04FF" },
                    { new Guid("222a837e-3736-3a2b-c5e0-30c551d90bab"), "as", "Assamese", "0980-09FF" },
                    { new Guid("27e65ad0-c8fc-e708-df1e-e341c3d5cbc2"), "kk", "Kazakh", "0000-007F" },
                    { new Guid("2e11867b-40c6-d81f-f06a-b5251d1a68fe"), "hu", "Hungarian", "0000-007F" },
                    { new Guid("325987e4-cfc8-2b3d-4232-34a1d35b15e0"), "rw", "Kinyarwanda", "0000-007F" },
                    { new Guid("33670468-65e4-6591-a4f5-9a1918cb1408"), "zu", "Zulu", "0000-007F" },
                    { new Guid("3393a5d1-e5c7-0562-fed9-bda9da34b201"), "gu", "Gujarati", "0A80-0AFF" },
                    { new Guid("383a4678-5a4a-faa4-d5fa-73e2f506ecfc"), "en", "English", "0000-007F" },
                    { new Guid("3d75c0ad-6437-d11e-17f7-b999a0ae79a0"), "ka", "Georgian", "10A0-10FF" },
                    { new Guid("3dbe972d-6692-1f14-e879-08a0e213c09f"), "mad", "Madurese", "0000-007F" },
                    { new Guid("41449857-5765-df28-e9c7-cc33e12dfa4c"), "om", "Oromo", "0000-007F" },
                    { new Guid("431f13f8-14da-6541-3f7e-cb1fd144eb6a"), "ug", "Uighur", "0600-06FF" },
                    { new Guid("46eee5f7-6ad2-b97a-9e04-41dc3981cfdf"), "ff", "Fulani", "0000-007F" },
                    { new Guid("47f520ca-f6f2-bf5d-6d23-b0395419dd43"), "uz", "Uzbek", "0000-007F" },
                    { new Guid("495be493-a892-2948-fce5-681feeb81782"), "hmn", "Hmong", "0000-007F" },
                    { new Guid("49c13280-e4d0-d38a-8ef6-b60289f910cd"), "fa", "Persian", "0600-06FF" },
                    { new Guid("4c29c0bb-17e6-937b-2fad-9c75af2b8563"), "pa", "Punjabi", "0600-06FF" },
                    { new Guid("4e39a7ef-a7ca-70fc-76a9-da13a77236b8"), "ro", "Romanian", "0000-007F" },
                    { new Guid("5dca9214-7ef7-49f7-ff0b-b1ddd79a2016"), "be", "Belorussian", "0400-04FF" },
                    { new Guid("6266d16c-8654-fb44-2621-f46c01701334"), "sn", "Shona", "0000-007F" },
                    { new Guid("639a4f74-411b-b2e8-8e48-0b30ac012d21"), "hne", "Chhattisgarhi", "0900-097F" },
                    { new Guid("6aed2cf3-a19b-c464-b3c0-47fd1d7c882e"), "ja", "Japanese", "3040-30FF" },
                    { new Guid("6daf947d-eacc-73d2-6bd1-d456ba3c750a"), "jv", "Javanese", "A980-A9DF" },
                    { new Guid("6de0e84b-bc27-e1a7-828f-2fa9a49ca985"), "it", "Italian", "0000-007F" },
                    { new Guid("6e1c9b52-ffaf-343f-bcda-64708f72586d"), "mr", "Marathi", "0900-097F" },
                    { new Guid("705f22ad-0278-11ba-8c22-987186dd38e8"), "fr", "French", "0000-007F" },
                    { new Guid("75a2b5f9-45bc-626b-ad13-e4a537f62a3b"), "ml", "Malayalam", "0D00-0D7F" },
                    { new Guid("76b4ca8c-180f-7106-7db2-fbbaf2e99300"), "az", "Azeri", "0000-007F" },
                    { new Guid("77f0532f-96d3-f032-b325-d5eabfc8798b"), "ne", "Nepali", "0900-097F" },
                    { new Guid("788311b2-f7fb-c0f4-57b6-686207960f37"), "syl", "Sylheti", "0980-09FF" },
                    { new Guid("79b555c5-26aa-922e-dbdf-0fed7f7638ee"), "ig", "Igbo", "0000-007F" },
                    { new Guid("7c2ce330-f44c-e934-c75e-60c14c442541"), "pt", "Portuguese", "0000-007F" },
                    { new Guid("7dec9973-4a74-f90b-18af-8a1975bebeca"), "ht", "Haitian Creole", "0000-007F" },
                    { new Guid("7e36bf68-8f22-ba45-83cb-8831a5ee6447"), "nl", "Dutch", "0000-007F" },
                    { new Guid("8044ff83-f2cd-0a90-e83b-f30d95774d76"), "cs", "Czech", "0000-007F" },
                    { new Guid("84cb6da5-c243-8e5f-14b4-7b8ff1748331"), "bn", "Bengali", "0980-09FF" },
                    { new Guid("85cb2c27-ba92-e0fd-baf1-dedce0fd3ac2"), "ar", "Arabic", "0600-06FF" },
                    { new Guid("888ab815-cfe6-a416-462c-1562fdcb0c19"), "te", "Telugu", "0C00-0C7F" },
                    { new Guid("89c7dd43-eabc-7741-a147-91f6c9a03b28"), "th", "Thai", "0E00-0E7F" },
                    { new Guid("8a6d600c-16ee-dd70-6ea5-81d6cf3c1490"), "mai", "Maithili", "0900-097F" },
                    { new Guid("8a829935-f84a-ecea-0992-e0a4499e5fb2"), "ilo", "Ilocano", "0000-007F" },
                    { new Guid("9211126f-b39e-880a-188a-f058eb7dbcfb"), "tk", "Turkmen", "0000-007F" },
                    { new Guid("97a4af0a-7d80-c95a-b509-656bd13b5283"), "el", "Greek", "0370-03FF" },
                    { new Guid("97d69375-a3bf-98e7-e23b-ecb86cbf3503"), "ha", "Hausa", "0000-007F" },
                    { new Guid("98ab275d-9bea-f873-dddc-c2ccb35f7cdd"), "mos", "Mossi", "0000-007F" },
                    { new Guid("9b3830c7-d9c8-599e-c867-766babdd48b5"), "pl", "Polish", "0000-007F" },
                    { new Guid("9cf036ff-d9e3-b326-7729-48a4f72144dd"), "sd", "Sindhi", "0600-06FF" },
                    { new Guid("9d237997-3ac3-cb5f-9773-0b8be66c1f59"), "ny", "Chewa", "0000-007F" },
                    { new Guid("a0068ec9-050f-1d51-f3d6-f2cfd610cb70"), "my", "Burmese", "1000-109F" },
                    { new Guid("a019ab66-49e3-5b4a-e559-8b106d98d651"), "xh", "Xhosa", "0000-007F" },
                    { new Guid("a2ce43dd-1ac4-cc54-6536-8193e2343bf9"), "ak", "Akan", "0000-007F" },
                    { new Guid("a54d100b-9473-4797-64cc-7b3a42f2c1de"), "so", "Somali", "0000-007F" },
                    { new Guid("aabfe6d8-3770-8f8d-1f44-11816036927e"), "bho", "Bhojpuri", "0900-097F" },
                    { new Guid("b5d83579-c9cf-941f-cd6a-de801ebe5792"), "bal", "Baluchi", "0600-06FF" },
                    { new Guid("ba4e569f-9cbc-b9f0-589b-4ecf6228379a"), "km", "Khmer", "1780-17FF" },
                    { new Guid("bb640f00-bbdb-ac74-a435-5df9f3cfad78"), "mg", "Malagasy", "0000-007F" },
                    { new Guid("bb933266-1f74-fd93-e4a3-e63d58fb2166"), "hil", "Hiligaynon", "0000-007F" },
                    { new Guid("bd2b61c1-faeb-4645-7e8a-b814e08daff1"), "tr", "Turkish", "0000-007F" },
                    { new Guid("bda64eb9-33af-ba1e-45e8-96cbf7ba6f1f"), "awa", "Awadhi", "0900-097F" },
                    { new Guid("be2094bf-80f7-cf23-6e9a-b6ed3f67ada9"), "mag", "Magahi", "0900-097F" },
                    { new Guid("c0f49fce-0d41-af35-bc22-06ab23c8759d"), "yo", "Yoruba", "0000-007F" },
                    { new Guid("c16f8ae7-d64a-7a4f-7838-6b20568ce95b"), "uk", "Ukrainian", "0400-04FF" },
                    { new Guid("c38054cb-712e-8877-52b0-8ae1e8712775"), "es", "Spanish", "0000-007F" },
                    { new Guid("c9d92254-c7ce-f352-abda-e0263b0934ed"), "ctg", "Chittagonian", "0980-09FF" },
                    { new Guid("ccacafe9-53b9-4238-bb5a-087f300e97a1"), "sr", "Serbo-Croatian", "0400-04FF" },
                    { new Guid("cdb3bdd0-77e4-2ed8-766d-a05ebda50ccb"), "ko", "Korean", "AC00-D7AF" },
                    { new Guid("ce82fee6-840d-ee02-bb9d-8c987e9eb2a3"), "rn", "Kirundi", "0000-007F" },
                    { new Guid("dc1f516b-8ae4-2774-4a35-e3365e36230c"), "kn", "Kannada", "0C80-0CFF" },
                    { new Guid("de592a60-60fb-29b6-4f92-05bd6834c18a"), "kok", "Konkani", "0900-097F" },
                    { new Guid("dfaa9b83-24b7-6ee0-ac3b-12a431dd8358"), "za", "Zhuang", "0000-007F" },
                    { new Guid("e2e34403-efe6-3b15-d3b1-d4ecded8f8c9"), "har", "Haryanvi", "0900-097F" },
                    { new Guid("e8fa807b-4056-6cc1-db02-61bef0c27636"), "vi", "Vietnamese", "0000-007F" },
                    { new Guid("f0107ed5-4e47-0e5d-f552-beb0dedd033b"), "skr", "Saraiki", "0600-06FF" },
                    { new Guid("f2edb282-346e-097b-5177-f699237327c2"), "dhd", "Dhundhari", "0900-097F" },
                    { new Guid("f5b82228-c2aa-1a19-fe53-4140a6629eea"), "am", "Amharic", "1200-137F" },
                    { new Guid("f94df503-c396-78a4-db20-5176ca1967b6"), "zh", "Mandarin Chinese", "4E00-9FFF" },
                    { new Guid("f9657d62-4515-8f8d-2e10-c40cc38fab62"), "or", "Odia", "0B00-0B7F" },
                    { new Guid("f9c03ef3-dd81-1094-e1e0-ebc9da2c4b5e"), "ku", "Kurdish", "0000-007F" },
                    { new Guid("fbc50d05-ed2c-eef3-f331-0a88820ecaa0"), "qu", "Quechua", "0000-007F" },
                    { new Guid("fe7ee7a5-961a-ddeb-ec93-a38680791bf4"), "mwr", "Marwari", "0900-097F" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_id",
                table: "categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "ix_languages_code",
                table: "languages",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_templates_hash",
                table: "templates",
                column: "hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_template_values_values_id",
                table: "TemplateValues",
                column: "values_id");

            migrationBuilder.CreateIndex(
                name: "ix_translations_language_id",
                table: "translations",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "ix_translations_template_value_id",
                table: "translations",
                column: "template_value_id");

            migrationBuilder.CreateIndex(
                name: "ix_values_hash",
                table: "values",
                column: "hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "TemplateValues");

            migrationBuilder.DropTable(
                name: "translations");

            migrationBuilder.DropTable(
                name: "templates");

            migrationBuilder.DropTable(
                name: "languages");

            migrationBuilder.DropTable(
                name: "values");
        }
    }
}
