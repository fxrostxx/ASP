using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace Academy.Models
{
	public class Direction
	{
		[Key]
		[Column(TypeName = "TINYINT")]
		public int direction_id { get; set; }

		[Required]
		[StringLength(50, MinimumLength = 2)]
		[Column(TypeName = "NVARCHAR(50)")]
		public string direction_name { get; set; }
	}
}
