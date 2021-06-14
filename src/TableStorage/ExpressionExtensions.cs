using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Devlooped
{
    static class ExpressionExtensions
    {
        public static string? GetPropertyName<T, TResult>(this Expression<Func<T, TResult>>? expression)
        {
            if (expression == null)
                return null;

            if (expression.Body is MemberExpression member &&
                member.Member is PropertyInfo property)
                return property.Name;

            var visitor = new FindPropertyVisitor();
            visitor.Visit(expression);

            return visitor.Property?.Name;
        }

        class FindPropertyVisitor : ExpressionVisitor
        {
            public PropertyInfo? Property { get; set; }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Member is PropertyInfo property)
                    Property = property;

                return base.VisitMember(node);
            }
        }
    }
}
