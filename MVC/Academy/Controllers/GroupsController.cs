using Academy.Data;
using Academy.Models;
using Academy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace Academy.Controllers
{
	public class GroupsController : Controller
	{
		private readonly AcademyContext _context;
		public GroupsController(AcademyContext context)
		{
			_context = context;
		}
		// GROUPS: Index
		public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber, int? directionId, int? id)
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
			IQueryable<Group> groups = _context.Groups.Include(g => g.Direction).AsNoTracking();
			if (directionId.HasValue) groups = groups.Where(g => g.direction == directionId);
			if (!String.IsNullOrEmpty(searchString)) groups = groups.Where(g => g.group_name.Contains(searchString) || g.Direction.direction_name.Contains(searchString));
			switch (sortOrder)
			{
				case "name_desc": groups = groups.OrderByDescending(g => g.group_name).ThenBy(g => g.group_id); break;
				case "direction": groups = groups.OrderBy(g => g.Direction.direction_name).ThenBy(g => g.group_id); break;
				case "direction_desc": groups = groups.OrderByDescending(g => g.Direction.direction_name).ThenBy(g => g.group_id); break;
				case "date": groups = groups.OrderBy(g => g.start_date).ThenBy(g => g.group_id); break;
				case "date_desc": groups = groups.OrderByDescending(g => g.start_date).ThenBy(g => g.group_id); break;
				default: groups = groups.OrderBy(g => g.group_name).ThenBy(g => g.group_id); break;
			}
			int pageSize = 5;
			PaginatedList<Group> items = await PaginatedList<Group>.CreateAsync(groups, pageNumber ?? 1, pageSize);
			Group selected = null;
			if (id.HasValue)
				selected = await _context.Groups.Include(g => g.Direction)
					.Include(g => g.Students.OrderBy(s => s.last_name).ThenBy(s => s.first_name))
					.AsNoTracking().FirstOrDefaultAsync(g => g.group_id == id);
			await DirectionsList(directionId);
			ViewData["DirectionId"] = directionId;
			ViewData["SelectedId"] = id;
			GroupIndexData viewModel = new GroupIndexData();
			viewModel.Groups = items;
			viewModel.SelectedGroup = selected;
			return View(viewModel);
		}
		// GROUPS: Details
		public async Task<IActionResult> Details(int id)
		{
			Group group = await _context.Groups.Include(g => g.Direction)
				.Include(g => g.Students.OrderBy(s => s.last_name).ThenBy(s => s.first_name))
				.AsNoTracking().FirstOrDefaultAsync(g => g.group_id == id);
			if (group == null)
			{
				return NotFound();
			}
			return View(group);
		}
		// GROUPS: Create
		public async Task<IActionResult> Create()
		{
			await DirectionsList();
			return View(new Group());
		}
		[HttpPost, ValidateAntiForgeryToken]
		// GROUPS: Create
		public async Task<IActionResult> Create([Bind("group_name,direction,start_time,start_date")] Group group, int[] selectedDays)
		{
			group.learning_days = Weekdays.FromValues(selectedDays);
			await ValidateDirection(group.direction);
			if (ModelState.IsValid)
			{
				_context.Groups.Add(group);
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!GroupExists(group.group_id))
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
			await DirectionsList(group.direction);
			return View(group);
		}
		// GROUPS: Edit
		public async Task<IActionResult> Edit(int id)
		{
			Group group = await _context.Groups.FindAsync(id);
			if (group == null) return NotFound();
			await DirectionsList(group.direction);
			return View(group);
		}
		[HttpPost, ValidateAntiForgeryToken]
		// GROUPS: Edit
		public async Task<IActionResult> Edit(int id, [Bind("group_id,group_name,direction,start_time,start_date")] Group group, int[] selectedDays)
		{
			if (id != group.group_id) return NotFound();
			Group target = await _context.Groups.FindAsync(id);
			if (target == null) return NotFound();
			group.learning_days = Weekdays.FromValues(selectedDays);
			await ValidateDirection(group.direction);
			if (ModelState.IsValid)
			{
				target.group_name = group.group_name;
				target.direction = group.direction;
				target.learning_days = group.learning_days;
				target.start_time = group.start_time;
				target.start_date = group.start_date;
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!GroupExists(group.group_id))
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
			await DirectionsList(group.direction);
			return View(group);
		}
		// GROUPS: Delete
		public async Task<IActionResult> Delete(int id)
		{
			Group group = await _context.Groups.Include(g => g.Direction).AsNoTracking().FirstOrDefaultAsync(g => g.group_id == id);
			if (group == null)
			{
				return NotFound();
			}
			return View(group);
		}
		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			Group group = await _context.Groups.Include(g => g.Direction).FirstOrDefaultAsync(g => g.group_id == id);
			if (group == null) return NotFound();
			if (await _context.Students.AnyAsync(s => s.group == id))
				ModelState.AddModelError("", "Сначала удалите или перенесите студентов этой группы.");
			else
			{
				_context.Groups.Remove(group);
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!GroupExists(group.group_id))
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
			return View("Delete", group);
		}
		private async Task DirectionsList(int? selected = null)
		{
			IQueryable<Direction> records = from record in _context.Directions
											orderby record.direction_name
											select record;
			ViewBag.Directions = new SelectList(await records.AsNoTracking().ToListAsync(), "direction_id", "direction_name", selected);
		}
		private async Task ValidateDirection(int id)
		{
			if (!await _context.Directions.AnyAsync(d => d.direction_id == id))
				ModelState.AddModelError("direction", "Выберите существующее направление.");
		}
		private bool GroupExists(int? id)
		{
			return _context.Groups.Any(e => e.group_id == id);
		}
	}
}
