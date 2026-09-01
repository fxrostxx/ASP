namespace Blazor.Components.Pages
{
	public partial class Counter
	{
		private int currentCount = 0;
		private int increment = 1;

		private void IncrementCount()
		{
			currentCount += increment;
		}
		private void Reset()
		{
			currentCount = 0;
		}
	}
}
