namespace Academy.Models.ViewModels
{
	public class DisciplineIndexData
	{
		public PaginatedList<Discipline> Disciplines { get; set; }
		public Discipline SelectedDiscipline { get; set; }
	}
}
