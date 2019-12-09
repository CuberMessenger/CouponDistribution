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
                    Username = table.Column<string>(maxLength: 20, nullable: false),
                    Password = table.Column<string>(maxLength: 32, nullable: true),
                    Kind = table.Column<string>(maxLength: 8, nullable: true),
                    Authorization = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "CouponsOfCustomer",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(maxLength: 60, nullable: true),
                    Description = table.Column<string>(maxLength: 60, nullable: true),
                    Stock = table.Column<int>(nullable: false),
                    Username = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponsOfCustomer", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CouponsOfCustomer_Users_Username",
                        column: x => x.Username,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CouponsOfSaler",
                columns: table => new
                {
                    Name = table.Column<string>(maxLength: 60, nullable: false),
                    Description = table.Column<string>(maxLength: 60, nullable: true),
                    Amount = table.Column<int>(nullable: false),
                    Left = table.Column<int>(nullable: false),
                    Stock = table.Column<int>(nullable: false),
                    Username = table.Column<string>(maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponsOfSaler", x => x.Name);
                    table.ForeignKey(
                        name: "FK_CouponsOfSaler_Users_Username",
                        column: x => x.Username,
                        principalTable: "Users",
                        principalColumn: "Username",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CouponsOfCustomer_Username",
                table: "CouponsOfCustomer",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_CouponsOfSaler_Username",
                table: "CouponsOfSaler",
                column: "Username");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CouponsOfCustomer");

            migrationBuilder.DropTable(
                name: "CouponsOfSaler");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
