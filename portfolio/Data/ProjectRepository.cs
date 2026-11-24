
using portfolio.Models;
using Npgsql;
using NpgsqlTypes;

namespace portfolio.Data
{
    public class ProjectRepository
    {
        // =========================================================
        // 1. DATABASE CONNECTION
        // Stores the database connection source used by this class.
        // =========================================================

        private readonly NpgsqlDataSource _dataSource;

        public ProjectRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }


        // =========================================================
        // 2. PROJECT DATA READER
        // Converts a database project row into a Project object.
        // =========================================================

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
                ProjectDate = reader.IsDBNull(6)
                    ? null
                    : reader.GetFieldValue<DateOnly>(6),
                CourseName = reader.GetString(7),
                CourseCode = reader.GetString(8)
            };
        }


        // =========================================================
        // 3. RETRIEVE ALL PROJECTS
        // Gets every project together with its course information.
        // =========================================================

        public async Task<List<Project>> GetAllAsync()
        {
            const string sql = """
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
                """;

            var projects = new List<Project>();

            await using var command = _dataSource.CreateCommand(sql);
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                projects.Add(ReadProject(reader));
            }

            return projects;
        }


        // =========================================================
        // 4. RETRIEVE ONE PROJECT
        // Finds a project using its project ID.
        // Returns null if the project does not exist.
        // =========================================================

        public async Task<Project?> GetByIdAsync(long id)
        {
            const string sql = """
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
                """;

            await using var command = _dataSource.CreateCommand(sql);

            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return ReadProject(reader);
            }

            return null;
        }


        // =========================================================
        // 5. ADD A PROJECT
        // Inserts a new project and returns its generated ID.
        // =========================================================

        public async Task<long> AddAsync(Project project)
        {
            const string sql = """
                INSERT INTO portfolio.project
                    (course_id, title, description, video_url, github_url, project_date)
                VALUES
                    (@course_id, @title, @description, @video_url, @github_url, @project_date)
                RETURNING project_id;
                """;

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


        // =========================================================
        // 6. UPDATE A PROJECT
        // Updates the details of an existing project.
        // =========================================================

        public async Task UpdateAsync(Project project)
        {
            const string sql = """
                UPDATE portfolio.project
                SET
                    course_id = @course_id,
                    title = @title,
                    description = @description,
                    video_url = @video_url,
                    github_url = @github_url,
                    project_date = @project_date
                WHERE project_id = @project_id;
                """;

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


        // =========================================================
        // 7. DELETE A PROJECT
        // Deletes a project using its project ID.
        // =========================================================

        public async Task DeleteAsync(long id)
        {
            const string sql = """
                DELETE FROM portfolio.project
                WHERE project_id = @id;
                """;

            await using var command = _dataSource.CreateCommand(sql);

            command.Parameters.AddWithValue("id", id);

            await command.ExecuteNonQueryAsync();
        }


        // =========================================================
        // 8. ADD A PROJECT IMAGE
        // Saves an image record connected to a project.
        // =========================================================

        public async Task AddImageAsync(
            long projectId,
            string imageUrl,
            string? caption,
            int displayOrder)
        {
            const string sql = """
                INSERT INTO portfolio.project_image
                    (project_id, image_url, caption, display_order)
                VALUES
                    (@project_id, @image_url, @caption, @display_order);
                """;

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


        // =========================================================
        // 9. RETRIEVE IMAGES FOR ONE PROJECT
        // Gets all image records belonging to a specific project.
        // =========================================================

        public async Task<List<ProjectImage>> GetImagesByProjectAsync(long projectId)
        {
            const string sql = """
                SELECT
                    image_id,
                    project_id,
                    image_url,
                    caption,
                    display_order
                FROM portfolio.project_image
                WHERE project_id = @project_id
                ORDER BY display_order, image_id;
                """;

            var images = new List<ProjectImage>();

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("project_id", projectId);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                images.Add(new ProjectImage
                {
                    ImageId = reader.GetInt64(0),
                    ProjectId = reader.GetInt64(1),
                    ImageUrl = reader.GetString(2),
                    Caption = reader.IsDBNull(3) ? null : reader.GetString(3),
                    DisplayOrder = reader.GetInt32(4)
                });
            }

            return images;
        }


        // =========================================================
        // 10. RETRIEVE IMAGES FOR MULTIPLE PROJECTS
        // Gets images for all supplied project IDs in one query.
        // Helps avoid making a separate database query per project.
        // =========================================================

        public async Task<Dictionary<long, List<ProjectImage>>> GetImagesByProjectsAsync(
            List<long> projectIds)
        {
            var result = new Dictionary<long, List<ProjectImage>>();

            if (projectIds.Count == 0)
            {
                return result;
            }

            const string sql = """
                SELECT
                    image_id,
                    project_id,
                    image_url,
                    caption,
                    display_order
                FROM portfolio.project_image
                WHERE project_id = ANY(@project_ids)
                ORDER BY project_id, display_order, image_id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "project_ids",
                NpgsqlDbType.Array | NpgsqlDbType.Bigint,
                projectIds);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var image = new ProjectImage
                {
                    ImageId = reader.GetInt64(0),
                    ProjectId = reader.GetInt64(1),
                    ImageUrl = reader.GetString(2),
                    Caption = reader.IsDBNull(3) ? null : reader.GetString(3),
                    DisplayOrder = reader.GetInt32(4)
                };

                if (!result.ContainsKey(image.ProjectId))
                {
                    result[image.ProjectId] = new List<ProjectImage>();
                }

                result[image.ProjectId].Add(image);
            }

            return result;
        }


        // =========================================================
        // 11. RETRIEVE ONE IMAGE
        // Finds an image using its image ID.
        // Returns null if the image does not exist.
        // =========================================================

        public async Task<ProjectImage?> GetImageByIdAsync(long imageId)
        {
            const string sql = """
                SELECT
                    image_id,
                    project_id,
                    image_url,
                    caption,
                    display_order
                FROM portfolio.project_image
                WHERE image_id = @image_id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("image_id", imageId);

            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new ProjectImage
            {
                ImageId = reader.GetInt64(0),
                ProjectId = reader.GetInt64(1),
                ImageUrl = reader.GetString(2),
                Caption = reader.IsDBNull(3) ? null : reader.GetString(3),
                DisplayOrder = reader.GetInt32(4)
            };
        }


        // =========================================================
        // 12. DELETE A PROJECT IMAGE
        // Deletes an image record using its image ID.
        // =========================================================

        public async Task DeleteImageAsync(long imageId)
        {
            const string sql = """
                DELETE FROM portfolio.project_image
                WHERE image_id = @image_id;
                """;

            await using var connection = await _dataSource.OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("image_id", imageId);

            await command.ExecuteNonQueryAsync();
        }
    }
}
