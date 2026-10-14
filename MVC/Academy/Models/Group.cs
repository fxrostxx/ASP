using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Academy.Models
{
	[Table("Groups")]
	public class Group
	{
		[Key]
		public int group_id { get; set; }
		[Required(ErrorMessage = "Введите название группы.")]
		[StringLength(10, MinimumLength = 5)]
		[Display(Name = "Группа")]
		[Column(TypeName = "NCHAR(10)")]
		public string group_name { get; set; } = "";
		[Range(1, 255, ErrorMessage = "Выберите направление.")]
		[Display(Name = "Направление")]
		[Column(TypeName = "TINYINT")]
		[ForeignKey(nameof(Direction))]
		public int direction { get; set; }
		[Column("weekdays", TypeName = "TINYINT")]
		public int? learning_days { get; set; } = 0;
		[DataType(DataType.Time)]
		[Display(Name = "Время начала занятий")]
		public TimeOnly? start_time { get; set; }
		[DataType(DataType.Date)]
		[Display(Name = "Дата начала обучения")]
		public DateOnly? start_date { get; set; }
		public Direction Direction { get; set; }
		public ICollection<Student> Students { get; set; } = new List<Student>();
		[NotMapped]
		public string LearningDaysText
		{
			get
			{
				return Weekdays.Format(learning_days ?? 0);
			}
		}
	}
}
