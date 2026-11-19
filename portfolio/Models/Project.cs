namespace portfolio.Models
{
    public class Project
    {
        public long ProjectId { get; set; }
        public long CourseId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public string? GithubUrl { get; set; }
        public DateOnly? ProjectDate { get; set; }

        public string CourseName { get; set; } = "";
        public List<ProjectImage> Images { get; set; } = new();

        public string CourseCode { get; set; } = "";
    }
}
