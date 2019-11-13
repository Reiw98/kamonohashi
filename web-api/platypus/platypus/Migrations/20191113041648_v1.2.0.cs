using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Nssol.Platypus.Migrations
{
    public partial class v120 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ClusterId",
                table: "TrainingHistories",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClusterId",
                table: "NotebookHistories",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClusterId",
                table: "InferenceHistories",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Cluster",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedBy = table.Column<string>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false),
                    HostName = table.Column<string>(nullable: false),
                    DisplayName = table.Column<string>(nullable: false),
                    ResourceManageKey = table.Column<string>(nullable: false),
                    Memo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cluster", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClusterTenantMaps",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    CreatedBy = table.Column<string>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    ModifiedBy = table.Column<string>(nullable: false),
                    ModifiedAt = table.Column<DateTime>(nullable: false),
                    ClusterId = table.Column<long>(nullable: false),
                    TenantId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClusterTenantMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClusterTenantMaps_Cluster_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "Cluster",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClusterTenantMaps_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingHistories_ClusterId",
                table: "TrainingHistories",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_NotebookHistories_ClusterId",
                table: "NotebookHistories",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_InferenceHistories_ClusterId",
                table: "InferenceHistories",
                column: "ClusterId");

            migrationBuilder.CreateIndex(
                name: "IX_ClusterTenantMaps_TenantId",
                table: "ClusterTenantMaps",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ClusterTenantMaps_ClusterId_TenantId",
                table: "ClusterTenantMaps",
                columns: new[] { "ClusterId", "TenantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InferenceHistories_Cluster_ClusterId",
                table: "InferenceHistories",
                column: "ClusterId",
                principalTable: "Cluster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_NotebookHistories_Cluster_ClusterId",
                table: "NotebookHistories",
                column: "ClusterId",
                principalTable: "Cluster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingHistories_Cluster_ClusterId",
                table: "TrainingHistories",
                column: "ClusterId",
                principalTable: "Cluster",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InferenceHistories_Cluster_ClusterId",
                table: "InferenceHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_NotebookHistories_Cluster_ClusterId",
                table: "NotebookHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingHistories_Cluster_ClusterId",
                table: "TrainingHistories");

            migrationBuilder.DropTable(
                name: "ClusterTenantMaps");

            migrationBuilder.DropTable(
                name: "Cluster");

            migrationBuilder.DropIndex(
                name: "IX_TrainingHistories_ClusterId",
                table: "TrainingHistories");

            migrationBuilder.DropIndex(
                name: "IX_NotebookHistories_ClusterId",
                table: "NotebookHistories");

            migrationBuilder.DropIndex(
                name: "IX_InferenceHistories_ClusterId",
                table: "InferenceHistories");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "TrainingHistories");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "NotebookHistories");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "InferenceHistories");
        }
    }
}
