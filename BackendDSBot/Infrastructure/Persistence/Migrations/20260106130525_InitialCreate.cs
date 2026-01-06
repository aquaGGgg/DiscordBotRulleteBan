using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BackendDSBot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bot_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    type = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    guild_id = table.Column<string>(type: "text", nullable: false),
                    discord_user_id = table.Column<string>(type: "text", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false),
                    attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    locked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    locked_by = table.Column<string>(type: "text", nullable: true),
                    run_after = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    last_error = table.Column<string>(type: "text", nullable: true),
                    dedup_key = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bot_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "config",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    ban_roulette_interval_seconds = table.Column<int>(type: "integer", nullable: false),
                    ban_roulette_pick_count = table.Column<int>(type: "integer", nullable: false),
                    ban_roulette_duration_min_seconds = table.Column<int>(type: "integer", nullable: false),
                    ban_roulette_duration_max_seconds = table.Column<int>(type: "integer", nullable: false),
                    ticket_roulette_interval_seconds = table.Column<int>(type: "integer", nullable: false),
                    ticket_roulette_pick_count = table.Column<int>(type: "integer", nullable: false),
                    ticket_roulette_tickets_min = table.Column<int>(type: "integer", nullable: false),
                    ticket_roulette_tickets_max = table.Column<int>(type: "integer", nullable: false),
                    eligible_role_id = table.Column<string>(type: "text", nullable: true),
                    jail_voice_channel_id = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_config", x => x.id);
                    table.CheckConstraint("ck_config_single_row", "\"id\" = 1");
                });

            migrationBuilder.CreateTable(
                name: "eligible_users_snapshot",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    guild_id = table.Column<string>(type: "text", nullable: false),
                    discord_user_id = table.Column<string>(type: "text", nullable: false),
                    is_eligible = table.Column<bool>(type: "boolean", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eligible_users_snapshot", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roulette_rounds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    type = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roulette_rounds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ticket_transfers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    from_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    to_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ticket_transfers", x => x.id);
                    table.CheckConstraint("ck_ticket_transfers_amount_positive", "\"amount\" > 0");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    discord_user_id = table.Column<string>(type: "text", nullable: false),
                    tickets_balance = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("ck_users_tickets_balance_nonnegative", "\"tickets_balance\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "punishments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_id = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    price_tickets = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ended_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_punishments", x => x.id);
                    table.ForeignKey(
                        name: "FK_punishments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "punishment_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    punishment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    delta_seconds = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_punishment_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_punishment_history_punishments_punishment_id",
                        column: x => x.punishment_id,
                        principalTable: "punishments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bot_jobs_locked_at",
                table: "bot_jobs",
                column: "locked_at");

            migrationBuilder.CreateIndex(
                name: "ix_bot_jobs_status_runafter_createdat",
                table: "bot_jobs",
                columns: new[] { "status", "run_after", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_bot_jobs_type_dedup_key",
                table: "bot_jobs",
                columns: new[] { "type", "dedup_key" },
                unique: true,
                filter: "\"dedup_key\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_eligible_users_snapshot_guild_eligible",
                table: "eligible_users_snapshot",
                columns: new[] { "guild_id", "is_eligible" });

            migrationBuilder.CreateIndex(
                name: "ux_eligible_users_snapshot_guild_discord",
                table: "eligible_users_snapshot",
                columns: new[] { "guild_id", "discord_user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_punishment_history_punishment_id",
                table: "punishment_history",
                column: "punishment_id");

            migrationBuilder.CreateIndex(
                name: "ix_punishments_guild_status_endsat",
                table: "punishments",
                columns: new[] { "guild_id", "status", "ends_at" });

            migrationBuilder.CreateIndex(
                name: "ux_punishments_active_user_guild",
                table: "punishments",
                columns: new[] { "user_id", "guild_id" },
                unique: true,
                filter: "\"status\" = 'Active'");

            migrationBuilder.CreateIndex(
                name: "ix_roulette_rounds_type_started_at",
                table: "roulette_rounds",
                columns: new[] { "type", "started_at" });

            migrationBuilder.CreateIndex(
                name: "ix_ticket_transfers_from_user_id",
                table: "ticket_transfers",
                column: "from_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_ticket_transfers_to_user_id",
                table: "ticket_transfers",
                column: "to_user_id");

            migrationBuilder.CreateIndex(
                name: "ux_users_discord_user_id",
                table: "users",
                column: "discord_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bot_jobs");

            migrationBuilder.DropTable(
                name: "config");

            migrationBuilder.DropTable(
                name: "eligible_users_snapshot");

            migrationBuilder.DropTable(
                name: "punishment_history");

            migrationBuilder.DropTable(
                name: "roulette_rounds");

            migrationBuilder.DropTable(
                name: "ticket_transfers");

            migrationBuilder.DropTable(
                name: "punishments");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
