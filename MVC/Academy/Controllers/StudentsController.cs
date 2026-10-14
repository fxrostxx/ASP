using Academy.Data;
using Academy.Models;
using Academy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace Academy.Controllers
{
	public class StudentsController : Controller
	{
		private readonly AcademyContext _context;
		public StudentsController(AcademyContext context)
		{
			_context = context;
		}
		// STUDENTS: Index
		public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber, int? directionId, int? groupId, bool showAll = false)
		{
			if (searchString != null)
			{
				pageNumber = 1;
			}
			else
			{
				searchString = currentFilter;
			}
			if (searchString != null)
			{
				searchString = searchString.Trim();
			}
			ViewData["CurrentSort"] = sortOrder;
			ViewData["CurrentFilter"] = searchString;
			ViewData["NameSortParam"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
			ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
			ViewData["DirectionSortParam"] = sortOrder == "direction" ? "direction_desc" : "direction";
			ViewData["LessonsSortParam"] = sortOrder == "lessons" ? "lessons_desc" : "lessons";
			ViewData["ExperienceSortParam"] = sortOrder == "experience" ? "experience_desc" : "experience";
			IQueryable<Student> students = _context.Students.Include(s => s.Group).ThenInclude(g => g.Direction).AsNoTracking();
			bool hasFilter = showAll || directionId.HasValue || groupId.HasValue || !String.IsNullOrEmpty(searchString);
			if (!hasFilter) students = students.Where(s => false);
			if (directionId.HasValue) students = students.Where(s => s.Group.direction == directionId);
			if (groupId.HasValue) students = students.Where(s => s.group == groupId);
			if (!String.IsNullOrEmpty(searchString))
				students = students.Where(s => s.last_name.Contains(searchString) || s.first_name.Contains(searchString) || (s.middle_name != null && s.middle_name.Contains(searchString)));
			switch (sortOrder)
			{
				case "name_desc": students = students.OrderByDescending(s => s.last_name).ThenBy(s => s.first_name).ThenBy(s => s.stud_id); break;
				case "date": students = students.OrderByDescending(s => s.birth_date).ThenBy(s => s.stud_id); break;
				case "date_desc": students = students.OrderBy(s => s.birth_date).ThenBy(s => s.stud_id); break;
				case "direction": students = students.OrderBy(s => s.Group.Direction.direction_name).ThenBy(s => s.stud_id); break;
				case "direction_desc": students = students.OrderByDescending(s => s.Group.Direction.direction_name).ThenBy(s => s.stud_id); break;
				default: students = students.OrderBy(s => s.last_name).ThenBy(s => s.first_name).ThenBy(s => s.stud_id); break;
			}
			int pageSize = 5;
			PaginatedList<Student> items = await PaginatedList<Student>.CreateAsync(students, pageNumber ?? 1, pageSize);
			IQueryable<Group> groups = _context.Groups.AsNoTracking();
			if (directionId.HasValue) groups = groups.Where(g => g.direction == directionId);
			ViewBag.Groups = new SelectList(await groups.OrderBy(g => g.group_name).ToListAsync(), "group_id", "group_name", groupId);
			ViewBag.Directions = new SelectList(await _context.Directions.AsNoTracking().OrderBy(d => d.direction_name).ToListAsync(), "direction_id", "direction_name", directionId);
			ViewData["DirectionId"] = directionId;
			ViewData["GroupId"] = groupId;
			ViewData["ShowAll"] = showAll;
			ViewData["HasFilter"] = hasFilter;
			return View(items);
		}
		// STUDENTS: Details
		public async Task<IActionResult> Details(int id)
		{
			Student student = await _context.Students.Include(s => s.Group).ThenInclude(g => g.Direction)
				.AsNoTracking().FirstOrDefaultAsync(s => s.stud_id == id);
			if (student == null)
			{
				return NotFound();
			}
			return View(student);
		}
		// STUDENTS: Create
		public async Task<IActionResult> Create()
		{
			await GroupsList();
			return View(new Student());
		}
		[HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(5 * 1024 * 1024)]
		// STUDENTS: Create
		public async Task<IActionResult> Create([Bind("group,last_name,first_name,middle_name,birth_date,email,phone")] Student student, IFormFile photoFile)
		{
			DateOnly today = DateOnly.FromDateTime(DateTime.Today);
			if (student.birth_date == DateOnly.MinValue || student.birth_date > today)
			{
				ModelState.AddModelError("birth_date", "Укажите корректную дату рождения.");
			}
			await ValidateGroup(student.group);
			if (photoFile != null && photoFile.Length > 0)
			{
				if (photoFile.Length > 4 * 1024 * 1024)
				{
					ModelState.AddModelError("photoFile", "Фотография должна быть не больше 4 МБ.");
				}
				else
				{
					using (MemoryStream stream = new MemoryStream())
					{
						await photoFile.CopyToAsync(stream);
						student.photo = stream.ToArray();
					}
				}
			}
			if (ModelState.IsValid)
			{
				_context.Students.Add(student);
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index), new { groupId = student.group });
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!StudentExists(student.stud_id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				catch (DbUpdateException)
				{
					ModelState.AddModelError("", "Не удалось сохранить изменения. Проверьте данные и связанные записи.");
				}
			}
			await GroupsList(student.group);
			return View(student);
		}
		// STUDENTS: Edit
		public async Task<IActionResult> Edit(int id)
		{
			Student student = await _context.Students.FindAsync(id);
			if (student == null) return NotFound();
			await GroupsList(student.group);
			return View(student);
		}
		[HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(5 * 1024 * 1024)]
		// STUDENTS: Edit
		public async Task<IActionResult> Edit(int id, [Bind("stud_id,group,last_name,first_name,middle_name,birth_date,email,phone")] Student student, IFormFile photoFile, bool removePhoto = false)
		{
			if (id != student.stud_id) return NotFound();
			Student target = await _context.Students.FindAsync(id);
			if (target == null) return NotFound();
			DateOnly today = DateOnly.FromDateTime(DateTime.Today);
			if (student.birth_date == DateOnly.MinValue || student.birth_date > today)
			{
				ModelState.AddModelError("birth_date", "Укажите корректную дату рождения.");
			}
			await ValidateGroup(student.group);
			if (photoFile != null && photoFile.Length > 0)
			{
				if (photoFile.Length > 4 * 1024 * 1024)
				{
					ModelState.AddModelError("photoFile", "Фотография должна быть не больше 4 МБ.");
				}
				else
				{
					using (MemoryStream stream = new MemoryStream())
					{
						await photoFile.CopyToAsync(stream);
						target.photo = stream.ToArray();
					}
				}
			}
			else if (removePhoto)
			{
				target.photo = null;
			}
			if (ModelState.IsValid)
			{
				target.last_name = student.last_name;
				target.first_name = student.first_name;
				target.middle_name = student.middle_name;
				target.birth_date = student.birth_date;
				target.email = student.email;
				target.phone = student.phone;
				target.group = student.group;
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index), new { groupId = target.group });
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!StudentExists(student.stud_id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				catch (DbUpdateException)
				{
					ModelState.AddModelError("", "Не удалось сохранить изменения. Проверьте данные и связанные записи.");
				}
			}
			student.photo = target.photo;
			await GroupsList(student.group);
			return View(student);
		}
		// STUDENTS: Delete
		public async Task<IActionResult> Delete(int id)
		{
			Student student = await _context.Students.Include(s => s.Group).ThenInclude(g => g.Direction)
				.AsNoTracking().FirstOrDefaultAsync(s => s.stud_id == id);
			if (student == null)
			{
				return NotFound();
			}
			return View(student);
		}
		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			Student student = await _context.Students.Include(s => s.Group).ThenInclude(g => g.Direction).FirstOrDefaultAsync(s => s.stud_id == id);
			if (student == null) return NotFound();
			_context.Students.Remove(student);
			try
			{
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index), new { groupId = student.group });
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!StudentExists(student.stud_id))
				{
					return NotFound();
				}
				else
				{
					throw;
				}
			}
			catch (DbUpdateException)
			{
				ModelState.AddModelError("", "Не удалось сохранить изменения. Проверьте данные и связанные записи.");
			}
			return View("Delete", student);
		}
		private async Task GroupsList(int? selected = null)
		{
			IQueryable<Group> records = from record in _context.Groups
										orderby record.group_name
										select record;
			ViewBag.Groups = new SelectList(await records.AsNoTracking().ToListAsync(), "group_id", "group_name", selected);
		}
		private async Task ValidateGroup(int id)
		{
			if (!await _context.Groups.AnyAsync(g => g.group_id == id))
				ModelState.AddModelError("group", "Выберите существующую группу.");
		}
		private bool StudentExists(int? id)
		{
			return _context.Students.Any(e => e.stud_id == id);
		}
	}
}
