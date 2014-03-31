using System;
using System.Linq.Expressions;

namespace ExpressionSimplifier
{
	public static class ExpressionExtensions
	{
		public static Expression Negate(this Expression expr)
		{
			if (expr.NodeType == ExpressionType.Negate)
				return ((UnaryExpression)expr).Operand;
			else
				return Expression.Negate(expr);
		}

		public static bool IsParameter(this Expression expr)
		{
			return expr.NodeType == ExpressionType.Parameter;
		}

		public static bool IsConstant(this Expression expr)
		{
			return expr.NodeType == ExpressionType.Constant;
		}

		public static double GetValueAsConstant(this Expression expr)
		{
			var constexpr = (ConstantExpression)expr;
			return (double)constexpr.Value;
		}

		public static bool Is(this Expression expr, int num)
		{
			if (!expr.IsConstant())
				return false;

			var constexpr = (ConstantExpression)expr;
			if (constexpr.Type == typeof(int))
				return (int)constexpr.Value == num;
			else if (constexpr.Type == typeof(double))
				return (double)constexpr.Value == (double)num;
			else
				return false;
		}
	}
}

