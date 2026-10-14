namespace Academy.Models.ViewModels
{
	public class GroupIndexData
	{
		public PaginatedList<Group> Groups { get; set; }
		public Group SelectedGroup { get; set; }
	}
}
