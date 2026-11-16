using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hi_Trade.DAL.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Role] = 1 OR [Email] = N'admin@hi-trade.com')
BEGIN
    INSERT INTO [Users] ([Address], [Balance], [CreatedAt], [Email], [Enabled], [FullName], [IsDeleted], [Password], [ProfilePictureUrl], [Role], [UpdatedAt])
    VALUES (N'Hi-Trade HQ', 0, SYSUTCDATETIME(), N'admin@hi-trade.com', 1, N'System Admin', 0, N'$2b$10$oV7uJ4NU/..Rif4HXzqmdOZkAC/BYU24Q07BkxmSOCoLKBsZi9oa6', N'', 1, SYSUTCDATETIME());
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DELETE FROM [Users] WHERE [Email] = N'admin@hi-trade.com' AND [Role] = 1;
");
        }
    }
}
