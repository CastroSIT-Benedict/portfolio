using portfolio.Data;
using portfolio.Models;
using Microsoft.AspNetCore.Mvc;

namespace portfolio.Controllers;

public class ProjectsController : Controller
{
    private readonly ProjectRepository _projects;
    // ASP.NET Core hands in the repository, because Program.cs registered it.
    public ProjectsController(ProjectRepository projects)
    {
        _projects = projects;
    }

    
    public async Task<IActionResult> Index()
    {
        var projects = await _projects.GetAllAsync();
        return View(projects); // Views/Projects/Index.cshtml
    }

}