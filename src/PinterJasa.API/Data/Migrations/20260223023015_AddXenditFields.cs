using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PinterJasa.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddXenditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "bank_account_name",
                table: "providers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_account_number",
                table: "providers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "bank_code",
                table: "providers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "xendit_disbursement_id",
                table: "payouts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "xendit_invoice_id",
                table: "payments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "xendit_invoice_url",
                table: "payments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bank_account_name",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "bank_account_number",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "bank_code",
                table: "providers");

            migrationBuilder.DropColumn(
                name: "xendit_disbursement_id",
                table: "payouts");

            migrationBuilder.DropColumn(
                name: "xendit_invoice_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "xendit_invoice_url",
                table: "payments");
        }
    }
}
