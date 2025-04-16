using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoursesManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class uniqueindex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
            name: "IX_CourseQuestionConfigs_QuestionLevelId",
            table: "CourseQuestionConfigs");

            // Optional: Recreate a non-unique index (for performance, if needed)
            migrationBuilder.CreateIndex(
                name: "IX_CourseQuestionConfigs_QuestionLevelId",
                table: "CourseQuestionConfigs",
                column: "QuestionLevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
           name: "IX_CourseQuestionConfigs_QuestionLevelId",
           table: "CourseQuestionConfigs");

            migrationBuilder.CreateIndex(
                name: "IX_CourseQuestionConfigs_QuestionLevelId",
                table: "CourseQuestionConfigs",
                column: "QuestionLevelId",
                unique: true);
        }
    }
}
