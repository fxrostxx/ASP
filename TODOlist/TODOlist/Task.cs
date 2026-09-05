namespace TODOlist
{
	public class Task
	{
		public string Description { get; set; }
		public bool Done { get; set; }
		public DateTime AssignTime { get; set; }
		public override bool Equals(object? other)
		{
			return this.Description.Equals((other as TODOlist.Task).Description, StringComparison.OrdinalIgnoreCase);
		}
	}
}
