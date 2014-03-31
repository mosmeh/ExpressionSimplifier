using System;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionSimplifier
{
	public class ExpressionSimplifier : ExpressionVisitor
	{
		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return Expression.Lambda(this.Visit(node.Body), node.Parameters);
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			var op = node.Operand;
			if (op.NodeType == ExpressionType.Negate)
				return this.Visit(((UnaryExpression)op).Operand);
			else
				return base.VisitUnary(node);
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			var left = node.Left;
			var right = node.Right;

			// some simplifying is not performed because of special values such as NaN and Infinity
			switch (node.NodeType)
			{
				case ExpressionType.Add:
				{
					// a + b -> const
					if (left.IsConstant() && right.IsConstant())
						return Expression.Constant(left.GetValueAsConstant() + right.GetValueAsConstant());
					// x + x -> 2 * x
					else if (left == right)
						return Expression.Multiply(Expression.Constant(2.0), this.Visit(left));
					else
						return Expression.Add(this.Visit(left), this.Visit(right));
				}

				case ExpressionType.Subtract:
				{
					// a - b -> const
					if (left.IsConstant() && right.IsConstant())
						return Expression.Constant(left.GetValueAsConstant() - right.GetValueAsConstant());
					// x - 0 -> x
					else if (!left.Is(0) && right.Is(0))
						return this.Visit(left);
					else
						return Expression.Subtract(this.Visit(left), this.Visit(right));
				}
				
				case ExpressionType.Multiply:
				{
					// a * b -> const
					if (left.IsConstant() && right.IsConstant())
						return Expression.Constant(left.GetValueAsConstant() * right.GetValueAsConstant());
					// 1 * y -> y
					else if (left.Is(1) && !right.Is(1))
						return this.Visit(right);
					// x * 1 -> x
					else if (!left.Is(1) && right.Is(1))
						return this.Visit(left);
					// x * x -> x ^ 2
					else if (left == right)
						return Expression.Power(this.Visit(left), Expression.Constant(2.0));
					// x * (y * z) -> (y * z) * x, a * (y * z) -> (y * z) * a
					else if ((left.IsParameter() || left.IsConstant()) && this.Visit(right).NodeType == ExpressionType.Multiply)
						return Expression.Multiply(this.Visit(right), left);
					else if (this.Visit(left).NodeType == ExpressionType.Multiply && right.IsConstant())
					{
						var mul = (BinaryExpression)left;
						// (x * a) * b -> (b * a) * x
						if (mul.Left.IsParameter())
							return Expression.Multiply(Expression.Multiply(right, mul.Right), mul.Left);
						// (a * x) * b -> (b * a) * x
						else if (mul.Right.IsParameter())
							return Expression.Multiply(Expression.Multiply(right, mul.Left), mul.Right);
						else
							return Expression.Multiply(this.Visit(left), this.Visit(right));
					}
					else
						return Expression.Multiply(this.Visit(left), this.Visit(right));
				}

				case ExpressionType.Divide:
				{
					// a / b -> const
					if (left.IsConstant() && right.IsConstant())
						return Expression.Constant(left.GetValueAsConstant() / right.GetValueAsConstant());
					// x / 1 -> x
					else if (!left.Is(1) && right.Is(1))
						return this.Visit(left);
					else
						return Expression.Divide(this.Visit(left), this.Visit(right));
				}

				default:
					return base.VisitBinary(node);
			}
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Arguments.All(x => x.IsConstant()))
			{
				return Expression.Constant(
							node.Method.Invoke(
								node.Method.ReflectedType,
								node.Arguments.OfType<ConstantExpression>().Select(x => x.Value).OfType<object>().ToArray())
				);
			}
			else
				return Expression.Call(node.Method, node.Arguments.Select(x => this.Visit(x)));
		}
	}
}
