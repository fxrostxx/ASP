using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Academy.Models
{
	[Table("Disciplines")]
	public class Discipline
	{
		[Key]
		[Column(TypeName = "SMALLINT")]
		public int discipline_id { get; set; }
		[Required(ErrorMessage = "Введите название дисциплины.")]
		[Display(Name = "Дисциплина")]
		public string discipline_name { get; set; } = "";
		[Range(1, 255)]
		[Display(Name = "Количество занятий")]
		[Column(TypeName = "TINYINT")]
		public int number_of_lessons { get; set; }
		public ICollection<TeachersDisciplinesRelation> TeachersRelations { get; set; }
			= new List<TeachersDisciplinesRelation>();
	}
}
