using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ListWish.Migrations
{
    public partial class addsoftdelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Softdelete",
                table: "ListItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Softdelete",
                table: "ListItems");
        }
    }
}
