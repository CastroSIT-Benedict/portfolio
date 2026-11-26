
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models;

public class ProjectImageUpload
{
    [Required]
    public long ProjectId { get; set; }

    public List<IFormFile> ImageFiles { get; set; } = new();

    public List<string?> Captions { get; set; } = new();

    public List<int> DisplayOrders { get; set; } = new();
}
