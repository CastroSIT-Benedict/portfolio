
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class ProjectImageUpload
{
    [Required]
    public long ProjectId { get; set; }

    [Required(ErrorMessage = "Please select an image.")]
    public IFormFile? ImageFile { get; set; }

    [StringLength(200)]
    public string? Caption { get; set; }

    [Range(1, int.MaxValue)]
    public int DisplayOrder { get; set; } = 1;
}
