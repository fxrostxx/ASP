using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Academy.Models
{
	public class Human
	{
		[Required(ErrorMessage = "Введите фамилию.")]
		[StringLength(50, MinimumLength = 2)]
		[Display(Name = "Фамилия")]
		public string last_name { get; set; } = "";
		[Required(ErrorMessage = "Введите имя.")]
		[StringLength(50, MinimumLength = 2)]
		[Display(Name = "Имя")]
		public string first_name { get; set; } = "";
		[StringLength(50)]
		[Display(Name = "Отчество")]
		public string middle_name { get; set; }
		[DataType(DataType.Date)]
		[Display(Name = "Дата рождения")]
		public DateOnly birth_date { get; set; }
		[EmailAddress(ErrorMessage = "Проверьте адрес электронной почты.")]
		[StringLength(100)]
		[Display(Name = "Электронная почта")]
		public string email { get; set; }
		[StringLength(30)]
		[Display(Name = "Телефон")]
		public string phone { get; set; }
		[Column(TypeName = "image")]
		public byte[] photo { get; set; }
		[NotMapped]
		public string FullName
		{
			get
			{
				return $"{last_name} {first_name} {middle_name}".Trim();
			}
		}
		[NotMapped]
		public int Age
		{
			get
			{
				DateOnly today = DateOnly.FromDateTime(DateTime.Today);
				int years = today.Year - birth_date.Year;
				if (birth_date.AddYears(years) > today) years--;
				return Math.Max(0, years);
			}
		}
	}
}
