﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IvorySharp.Reflection
{
    internal static class Expressions
    {
        public static Action<object, object> CreatePropertySetter(PropertyInfo property)
        {
            if (property == null) 
                throw new ArgumentNullException(nameof(property));
            
            if (property.DeclaringType == null)
                throw new ArgumentException($"{nameof(property)}.{nameof(property.DeclaringType)}", nameof(property));
            
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            var expression = Expression.Lambda<Action<object, object>>(
                Expression.Call(
                    Expression.Convert(instance, property.DeclaringType),
                    property.SetMethod,
                    Expression.Convert(value, property.PropertyType)
                ), 
                instance, value);

            return expression.Compile();
        }
        
        public static Func<object, object[], object> CreateMethodInvoker(MethodInfo method)
        {
            if (method == null) 
                throw new ArgumentNullException(nameof(method));
            
            if (method.DeclaringType == null)
                throw new ArgumentException($"{nameof(method)}.{nameof(method.DeclaringType)}", nameof(method));
            
            var instanceParameterExpression = Expression.Parameter(typeof(object), "instance");
            var argumentsParameterExpression = Expression.Parameter(typeof(object[]), "args");

            var index = 0;
            var argumentExtractionExpressions =
                method
                    .GetParameters()
                    .Select(parameter =>
                        Expression.Convert(
                            Expression.ArrayAccess(
                                argumentsParameterExpression,
                                Expression.Constant(index++)
                            ),
                            parameter.ParameterType
                        )
                    ).ToList();

            var callExpression = method.IsStatic
                ? Expression.Call(method, argumentExtractionExpressions)
                : Expression.Call(
                    Expression.Convert(
                        instanceParameterExpression,
                        method.DeclaringType
                    ),
                    method,
                    argumentExtractionExpressions
                );

            var endLabel = Expression.Label(typeof(object));
            var finalExpression = method.ReturnType == typeof(void)
                ? (Expression) Expression.Block(
                    callExpression,
                    Expression.Return(endLabel, Expression.Constant(null)),
                    Expression.Label(endLabel, Expression.Constant(null))
                )
                : Expression.Convert(callExpression, typeof(object));

            var lambdaExpression = Expression.Lambda<Func<object, object[], object>>(
                finalExpression,
                instanceParameterExpression,
                argumentsParameterExpression
            );

            return lambdaExpression.Compile();
        }
    }
}