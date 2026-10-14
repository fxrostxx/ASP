using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Academy.Models
{
	[Table("Directions")]
	public class Direction
	{
		[Key]
		[Column(TypeName = "TINYINT")]
		public int direction_id { get; set; }
		[Required(ErrorMessage = "Введите направление.")]
		[StringLength(50, MinimumLength = 2)]
		[Display(Name = "Направление обучения")]
		[Column(TypeName = "NVARCHAR(50)")]
		public string direction_name { get; set; } = "";
		public ICollection<Group> Groups { get; set; } = new List<Group>();
	}
}
