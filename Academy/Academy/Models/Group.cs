using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Academy.Models
{
	public class Group
	{
		[Key]
		public int group_id { get; set; }

		[Required]
		[StringLength(10, MinimumLength = 5)]
		[Column(TypeName = "NCHAR(10)")]
		public string group_name { get; set; }

		[Required]
		[Column(TypeName = "TINYINT")]
		[ForeignKey(nameof(Direction))]
		public int direction { get; set; }

		[Column("weekdays", TypeName = "TINYINT")]
		public int? learning_days { get; set; }

		public TimeOnly? start_time { get; set; }
		public DateOnly? start_date { get; set; }


		public Direction Direction { get; set; }
	}
}
