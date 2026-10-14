namespace Academy.Models.ViewModels
{
	public class DirectionIndexData
	{
		public PaginatedList<Direction> Directions { get; set; }
		public Direction SelectedDirection { get; set; }
	}
}
