using Academy.Data;
using Academy.Models;
using Academy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace Academy.Controllers
{
	public class TeachersController : Controller
	{
		private readonly AcademyContext _context;
		public TeachersController(AcademyContext context)
		{
			_context = context;
		}
		// TEACHERS: Index
		public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber, int? id)
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
			IQueryable<Teacher> teachers = _context.Teachers.AsNoTracking();
			if (!String.IsNullOrEmpty(searchString))
				teachers = teachers.Where(t => t.last_name.Contains(searchString) || t.first_name.Contains(searchString) || (t.middle_name != null && t.middle_name.Contains(searchString)));
			switch (sortOrder)
			{
				case "name_desc": teachers = teachers.OrderByDescending(t => t.last_name).ThenBy(t => t.first_name).ThenBy(t => t.teacher_id); break;
				case "date": teachers = teachers.OrderByDescending(t => t.birth_date).ThenBy(t => t.teacher_id); break;
				case "date_desc": teachers = teachers.OrderBy(t => t.birth_date).ThenBy(t => t.teacher_id); break;
				case "experience": teachers = teachers.OrderByDescending(t => t.work_since).ThenBy(t => t.teacher_id); break;
				case "experience_desc": teachers = teachers.OrderBy(t => t.work_since).ThenBy(t => t.teacher_id); break;
				default: teachers = teachers.OrderBy(t => t.last_name).ThenBy(t => t.first_name).ThenBy(t => t.teacher_id); break;
			}
			int pageSize = 5;
			PaginatedList<Teacher> items = await PaginatedList<Teacher>.CreateAsync(teachers, pageNumber ?? 1, pageSize);
			Teacher selected = null;
			if (id.HasValue)
				selected = await _context.Teachers.Include(t => t.DisciplinesRelations).ThenInclude(r => r.Discipline)
					.AsNoTracking().FirstOrDefaultAsync(t => t.teacher_id == id);
			ViewData["SelectedId"] = id;
			TeacherIndexData viewModel = new TeacherIndexData();
			viewModel.Teachers = items;
			viewModel.SelectedTeacher = selected;
			return View(viewModel);
		}
		// TEACHERS: Details
		public async Task<IActionResult> Details(int id)
		{
			Teacher teacher = await _context.Teachers.Include(t => t.DisciplinesRelations).ThenInclude(r => r.Discipline)
				.AsNoTracking().FirstOrDefaultAsync(t => t.teacher_id == id);
			if (teacher == null)
			{
				return NotFound();
			}
			return View(teacher);
		}
		// TEACHERS: Create
		public async Task<IActionResult> Create()
		{
			await DisciplinesList(Array.Empty<int>());
			return View(new Teacher());
		}
		[HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(5 * 1024 * 1024)]
		// TEACHERS: Create
		public async Task<IActionResult> Create([Bind("work_since,rate,last_name,first_name,middle_name,birth_date,email,phone")] Teacher teacher, int[] disciplineIds, IFormFile photoFile)
		{
			DateOnly today = DateOnly.FromDateTime(DateTime.Today);
			if (teacher.birth_date == DateOnly.MinValue || teacher.birth_date > today)
			{
				ModelState.AddModelError("birth_date", "Укажите корректную дату рождения.");
			}
			if (teacher.work_since == DateOnly.MinValue || teacher.work_since > today || teacher.work_since < teacher.birth_date)
			{
				ModelState.AddModelError("work_since", "Проверьте дату начала преподавания.");
			}
			disciplineIds = disciplineIds.Distinct().ToArray();
			await ValidateDisciplines(disciplineIds);
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
						teacher.photo = stream.ToArray();
					}
				}
			}
			if (ModelState.IsValid)
			{
				foreach (int disciplineId in disciplineIds)
					teacher.DisciplinesRelations.Add(new TeachersDisciplinesRelation { Teacher = teacher, discipline = disciplineId });
				_context.Teachers.Add(teacher);
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!TeacherExists(teacher.teacher_id))
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
			await DisciplinesList(disciplineIds);
			return View(teacher);
		}
		// TEACHERS: Edit
		public async Task<IActionResult> Edit(int id)
		{
			Teacher teacher = await _context.Teachers.Include(t => t.DisciplinesRelations).FirstOrDefaultAsync(t => t.teacher_id == id);
			if (teacher == null) return NotFound();
			await DisciplinesList(teacher.DisciplinesRelations.Select(r => r.discipline));
			return View(teacher);
		}
		[HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(5 * 1024 * 1024)]
		// TEACHERS: Edit
		public async Task<IActionResult> Edit(int id, [Bind("teacher_id,work_since,rate,last_name,first_name,middle_name,birth_date,email,phone")] Teacher teacher, int[] disciplineIds, IFormFile photoFile, bool removePhoto = false)
		{
			if (id != teacher.teacher_id) return NotFound();
			Teacher target = await _context.Teachers.Include(t => t.DisciplinesRelations).FirstOrDefaultAsync(t => t.teacher_id == id);
			if (target == null) return NotFound();
			disciplineIds = disciplineIds.Distinct().ToArray();
			DateOnly today = DateOnly.FromDateTime(DateTime.Today);
			if (teacher.birth_date == DateOnly.MinValue || teacher.birth_date > today)
			{
				ModelState.AddModelError("birth_date", "Укажите корректную дату рождения.");
			}
			if (teacher.work_since == DateOnly.MinValue || teacher.work_since > today || teacher.work_since < teacher.birth_date)
			{
				ModelState.AddModelError("work_since", "Проверьте дату начала преподавания.");
			}
			await ValidateDisciplines(disciplineIds);
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
				target.last_name = teacher.last_name;
				target.first_name = teacher.first_name;
				target.middle_name = teacher.middle_name;
				target.birth_date = teacher.birth_date;
				target.email = teacher.email;
				target.phone = teacher.phone;
				target.work_since = teacher.work_since;
				target.rate = teacher.rate;
				foreach (TeachersDisciplinesRelation relation in target.DisciplinesRelations.ToList())
				{
					if (!disciplineIds.Contains(relation.discipline))
					{
						_context.TeachersDisciplinesRelation.Remove(relation);
					}
				}
				foreach (int relationId in disciplineIds)
				{
					if (!target.DisciplinesRelations.Any(r => r.discipline == relationId))
					{
						target.DisciplinesRelations.Add(new TeachersDisciplinesRelation { teacher = id, discipline = relationId });
					}
				}
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!TeacherExists(teacher.teacher_id))
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
			teacher.photo = target.photo;
			await DisciplinesList(disciplineIds);
			return View(teacher);
		}
		// TEACHERS: Delete
		public async Task<IActionResult> Delete(int id)
		{
			Teacher teacher = await _context.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.teacher_id == id);
			if (teacher == null)
			{
				return NotFound();
			}
			return View(teacher);
		}
		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			Teacher teacher = await _context.Teachers.Include(t => t.DisciplinesRelations).FirstOrDefaultAsync(t => t.teacher_id == id);
			if (teacher == null) return NotFound();
			_context.TeachersDisciplinesRelation.RemoveRange(teacher.DisciplinesRelations);
			_context.Teachers.Remove(teacher);
			try
			{
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!TeacherExists(teacher.teacher_id))
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
			return View("Delete", teacher);
		}
		private async Task DisciplinesList(IEnumerable<int> selected)
		{
			List<int> ids = selected.ToList();
			List<Discipline> disciplines = await _context.Disciplines.AsNoTracking().OrderBy(d => d.discipline_name).ToListAsync();
			List<SelectListItem> options = new List<SelectListItem>();
			foreach (Discipline discipline in disciplines)
			{
				options.Add(new SelectListItem(discipline.discipline_name, discipline.discipline_id.ToString(), ids.Contains(discipline.discipline_id)));
			}
			ViewBag.Relations = options;
			ViewData["RelationField"] = "disciplineIds";
		}
		private async Task ValidateDisciplines(int[] ids)
		{
			if (await _context.Disciplines.CountAsync(d => ids.Contains(d.discipline_id)) != ids.Length)
				ModelState.AddModelError("", "Выбрана несуществующая дисциплина.");
		}
		private bool TeacherExists(int? id)
		{
			return _context.Teachers.Any(e => e.teacher_id == id);
		}
	}
}
