﻿using System;
using IvorySharp.Core;
using IvorySharp.Exceptions;
using IvorySharp.Extensions;

namespace IvorySharp.Aspects.Pipeline
{
    /// <summary>
    /// Реализация пайплайна выполнения метода.
    /// </summary>
    internal class InvocationPipeline : IInvocationPipeline
    {
        /// <summary>
        /// Текущий выполняемый аспект.
        /// </summary>
        internal IMethodAspect CurrentExecutingAspect { get; set; }

        /// <inheritdoc />
        public InvocationContext Context { get; }

        /// <inheritdoc />
        public Exception CurrentException { get; set; }

        /// <inheritdoc />
        public FlowBehaviour FlowBehaviour { get; set; }

        /// <inheritdoc />
        public bool CanReturnResult { get; }

        /// <summary>
        /// Инициализирует экземпляр <see cref="InvocationPipeline"/>.
        /// </summary>
        /// <param name="invocationContext">Контекст выполнения.</param>
        internal InvocationPipeline(InvocationContext invocationContext)
        {
            Context = invocationContext;
            CanReturnResult = !Context.Method.IsVoidReturn();
        }

        /// <inheritdoc />
        public void ReturnValue(object returnValue)
        {
            CurrentException = null;
            FlowBehaviour = FlowBehaviour.Return;
            
            if (Context.Method.IsVoidReturn())
            {
                throw new IvorySharpException(
                    $"Невозможно вернуть значение '{returnValue}' из аспекта '{CurrentExecutingAspect?.GetType().FullName}'. " +
                    $"Метод '{Context.Method.Name}' типа '{Context.InstanceDeclaredType.FullName}' " +
                    $"не имеет возвращаемого значения (void). " +
                    $"Для возврата используйте перегрузку '{nameof(ReturnValue)}' без параметров.");
            }

            if (returnValue == null)
            {
                Context.ReturnValue = Context.Method.ReturnType.GetDefaultValue();
            }
            else
            {
                if (!Context.Method.ReturnType.IsInstanceOfType(returnValue))
                {
                    throw new IvorySharpException(
                        $"Невозможно вернуть значение '{returnValue}' из аспекта '{CurrentExecutingAspect?.GetType().FullName}'. " +
                        $"Тип результата '{returnValue.GetType().FullName}' невозможно привести к возвращаемому типу '{Context.Method.ReturnType.FullName}' " +
                        $"метода '{Context.Method.Name}' сервиса 'InvocationContext.InstanceDeclaringType.FullName'.");
                }

                Context.ReturnValue = returnValue;
            }
        }

        /// <inheritdoc />
        public void ReturnDefault()
        {
            FlowBehaviour = FlowBehaviour.Return;
            Context.ReturnValue = Context.Method.ReturnType.GetDefaultValue();
        }

        /// <inheritdoc />
        public void Return()
        {
            FlowBehaviour = FlowBehaviour.Return;

            // Если забыли указать результат, то ставим результат по умолчанию
            // Возможно лучше отдельно проверять этот кейс и кидать исключение
            if (Context.ReturnValue == null)
            {
                Context.ReturnValue = Context.Method.ReturnType.GetDefaultValue();
            }
        }

        /// <inheritdoc />
        public void ThrowException(Exception exception)
        {
            CurrentException = exception;
            FlowBehaviour = FlowBehaviour.ThrowException;
        }
    }
}