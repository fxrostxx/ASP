using Microsoft.EntityFrameworkCore;
namespace Academy
{
	public class PaginatedList<T> : List<T>
	{
		public int PageIndex { get; }
		public int TotalPages { get; }
		public int TotalCount { get; }
		public bool HasPreviousPage => PageIndex > 1;
		public bool HasNextPage => PageIndex < TotalPages;
		public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
		{
			PageIndex = pageIndex;
			TotalCount = count;
			TotalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
			AddRange(items);
		}
		public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> query, int pageIndex, int pageSize = 5)
		{
			int count = await query.CountAsync();
			int totalPages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
			pageIndex = Math.Clamp(pageIndex, 1, totalPages);
			List<T> items = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
			return new PaginatedList<T>(items, count, pageIndex, pageSize);
		}
	}
}
