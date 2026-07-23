using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShortUrls",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsCustomAlias = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ExpiresAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastVisitedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    VisitCount = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShortUrls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UrlVisits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShortUrlId = table.Column<long>(type: "bigint", nullable: false),
                    VisitedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Referer = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    IpHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlVisits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrlVisits_ShortUrls_ShortUrlId",
                        column: x => x.ShortUrlId,
                        principalTable: "ShortUrls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_Code",
                table: "ShortUrls",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ShortUrls_CreatedAtUtc",
                table: "ShortUrls",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UrlVisits_ShortUrlId_VisitedAtUtc",
                table: "UrlVisits",
                columns: new[] { "ShortUrlId", "VisitedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UrlVisits");

            migrationBuilder.DropTable(
                name: "ShortUrls");
        }
    }
}
