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
                ProjectDate = reader.IsDBNull(6) ? null : reader.GetFieldValue<DateOnly>(6)
            };
        }

        public async Task<List<Project>> GetAllAsync()
        {
            const string sql = @"
                SELECT
                    project_id,
                    course_id,
                    title,
                    description,
                    video_url,
                    github_url,
                    project_date
                FROM portfolio.project
                ORDER BY project_id;
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
    }
}
