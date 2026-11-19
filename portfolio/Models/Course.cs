
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class Course
{
    public long CourseId { get; set; }

    [Required(ErrorMessage = "Course code is required.")]

    public string CourseCode { get; set; } = "";

    [Required(ErrorMessage = "Course name is required.")]
    public string CourseName { get; set; } = "";
}
