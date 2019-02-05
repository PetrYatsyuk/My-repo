using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IQ_Test
{
    public class MyExpressionVisitor : ExpressionVisitor
    {
        public static Dictionary<string, Delegate> cash;

        #region protected
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Apply")
            {
                var lambda = GetLambda(node.Arguments[0]);
                return Replace(lambda, node.Arguments.Skip(1));
            }
            else if (node.Method.Name == "Simlify")
            {
                var lambda = GetLambda(node.Arguments[0]);
                return Replace(lambda, node.Arguments.Skip(1));
            }
            return base.VisitMethodCall(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            var constantExpression = (ConstantExpression)node.Expression;
            var info = (FieldInfo)node.Member;
            var fieldView = (Expression)info.GetValue(constantExpression.Value);
            return fieldView;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Console.Write(node.Name);
            return base.VisitParameter(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Console.Write(node.Value);
            return base.VisitConstant(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Console.Write("(");

            if (cash.ContainsKey(node.ToString()))
            {
                Console.Write(" ? ");

                this.Visit(Expression.Parameter(typeof(int), "p"));
            } else
            {
                this.Visit(node.Left);
            }

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                    Console.Write(" + ");
                    break;
                case ExpressionType.Divide:
                    Console.Write(" / ");
                    break;
                case ExpressionType.Equal:
                    break;
                case ExpressionType.ExclusiveOr:
                    break;
                case ExpressionType.GreaterThan:
                    if (cash.ContainsKey(node.Right.ToString()))
                    {
                        Console.Write("p");
                        this.Visit(node.Left);
                    }
                    Console.Write(" > ");
                    break;
                case ExpressionType.LessThan:
                    Console.Write(" < ");
                    if (cash.ContainsKey(node.Right.ToString()))
                    {
                        Console.Write("p(y)");
                        // Here we can make pre-calcullation by calling  Delegate
                        Delegate del;
                        cash.TryGetValue(node.Right.ToString(), out del);
                        var value = del.DynamicInvoke(2);

                        this.Visit(Expression.Parameter(typeof(int), "p"));
                    }
                    
                    break;
                default:
                    break;
            }

            this.Visit(node.Right);

            Console.Write(")");

            return node;
        }
        #endregion

        #region private
        private Expression Replace(LambdaExpression lambda, IEnumerable<Expression> arguments)
        {
            var replacer = new Replacer(lambda.Parameters, arguments);
            return replacer.Replace(lambda.Body);
        }


        private LambdaExpression GetLambda(Expression expression)
        {
            var finder = new FieldLambdaFinder();
            return (LambdaExpression)finder.Find(expression);
        }
        #endregion

        #region public
        public Expression Find(Expression expression)
        {
            return Visit(expression);
        }
        #endregion
    }

    internal class Replacer : ExpressionVisitor
    {
        private Dictionary<ParameterExpression, Expression> _replacements;

        public Replacer(IEnumerable<ParameterExpression> what, IEnumerable<Expression> with)
        {
            _replacements = what.Zip(with, (param, expr) => new { param, expr }).ToDictionary(x => x.param, x => x.expr);
        }

        public Expression Replace(Expression body)
        {
            return Visit(body);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Expression replacement;
            return _replacements.TryGetValue(node, out replacement) ? replacement : base.VisitParameter(node);
        }
    }

    class FieldLambdaFinder : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var constantExpression = (ConstantExpression)node.Expression;
            var info = (FieldInfo)node.Member;
            var fieldValue = (Expression)info.GetValue(constantExpression.Value);
            return fieldValue;
        }

        public Expression Find(Expression expression)
        {
            return Visit(expression);
        }
    }

    public class Rewriter : ExpressionVisitor
    {
        private readonly Expression candidate_;
        private readonly Expression replacement_;

        public Rewriter(Expression candidate, Expression replacement)
        {
            candidate_ = candidate;
            replacement_ = replacement;
        }

        public override Expression Visit(Expression node)
        {
            return node == candidate_ ? replacement_ : base.Visit(node);
        }
    }
}
