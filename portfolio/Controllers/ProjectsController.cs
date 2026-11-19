
using portfolio.Data;
using portfolio.Models;
using portfolio.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace portfolio.Controllers;

public class ProjectsController : Controller
{
    private readonly ProjectRepository _projects;
    private readonly CourseRepository _courses;
    private readonly SupabaseStorageService _storage;

    public ProjectsController(
        ProjectRepository projects,
        CourseRepository courses,
        SupabaseStorageService storage)
    {
        _projects = projects;
        _courses = courses;
        _storage = storage;
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

    // Display the image upload form.
    [Authorize(Roles = "Editor")]
    [HttpGet]
    public async Task<IActionResult> UploadImage(long projectId)
    {
        var project = await _projects.GetByIdAsync(projectId);

        if (project == null)
        {
            return NotFound();
        }

        ViewBag.ProjectTitle = project.Title;

        return View(new ProjectImageUpload
        {
            ProjectId = projectId
        });
    }

    // Upload an image to Supabase Storage.
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadImage(ProjectImageUpload model)
    {
        var project = await _projects.GetByIdAsync(model.ProjectId);

        if (project == null)
        {
            return NotFound();
        }

        ViewBag.ProjectTitle = project.Title;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var file = model.ImageFile!;

        // Allowed image types.
        var allowedTypes = new[]
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()) ||
            !allowedExtensions.Contains(
                Path.GetExtension(file.FileName).ToLowerInvariant()))
        {
            ModelState.AddModelError(
                nameof(model.ImageFile),
                "Only JPG, PNG, and WebP images are allowed.");

            return View(model);
        }

        // Check image size (5 MB maximum).
        if (file.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError(
                nameof(model.ImageFile),
                "The image must not exceed 5 MB.");

            return View(model);
        }

        if (file.Length == 0)
        {
            ModelState.AddModelError(
                nameof(model.ImageFile),
                "The selected image is empty.");

            return View(model);
        }

        // Upload image to Supabase.
        (string ObjectPath, string PublicUrl) uploaded;

        try
        {
            uploaded = await _storage.UploadAsync(
                model.ProjectId,
                file);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                "",
                "We couldn't upload your image. Please check the file and try again.");

            return View(model);
        }

        // Save image information in PostgreSQL.
        await _projects.AddImageAsync(
            model.ProjectId,
            uploaded.PublicUrl,
            model.Caption,
            model.DisplayOrder);

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
