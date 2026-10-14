using Academy.Data;
using Academy.Models;
using Academy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace Academy.Controllers
{
	public class DisciplinesController : Controller
	{
		private readonly AcademyContext _context;
		public DisciplinesController(AcademyContext context)
		{
			_context = context;
		}
		// DISCIPLINES: Index
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
			IQueryable<Discipline> disciplines = _context.Disciplines.AsNoTracking();
			if (!String.IsNullOrEmpty(searchString)) disciplines = disciplines.Where(d => d.discipline_name.Contains(searchString));
			switch (sortOrder)
			{
				case "name_desc": disciplines = disciplines.OrderByDescending(d => d.discipline_name).ThenBy(d => d.discipline_id); break;
				case "lessons": disciplines = disciplines.OrderBy(d => d.number_of_lessons).ThenBy(d => d.discipline_id); break;
				case "lessons_desc": disciplines = disciplines.OrderByDescending(d => d.number_of_lessons).ThenBy(d => d.discipline_id); break;
				default: disciplines = disciplines.OrderBy(d => d.discipline_name).ThenBy(d => d.discipline_id); break;
			}
			int pageSize = 5;
			PaginatedList<Discipline> items = await PaginatedList<Discipline>.CreateAsync(disciplines, pageNumber ?? 1, pageSize);
			Discipline selected = null;
			if (id.HasValue)
				selected = await _context.Disciplines.Include(d => d.TeachersRelations).ThenInclude(r => r.Teacher)
					.AsNoTracking().FirstOrDefaultAsync(d => d.discipline_id == id);
			ViewData["SelectedId"] = id;
			DisciplineIndexData viewModel = new DisciplineIndexData();
			viewModel.Disciplines = items;
			viewModel.SelectedDiscipline = selected;
			return View(viewModel);
		}
		// DISCIPLINES: Details
		public async Task<IActionResult> Details(int id)
		{
			Discipline discipline = await _context.Disciplines.Include(d => d.TeachersRelations).ThenInclude(r => r.Teacher)
				.AsNoTracking().FirstOrDefaultAsync(d => d.discipline_id == id);
			if (discipline == null)
			{
				return NotFound();
			}
			return View(discipline);
		}
		// DISCIPLINES: Create
		public async Task<IActionResult> Create()
		{
			await TeachersList(Array.Empty<int>());
			return View(new Discipline { number_of_lessons = 1 });
		}
		[HttpPost, ValidateAntiForgeryToken]
		// DISCIPLINES: Create
		public async Task<IActionResult> Create([Bind("discipline_name,number_of_lessons")] Discipline discipline, int[] teacherIds)
		{
			teacherIds = teacherIds.Distinct().ToArray();
			await ValidateTeachers(teacherIds);
			if (ModelState.IsValid)
			{
				foreach (int teacherId in teacherIds)
					discipline.TeachersRelations.Add(new TeachersDisciplinesRelation { Discipline = discipline, teacher = teacherId });
				_context.Disciplines.Add(discipline);
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DisciplineExists(discipline.discipline_id))
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
			await TeachersList(teacherIds);
			return View(discipline);
		}
		// DISCIPLINES: Edit
		public async Task<IActionResult> Edit(int id)
		{
			Discipline discipline = await _context.Disciplines.Include(d => d.TeachersRelations).FirstOrDefaultAsync(d => d.discipline_id == id);
			if (discipline == null) return NotFound();
			await TeachersList(discipline.TeachersRelations.Select(r => r.teacher));
			return View(discipline);
		}
		[HttpPost, ValidateAntiForgeryToken]
		// DISCIPLINES: Edit
		public async Task<IActionResult> Edit(int id, [Bind("discipline_id,discipline_name,number_of_lessons")] Discipline discipline, int[] teacherIds)
		{
			if (id != discipline.discipline_id) return NotFound();
			Discipline target = await _context.Disciplines.Include(d => d.TeachersRelations).FirstOrDefaultAsync(d => d.discipline_id == id);
			if (target == null) return NotFound();
			teacherIds = teacherIds.Distinct().ToArray();
			await ValidateTeachers(teacherIds);
			if (ModelState.IsValid)
			{
				target.discipline_name = discipline.discipline_name;
				target.number_of_lessons = discipline.number_of_lessons;
				foreach (TeachersDisciplinesRelation relation in target.TeachersRelations.ToList())
				{
					if (!teacherIds.Contains(relation.teacher))
					{
						_context.TeachersDisciplinesRelation.Remove(relation);
					}
				}
				foreach (int relationId in teacherIds)
				{
					if (!target.TeachersRelations.Any(r => r.teacher == relationId))
					{
						target.TeachersRelations.Add(new TeachersDisciplinesRelation { teacher = relationId, discipline = id });
					}
				}
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DisciplineExists(discipline.discipline_id))
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
			await TeachersList(teacherIds);
			return View(discipline);
		}
		// DISCIPLINES: Delete
		public async Task<IActionResult> Delete(int id)
		{
			Discipline discipline = await _context.Disciplines.AsNoTracking().FirstOrDefaultAsync(d => d.discipline_id == id);
			if (discipline == null)
			{
				return NotFound();
			}
			return View(discipline);
		}
		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			Discipline discipline = await _context.Disciplines.Include(d => d.TeachersRelations).FirstOrDefaultAsync(d => d.discipline_id == id);
			if (discipline == null) return NotFound();
			_context.TeachersDisciplinesRelation.RemoveRange(discipline.TeachersRelations);
			_context.Disciplines.Remove(discipline);
			try
			{
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!DisciplineExists(discipline.discipline_id))
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
			return View("Delete", discipline);
		}
		private async Task TeachersList(IEnumerable<int> selected)
		{
			List<int> ids = selected.ToList();
			List<Teacher> teachers = await _context.Teachers.AsNoTracking().OrderBy(t => t.last_name).ThenBy(t => t.first_name).ToListAsync();
			List<SelectListItem> options = new List<SelectListItem>();
			foreach (Teacher teacher in teachers)
			{
				options.Add(new SelectListItem(teacher.FullName, teacher.teacher_id.ToString(), ids.Contains(teacher.teacher_id)));
			}
			ViewBag.Relations = options;
			ViewData["RelationField"] = "teacherIds";
		}
		private async Task ValidateTeachers(int[] ids)
		{
			if (await _context.Teachers.CountAsync(t => ids.Contains(t.teacher_id)) != ids.Length)
				ModelState.AddModelError("", "Выбран несуществующий преподаватель.");
		}
		private bool DisciplineExists(int? id)
		{
			return _context.Disciplines.Any(e => e.discipline_id == id);
		}
	}
}
