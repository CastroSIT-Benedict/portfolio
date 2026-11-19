
using portfolio.Models;
using Npgsql;

namespace portfolio.Data;

public class CourseRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public CourseRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Get all courses
    public async Task<List<Course>> GetAllAsync()
    {
        const string sql = @"
            SELECT course_id, course_code, course_name
            FROM portfolio.course
            ORDER BY course_code;
        ";

        var courses = new List<Course>();

        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            courses.Add(new Course
            {
                CourseId = reader.GetInt64(0),
                CourseCode = reader.GetString(1),
                CourseName = reader.GetString(2)
            });
        }

        return courses;
    }

    // Get one course
    public async Task<Course?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT course_id, course_code, course_name
            FROM portfolio.course
            WHERE course_id = @id;
        ";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Course
            {
                CourseId = reader.GetInt64(0),
                CourseCode = reader.GetString(1),
                CourseName = reader.GetString(2)
            };
        }

        return null;
    }

    // Add a course
    public async Task AddAsync(Course course)
    {
        const string sql = @"
            INSERT INTO portfolio.course (course_code, course_name)
            VALUES (@course_code, @course_name);
        ";

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue("course_code", course.CourseCode);
        command.Parameters.AddWithValue("course_name", course.CourseName);

        await command.ExecuteNonQueryAsync();
    }

    // Update a course
    public async Task UpdateAsync(Course course)
    {
        const string sql = @"
            UPDATE portfolio.course
            SET course_code = @course_code,
                course_name = @course_name
            WHERE course_id = @course_id;
        ";

        await using var command = _dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue("course_id", course.CourseId);
        command.Parameters.AddWithValue("course_code", course.CourseCode);
        command.Parameters.AddWithValue("course_name", course.CourseName);

        await command.ExecuteNonQueryAsync();
    }

    // Delete a course
    public async Task DeleteAsync(long id)
    {
        const string sql = @"
            DELETE FROM portfolio.course
            WHERE course_id = @id;
        ";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}
