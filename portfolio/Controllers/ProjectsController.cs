
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

    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
    ProjectRepository projects,
    CourseRepository courses,
    SupabaseStorageService storage,
    ILogger<ProjectsController> logger)
    {
        _projects = projects;
        _courses = courses;
        _storage = storage;
        _logger = logger;
    }

    // Display all projects.

    public async Task<IActionResult> Index()
    {
        var projects = await _projects.GetAllAsync();

        var projectIds = projects
            .Select(project => project.ProjectId)
            .ToList();

        var images = await _projects.GetImagesByProjectsAsync(projectIds);

        foreach (var project in projects)
        {
            project.Images = images.TryGetValue(project.ProjectId, out var projectImages)
                ? projectImages
                : new List<ProjectImage>();
        }

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

        try
        {
            await _projects.AddAsync(project);

            TempData["Success"] = "Project created successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create project {ProjectTitle}.",
                project.Title);

            ModelState.AddModelError(
                "",
                "We couldn't create the project. Please check your information and try again.");

            ViewBag.Courses = await _courses.GetAllAsync();

            return View(project);
        }
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

        try
        {
            var updated = await _projects.UpdateAsync(project);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = "Project updated successfully.";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to update project {ProjectId}.",
                project.ProjectId);

            ModelState.AddModelError(
                "",
                "We couldn't update the project. Please check your information and try again.");

            ViewBag.Courses = await _courses.GetAllAsync();

            return View(project);
        }
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
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to upload an image for project {ProjectId}.",
                model.ProjectId);

            ModelState.AddModelError(
                "",
                "We couldn't upload your image. Please check the file and try again.");

            return View(model);
        }

        // Save image information in PostgreSQL.
        try
        {
            await _projects.AddImageAsync(
                model.ProjectId,
                uploaded.PublicUrl,
                model.Caption,
                model.DisplayOrder);
        }
        catch (Exception ex)
        {
            // Record the database insertion failure.
            _logger.LogError(
                ex,
                "Failed to save image information for project {ProjectId}.",
                model.ProjectId);

            // Attempt to remove the uploaded file from Supabase.
            try
            {
                await _storage.DeleteAsync(uploaded.PublicUrl);
            }
            catch (Exception cleanupEx)
            {
                // Record the cleanup failure separately.
                _logger.LogError(
                    cleanupEx,
                    "Failed to clean up uploaded image for project {ProjectId}.",
                    model.ProjectId);
            }

            ModelState.AddModelError(
                "",
                "We couldn't save your image. Please try again.");

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    // Delete a project and its associated images.
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var images = await _projects.GetImagesByProjectAsync(id);

        try
        {
            foreach (var image in images)
            {
                // Delete the actual image from Supabase Storage.
                await _storage.DeleteAsync(image.ImageUrl);

                // Delete its database record.
                await _projects.DeleteImageAsync(image.ImageId);
            }

            // Delete the project after all images are handled.
            await _projects.DeleteAsync(id);

            TempData["Success"] = "Project deleted successfully.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete an image from storage while deleting project {ProjectId}.",
                id);

            TempData["Error"] =
                "The project could not be deleted because an image could not be removed from storage. Please try again.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Invalid storage URL encountered while deleting project {ProjectId}.",
                id);

            TempData["Error"] =
                "The project could not be deleted because an image has an invalid storage URL.";
        }

        return RedirectToAction(nameof(Index));
    }


    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(long imageId)
    {
        var image = await _projects.GetImageByIdAsync(imageId);

        if (image == null)
        {
            return NotFound();
        }

        try
        {
            // Delete the actual file from Supabase Storage.
            await _storage.DeleteAsync(image.ImageUrl);

            // Delete its database record.
            await _projects.DeleteImageAsync(imageId);

            TempData["Success"] = "Image deleted successfully.";
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete image {ImageId} from storage.",
                imageId);

            TempData["Error"] =
                "The image could not be deleted from storage. Please try again.";
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(
                ex,
                "Invalid storage URL encountered while deleting image {ImageId}.",
                imageId);

            TempData["Error"] =
                "The image could not be deleted because its storage URL is invalid.";
        }

        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to delete database record for image {ImageId}.",
                imageId);

            TempData["Error"] =
                "The image file may have been removed, but its database record could not be deleted. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }

}
