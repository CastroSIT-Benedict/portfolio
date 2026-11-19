
using portfolio.Data;
using portfolio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace portfolio.Controllers;

public class ProjectsController : Controller
{
    private readonly ProjectRepository _projects;
    private readonly CourseRepository _courses;

    public ProjectsController(
        ProjectRepository projects,
        CourseRepository courses)
    {
        _projects = projects;
        _courses = courses;
    }

    // Display all projects.
    public async Task<IActionResult> Index()
    {
        var projects = await _projects.GetAllAsync();
        return View(projects);
    }

    // Display the Create form.
    [Authorize(Roles = "Editor")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Courses = await _courses.GetAllAsync();
        return View();
    }

    // Save a new project.
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await _courses.GetAllAsync();
            return View(project);
        }

        await _projects.AddAsync(project);

        return RedirectToAction(nameof(Index));
    }

    // Display the Edit form.
    [Authorize(Roles = "Editor")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var project = await _projects.GetByIdAsync(id);

        if (project == null)
        {
            return NotFound();
        }

        ViewBag.Courses = await _courses.GetAllAsync();

        return View(project);
    }

    // Save project changes.
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Project project)
    {
        if (id != project.ProjectId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await _courses.GetAllAsync();
            return View(project);
        }

        await _projects.UpdateAsync(project);

        return RedirectToAction(nameof(Index));
    }

    // Delete a project.
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        await _projects.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
}
