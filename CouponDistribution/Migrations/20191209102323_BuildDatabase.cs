using Microsoft.EntityFrameworkCore.Migrations;

namespace CouponDistribution.Migrations
{
    public partial class BuildDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    username = table.Column<string>(maxLength: 20, nullable: false),
                    password = table.Column<string>(maxLength: 32, nullable: true),
                    kind = table.Column<string>(maxLength: 8, nullable: true),
                    auth = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.username);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    name = table.Column<string>(maxLength: 60, nullable: false),
                    amount = table.Column<int>(nullable: false),
                    left = table.Column<int>(nullable: false),
                    stock = table.Column<int>(nullable: false),
                    description = table.Column<string>(maxLength: 60, nullable: true),
                    username = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.name);
                    table.ForeignKey(
                        name: "FK_Coupons_Users_username",
                        column: x => x.username,
                        principalTable: "Users",
                        principalColumn: "username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Coupons2",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(maxLength: 60, nullable: true),
                    stock = table.Column<int>(nullable: false),
                    description = table.Column<string>(maxLength: 60, nullable: true),
                    username = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons2", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Coupons2_Users_username",
                        column: x => x.username,
                        principalTable: "Users",
                        principalColumn: "username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_username",
                table: "Coupons",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons2_username",
                table: "Coupons2",
                column: "username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "Coupons2");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
