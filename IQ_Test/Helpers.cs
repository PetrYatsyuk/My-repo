using System;
using System.Linq.Expressions;

namespace IQ_Test
{
    public static class Helpers
    {
        public static string GenerateKey(Expression<Action<String>> expression)
        {
            ParameterExpression newParam = Expression.Parameter(expression.Parameters[0].Type);
            Expression newExpressionBody = Expression.Call(newParam, ((MethodCallExpression)expression.Body).Method);
            Expression<Action<String>> newExpression = Expression.Lambda<Action<String>>(newExpressionBody, newParam);

            return newExpression.ToString();

        }

        public static string GenerateKey(Expression<Func<int, int>> expression)
        {
            ParameterExpression newParam = Expression.Parameter(expression.Parameters[0].Type);
            Expression newExpressionBody = expression;
            Expression<Func<int, int>> newExpression = Expression.Lambda<Func<int, int>>(newExpressionBody, newParam);

            return newExpression.ToString();

        }
    }
}
