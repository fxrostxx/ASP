namespace Blazor.Components.Pages
{
	public partial class Power
	{
		int baseNum = 0;
		int powerNum = 0;
		double result = 0;
		void Calculate()
		{
			result = 1;
			if (powerNum == 0) result = 1;
			for (int i = 0; i < Math.Abs(powerNum); ++i) result *= baseNum;
			if (powerNum < 0) result = 1 / result;
		}
	}
}
