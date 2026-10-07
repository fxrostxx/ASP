using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models
{
	public class Instructor
	{
		public int ID { get; set; }

		[Required]
		[StringLength(50)]
		[DisplayName("Last name")]
		public string LastName { get; set; }

		[Required]
		[StringLength(50)]
		[DisplayName("First name")]
		public string FirstName { get; set; }

		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		[Display(Name = "Hire date")]
		public DateTime HireDate { get; set; }


		[Display(Name = "Instructor")]
		public string FullName
		{
			get => $"{LastName} {FirstName}";
		}


		// TODO: Navigation Properties
	}
}
