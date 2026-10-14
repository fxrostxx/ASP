using Academy.Data;
using Academy.Models;
using Academy.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Academy.Controllers
{
	public class DirectionsController : Controller
	{
		private readonly AcademyContext _context;
		public DirectionsController(AcademyContext context)
		{
			_context = context;
		}
		// DIRECTIONS: Index
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
			IQueryable<Direction> directions = _context.Directions.AsNoTracking();
			if (!String.IsNullOrEmpty(searchString)) directions = directions.Where(d => d.direction_name.Contains(searchString));
			switch (sortOrder)
			{
				case "name_desc": directions = directions.OrderByDescending(d => d.direction_name).ThenBy(d => d.direction_id); break;
				default: directions = directions.OrderBy(d => d.direction_name).ThenBy(d => d.direction_id); break;
			}
			int pageSize = 5;
			PaginatedList<Direction> items = await PaginatedList<Direction>.CreateAsync(directions, pageNumber ?? 1, pageSize);
			Direction selected = null;
			if (id.HasValue)
				selected = await _context.Directions.Include(d => d.Groups.OrderBy(g => g.group_name))
					.AsNoTracking().FirstOrDefaultAsync(d => d.direction_id == id);
			ViewData["SelectedId"] = id;
			DirectionIndexData viewModel = new DirectionIndexData();
			viewModel.Directions = items;
			viewModel.SelectedDirection = selected;
			return View(viewModel);
		}
		// DIRECTIONS: Details
		public async Task<IActionResult> Details(int id)
		{
			Direction direction = await _context.Directions.Include(d => d.Groups.OrderBy(g => g.group_name))
				.AsNoTracking().FirstOrDefaultAsync(d => d.direction_id == id);
			if (direction == null)
			{
				return NotFound();
			}
			return View(direction);
		}
		// DIRECTIONS: Create
		public IActionResult Create()
		{
			return View(new Direction());
		}
		[HttpPost, ValidateAntiForgeryToken]
		// DIRECTIONS: Create
		public async Task<IActionResult> Create([Bind("direction_name")] Direction direction)
		{
			if (ModelState.IsValid)
			{
				_context.Directions.Add(direction);
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DirectionExists(direction.direction_id))
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
			return View(direction);
		}
		// DIRECTIONS: Edit
		public async Task<IActionResult> Edit(int id)
		{
			Direction direction = await _context.Directions.FindAsync(id);
			if (direction == null)
			{
				return NotFound();
			}
			return View(direction);
		}
		[HttpPost, ValidateAntiForgeryToken]
		// DIRECTIONS: Edit
		public async Task<IActionResult> Edit(int id, [Bind("direction_id,direction_name")] Direction direction)
		{
			if (id != direction.direction_id) return NotFound();
			Direction target = await _context.Directions.FindAsync(id);
			if (target == null) return NotFound();
			if (ModelState.IsValid)
			{
				target.direction_name = direction.direction_name;
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DirectionExists(direction.direction_id))
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
			return View(direction);
		}
		// DIRECTIONS: Delete
		public async Task<IActionResult> Delete(int id)
		{
			Direction direction = await _context.Directions.AsNoTracking().FirstOrDefaultAsync(d => d.direction_id == id);
			if (direction == null)
			{
				return NotFound();
			}
			return View(direction);
		}
		[HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			Direction direction = await _context.Directions.FindAsync(id);
			if (direction == null) return NotFound();
			if (await _context.Groups.AnyAsync(g => g.direction == id))
				ModelState.AddModelError("", "Сначала удалите или перенесите группы этого направления.");
			else
			{
				_context.Directions.Remove(direction);
				try
				{
					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!DirectionExists(direction.direction_id))
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
			return View("Delete", direction);
		}
		private bool DirectionExists(int? id)
		{
			return _context.Directions.Any(e => e.direction_id == id);
		}
	}
}
