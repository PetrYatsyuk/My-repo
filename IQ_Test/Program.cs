using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IQ_Test
{
    public static class Program
    {
        public static T Apply<T, T1, TResult>(this Expression<Func<T1, TResult, T>> expression, T1 t1, TResult t2)
        {
            return expression.Compile()(t1, t2);
        }
        public static T Simlify<T, TResult>(this Expression<Func<TResult, T>> expression, T t, TResult t1)
        {
            return expression.Compile()(t1);
        }
        public static Expression<Func<T1, T2, T3, TResult>> Simlify2<T1, T2, T3, TResult>(Expression<Func<T1, T2, TResult>> expression, T3 argument)
        {
            Expression arg2 = Expression.Constant(argument, typeof(T3));
            Expression newBody = new Rewriter(expression.Parameters[1], arg2).Visit(expression.Body);
            return Expression.Lambda<Func<T1, T2, T3, TResult>>(newBody, expression.Parameters[0]);
        }

        static void Main(string[] args)
        {
            var myVisitor = new MyExpressionVisitor();

            MyExpressionVisitor.cash = new Dictionary<string, Delegate>();

            //Creat expression which should be replaced
            Expression<Func<int, int>> tsk = (y) => y * 2;
            Delegate del = tsk.Compile();
            MyExpressionVisitor.cash.Add(tsk.Body.ToString(), del);
            
            //Expressoin where spmething should be removed
            Expression<Func<int, int, int>> tsk2 = (x, y) => x > y ? x : (x < (y * 2) ? (y * 2) : y);

            //Predicate variant
            Expression<Func<int, int, int>> taskExpression = (a, b) => a + b;
            Expression<Func<int, int, int, int>> taskExpression2 = (a, b, c) => taskExpression.Apply(a + b, c) * c;

            //var result = PredicateExtensions.PredicateExtensions.Or<int>(taskExpression2, taskExpression2);
            Console.Write(taskExpression2.ToString());

            Console.WriteLine();
            var result2 = myVisitor.Visit(taskExpression2);
            Console.Write(result2.ToString());

            Console.WriteLine();
            result2 = myVisitor.Visit(tsk2);
            Console.WriteLine(result2.ToString());

            MyExpressionVisitor.cash.Clear();
        }
    }
}
