using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
	public class Course
	{
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public int CourseID { get; set; }
		public string Title { get; set; }
		public int Credits { get; set; }


		public ICollection<Enrollment> Enrollments { get; set; }
		public Department Department { get; set; }
		public ICollection<Instructor> Instructors { get; set; }
	}
}
