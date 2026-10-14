using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Academy.Models
{
	[Table("Teachers")]
	public class Teacher : Human
	{
		[Key]
		[Column(TypeName = "SMALLINT")]
		public int teacher_id { get; set; }
		[DataType(DataType.Date)]
		[Display(Name = "Начало преподавания")]
		public DateOnly work_since { get; set; }
		[Range(typeof(decimal), "0", "214748.3647")]
		[Display(Name = "Ставка")]
		[Column(TypeName = "SMALLMONEY")]
		public decimal rate { get; set; }
		public ICollection<TeachersDisciplinesRelation> DisciplinesRelations { get; set; }
			= new List<TeachersDisciplinesRelation>();
		[NotMapped]
		public string Experience
		{
			get
			{
				DateOnly today = DateOnly.FromDateTime(DateTime.Today);
				int months = (today.Year - work_since.Year) * 12 + today.Month - work_since.Month;
				if (work_since.AddMonths(months) > today) months--;
				months = Math.Max(0, months);
				if (months < 12) return $"{months} мес.";
				return $"{months / 12} г. {months % 12} мес.";
			}
		}
	}
}
