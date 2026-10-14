using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Academy.Models
{
	[Table("Students")]
	public class Student : Human
	{
		[Key]
		public int stud_id { get; set; }
		[Range(1, int.MaxValue, ErrorMessage = "Выберите группу.")]
		[Display(Name = "Группа")]
		[Column("group")]
		[ForeignKey(nameof(Group))]
		public int group { get; set; }
		public Group Group { get; set; }
	}
}
