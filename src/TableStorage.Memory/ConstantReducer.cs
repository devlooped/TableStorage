using System.Linq.Expressions;
using System.Reflection;

class ConstantReducer : ExpressionVisitor
{
    protected override Expression VisitMember(MemberExpression node)
    {
        // If the expression is a constant or can be reduced to a constant, evaluate it
        if (node.Expression is ConstantExpression constantExpr)
        {
            object? container = constantExpr.Value;
            object? value = null;

            if (node.Member is FieldInfo field)
                value = field.GetValue(container);
            else if (node.Member is PropertyInfo prop)
                value = prop.GetValue(container);

            return Expression.Constant(value, node.Type);
        }

        // Try to evaluate more complex expressions
        try
        {
            var lambda = Expression.Lambda(node);
            var compiled = lambda.Compile();
            var value = compiled.DynamicInvoke();
            return Expression.Constant(value, node.Type);
        }
        catch
        {
            // If evaluation fails, fallback to default behavior
            return base.VisitMember(node);
        }
    }
}