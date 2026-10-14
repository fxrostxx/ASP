namespace Academy.Models.ViewModels
{
	public class TeacherIndexData
	{
		public PaginatedList<Teacher> Teachers { get; set; }
		public Teacher SelectedTeacher { get; set; }
	}
}
