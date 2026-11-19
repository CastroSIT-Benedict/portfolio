
using portfolio.Models;
using Npgsql;
using NpgsqlTypes;

namespace portfolio.Data
{
    public class ProjectRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public ProjectRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        // Copies the current row into a Project object.
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

        // Retrieve all projects.
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

        // Retrieve one project using its ID.
        public async Task<Project?> GetByIdAsync(long id)
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
                WHERE p.project_id = @id;
            ";

            await using var command = _dataSource.CreateCommand(sql);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return ReadProject(reader);
            }

            return null;
        }

        // Add a new project.
        public async Task<long> AddAsync(Project project)
        {
            const string sql = @"
                INSERT INTO portfolio.project
                    (course_id, title, description, video_url, github_url, project_date)
                VALUES
                    (@course_id, @title, @description, @video_url, @github_url, @project_date)
                RETURNING project_id;
            ";

            await using var command = _dataSource.CreateCommand(sql);

            command.Parameters.AddWithValue("course_id", project.CourseId);
            command.Parameters.AddWithValue("title", project.Title);

            command.Parameters.Add(new NpgsqlParameter("description", NpgsqlDbType.Text)
            {
                Value = (object?)project.Description ?? DBNull.Value
            });

            command.Parameters.Add(new NpgsqlParameter("video_url", NpgsqlDbType.Text)
            {
                Value = (object?)project.VideoUrl ?? DBNull.Value
            });

            command.Parameters.Add(new NpgsqlParameter("github_url", NpgsqlDbType.Text)
            {
                Value = (object?)project.GithubUrl ?? DBNull.Value
            });

            command.Parameters.Add(new NpgsqlParameter("project_date", NpgsqlDbType.Date)
            {
                Value = (object?)project.ProjectDate ?? DBNull.Value
            });

            return Convert.ToInt64(await command.ExecuteScalarAsync());
        }

        // Update an existing project.
        public async Task UpdateAsync(Project project)
        {
            const string sql = @"
                UPDATE portfolio.project
                SET
                    course_id = @course_id,
                    title = @title,
                    description = @description,
                    video_url = @video_url,
                    github_url = @github_url,
                    project_date = @project_date
                WHERE project_id = @project_id;
            ";

            await using var command = _dataSource.CreateCommand(sql);

            command.Parameters.AddWithValue("project_id", project.ProjectId);
            command.Parameters.AddWithValue("course_id", project.CourseId);
            command.Parameters.AddWithValue("title", project.Title);

            command.Parameters.Add(new NpgsqlParameter("description", NpgsqlDbType.Text)
            {
                Value = (object?)project.Description ?? DBNull.Value
            });

            command.Parameters.Add(new NpgsqlParameter("video_url", NpgsqlDbType.Text)
            {
                Value = (object?)project.VideoUrl ?? DBNull.Value
            });

            command.Parameters.Add(new NpgsqlParameter("github_url", NpgsqlDbType.Text)
            {
                Value = (object?)project.GithubUrl ?? DBNull.Value
            });

            command.Parameters.Add(new NpgsqlParameter("project_date", NpgsqlDbType.Date)
            {
                Value = (object?)project.ProjectDate ?? DBNull.Value
            });

            await command.ExecuteNonQueryAsync();
        }

        // Delete a project.
        public async Task DeleteAsync(long id)
        {
            const string sql = @"
                DELETE FROM portfolio.project
                WHERE project_id = @id;
            ";

            await using var command = _dataSource.CreateCommand(sql);
            command.Parameters.AddWithValue("id", id);

            await command.ExecuteNonQueryAsync();
        }

        // Add an image record to a project.
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

            command.Parameters.Add(new NpgsqlParameter("caption", NpgsqlDbType.Varchar)
            {
                Value = (object?)caption ?? DBNull.Value
            });

            command.Parameters.AddWithValue("display_order", displayOrder);

            await command.ExecuteNonQueryAsync();
        }
    }
}
