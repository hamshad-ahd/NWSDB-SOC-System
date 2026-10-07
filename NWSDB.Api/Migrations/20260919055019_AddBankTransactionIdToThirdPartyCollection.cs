using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NWSDB.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBankTransactionIdToThirdPartyCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankTransactionId",
                table: "ThirdPartyCollections",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BankTransactionId",
                table: "ThirdPartyCollections");
        }
    }
}
