using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models
{
	public class OfficeAssignment
	{
		public int InstructorID { get; set; }

		[StringLength(50)]
		[Display(Name = "Office location")]
		public string Location { get; set; }


		public Instructor instructor { get; set; }
	}
}
