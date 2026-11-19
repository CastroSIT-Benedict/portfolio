
using portfolio.Data;
using portfolio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace portfolio.Controllers;

public class CoursesController : Controller
{
    private readonly CourseRepository _courses;

    public CoursesController(CourseRepository courses)
    {
        _courses = courses;
    }

    // Display all courses
    public async Task<IActionResult> Index()
    {
        var courses = await _courses.GetAllAsync();

        return View("~/Views/Projects/ManageCourses.cshtml", courses);
    }

    // Display the Create form
    [Authorize(Roles = "Editor")]
    [HttpGet]
    public IActionResult Create()
    {
        return View("~/Views/Projects/Courses/Create.cshtml");
    }

    // Save the new course
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course course)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Projects/Courses/Create.cshtml", course);
        }

        try
        {
            await _courses.AddAsync(course);
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex) when (
            ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError(
                "CourseCode",
                "This course code already exists.");

            return View("~/Views/Projects/Courses/Create.cshtml", course);
        }
    }

    // Display the Edit form
    [Authorize(Roles = "Editor")]
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var course = await _courses.GetByIdAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        return View("~/Views/Projects/Courses/Edit.cshtml", course);
    }

    // Save course changes
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, Course course)
    {
        if (id != course.CourseId)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View("~/Views/Projects/Courses/Edit.cshtml", course);
        }

        try
        {
            await _courses.UpdateAsync(course);
            return RedirectToAction(nameof(Index));
        }
        catch (PostgresException ex) when (
            ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            ModelState.AddModelError(
                "CourseCode",
                "This course code already exists.");

            return View("~/Views/Projects/Courses/Edit.cshtml", course);
        }
    }

    // Delete a course
    [Authorize(Roles = "Editor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _courses.DeleteAsync(id);
        }
        catch (PostgresException ex) when (
            ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            TempData["Error"] =
                "This course cannot be deleted because it has existing projects.";
        }

        return RedirectToAction(nameof(Index));
    }
}
