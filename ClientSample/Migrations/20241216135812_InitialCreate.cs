using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClientSample.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommandTypes",
                columns: table => new
                {
                    CommandTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandTypes", x => x.CommandTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ConditionTypes",
                columns: table => new
                {
                    ConditionTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConditionTypes", x => x.ConditionTypeID);
                });

            migrationBuilder.CreateTable(
                name: "KeyMasters",
                columns: table => new
                {
                    KeyID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyMasters", x => x.KeyID);
                });

            migrationBuilder.CreateTable(
                name: "TemplateTypes",
                columns: table => new
                {
                    TemplateTypeID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateTypes", x => x.TemplateTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    TemplateID = table.Column<string>(type: "TEXT", nullable: false),
                    TemplateTypeID = table.Column<int>(type: "INTEGER", nullable: false),
                    ConditionTypeID = table.Column<int>(type: "INTEGER", nullable: true),
                    SuccessNextTemplateID = table.Column<string>(type: "TEXT", nullable: false),
                    FailureNextTemplateID = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.TemplateID);
                    table.ForeignKey(
                        name: "FK_Templates_ConditionTypes_ConditionTypeID",
                        column: x => x.ConditionTypeID,
                        principalTable: "ConditionTypes",
                        principalColumn: "ConditionTypeID");
                    table.ForeignKey(
                        name: "FK_Templates_TemplateTypes_TemplateTypeID",
                        column: x => x.TemplateTypeID,
                        principalTable: "TemplateTypes",
                        principalColumn: "TemplateTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commands",
                columns: table => new
                {
                    CommandID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateID = table.Column<string>(type: "TEXT", nullable: false),
                    CommandTypeID = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetDeviceID = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commands", x => x.CommandID);
                    table.ForeignKey(
                        name: "FK_Commands_CommandTypes_CommandTypeID",
                        column: x => x.CommandTypeID,
                        principalTable: "CommandTypes",
                        principalColumn: "CommandTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Commands_Templates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "Templates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    ConditionID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateID = table.Column<string>(type: "TEXT", nullable: false),
                    KeyID = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.ConditionID);
                    table.ForeignKey(
                        name: "FK_Conditions_KeyMasters_KeyID",
                        column: x => x.KeyID,
                        principalTable: "KeyMasters",
                        principalColumn: "KeyID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conditions_Templates_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "Templates",
                        principalColumn: "TemplateID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CommandTypes",
                columns: new[] { "CommandTypeID", "Name" },
                values: new object[,]
                {
                    { 1, "Send" },
                    { 2, "Receive" }
                });

            migrationBuilder.InsertData(
                table: "ConditionTypes",
                columns: new[] { "ConditionTypeID", "Name" },
                values: new object[,]
                {
                    { 1, "threshold" },
                    { 2, "case" }
                });

            migrationBuilder.InsertData(
                table: "KeyMasters",
                columns: new[] { "KeyID", "Name" },
                values: new object[,]
                {
                    { 1, "threshold" },
                    { 2, "SUCCESS" },
                    { 3, "FAILURE" }
                });

            migrationBuilder.InsertData(
                table: "TemplateTypes",
                columns: new[] { "TemplateTypeID", "Name" },
                values: new object[,]
                {
                    { 1, "Threshold" },
                    { 2, "Case" },
                    { 3, "End" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commands_CommandTypeID",
                table: "Commands",
                column: "CommandTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Commands_TemplateID",
                table: "Commands",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Conditions_KeyID",
                table: "Conditions",
                column: "KeyID");

            migrationBuilder.CreateIndex(
                name: "IX_Conditions_TemplateID",
                table: "Conditions",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_ConditionTypeID",
                table: "Templates",
                column: "ConditionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Templates_TemplateTypeID",
                table: "Templates",
                column: "TemplateTypeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Commands");

            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropTable(
                name: "CommandTypes");

            migrationBuilder.DropTable(
                name: "KeyMasters");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "ConditionTypes");

            migrationBuilder.DropTable(
                name: "TemplateTypes");
        }
    }
}
