using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionSimplifier
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			Expression<Func<double, double>> expr = x => x * 3 * x * 4 * x * 2 + - -x + x * x + 1 * x * 5 / x - Math.Sin(6.0);

			Console.WriteLine(expr.ToString());

			var simplifier = new ExpressionSimplifier();
			Expression simplified = expr;
			foreach (var _ in Enumerable.Range(0, 8))
			{
				simplified = simplifier.Visit(simplified);
				Console.WriteLine(simplified.ToString());
			}
		}
	}
}
