namespace Academy.Models
{
	public class Weekdays
	{
		public static readonly string[] Names = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
		public static string Format(int mask)
		{
			List<string> result = new List<string>();
			for (int i = 0; i < 7; i++)
			{
				if ((mask & (1 << i)) != 0)
				{
					result.Add(Names[i]);
				}
			}
			if (result.Count == 0) return "Не заданы";
			return String.Join(", ", result);
		}
		public static int FromValues(int[] values)
		{
			int mask = 0;
			foreach (int day in values)
			{
				if (day >= 0 && day < 7)
				{
					mask = mask | (1 << day);
				}
			}
			return mask;
		}
	}
}
