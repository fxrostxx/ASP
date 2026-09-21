namespace Academy.Components.Pages.GroupPages
{
	public class Week
	{
		public static readonly string[] DAY_NAMES = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
		public int Days { get; set; }
		public Week(int days)
		{
			this.Days = days;
		}
		public override string ToString()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < 7; ++i) if (((Days >> i) & 1) != 0) result.Add(DAY_NAMES[i]);
			return string.Join(", ", result);
		}
	}
}
