using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Models
{
	[PrimaryKey(nameof(CourseID), nameof(InstructorID))]
	public class CourseAssignment
	{
		public int CourseID { get; set; }
		public int InstructorID { get; set; }


		public Course Course { get; set; }
		public Instructor Instructor { get; set; }
	}
}
