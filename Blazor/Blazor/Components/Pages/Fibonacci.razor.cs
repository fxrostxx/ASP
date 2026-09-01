namespace Blazor.Components.Pages
{
	public partial class Fibonacci
	{
		int length = 0;
		string sequence = "";
		void Calculate()
		{
			sequence = "";
			if (length <= 0)
			{
				sequence = "";
				return;
			}
			int fib1 = 0;
			int fib2 = 1;
			for (int i = 0; i < length; ++i)
			{
				sequence += $"{fib1} ";
				int next = fib1 + fib2;
				fib1 = fib2;
				fib2 = next;
			}
		}
	}
}
