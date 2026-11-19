using portfolio.Models;
using Npgsql;

namespace portfolio.Data
{
    public class ProjectRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        // ASP.NET Core hands in the data source that Program.cs registered.
        public ProjectRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        // Copies the current row of the reader into a Project.
        // The positions 0 to 6 match the column order in the SELECT.
        private static Project ReadProject(NpgsqlDataReader reader)
        {
            return new Project
            {
                ProjectId = reader.GetInt64(0),
                CourseId = reader.GetInt64(1),
                Title = reader.GetString(2),
                Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                VideoUrl = reader.IsDBNull(4) ? null : reader.GetString(4),
                GithubUrl = reader.IsDBNull(5) ? null : reader.GetString(5),
                ProjectDate = reader.IsDBNull(6) ? null : reader.GetFieldValue<DateOnly>(6),
                CourseName = reader.GetString(7),
                CourseCode = reader.GetString(8)
            };
        }

        public async Task<List<Project>> GetAllAsync()
        {
            const string sql = @"
        SELECT
            p.project_id,
            p.course_id,
            p.title,
            p.description,
            p.video_url,
            p.github_url,
            p.project_date,
            c.course_name,
            c.course_code
        FROM portfolio.project p
        INNER JOIN portfolio.course c
            ON p.course_id = c.course_id
        ORDER BY c.course_code, p.project_id;
    ";

            var projects = new List<Project>();

            await using var command = _dataSource.CreateCommand(sql);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                projects.Add(ReadProject(reader));
            }

            return projects;
        }


        public async Task AddImageAsync(
            long projectId,
            string imageUrl,
            string? caption,
            int displayOrder)
        {
            const string sql = @"
        INSERT INTO portfolio.project_image
            (project_id, image_url, caption, display_order)
        VALUES
            (@project_id, @image_url, @caption, @display_order);
    ";

            await using var command = _dataSource.CreateCommand(sql);

            command.Parameters.AddWithValue("project_id", projectId);
            command.Parameters.AddWithValue("image_url", imageUrl);
            command.Parameters.AddWithValue(
                "caption",
                (object?)caption ?? DBNull.Value);
            command.Parameters.AddWithValue("display_order", displayOrder);

            await command.ExecuteNonQueryAsync();
        }

    }
}
