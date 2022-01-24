using RubyUIExtension.Abstractions;
using System;
using System.Windows.Input;

namespace RubyUIExtension.Common
{
    public sealed class RCommand : ICommand
    {

        #region Actions

        private readonly Action targetExecuteMethod;
        private readonly Action<object> targetExecuteMethodWithParam;

        private readonly Func<bool> targetCanExecuteMethod;

        #endregion

        #region Constructors

        public RCommand(Action executeMethod)
        {
            targetExecuteMethod = executeMethod;
        }
        public RCommand(Action<object> executeMethod)
        {
            targetExecuteMethodWithParam = executeMethod;
        }

        public RCommand(Action executeMethod, Func<bool> canExecuteMethod)
        {
            targetExecuteMethod = executeMethod;
            targetCanExecuteMethod = canExecuteMethod;
        }

        public RCommand(Action<object> executeMethod, Func<bool> canExecuteMethod)
        {
            targetExecuteMethodWithParam = executeMethod;
            targetCanExecuteMethod = canExecuteMethod;
        }

        #endregion


        public bool CanExecute(object parameter)
        {
            if (targetCanExecuteMethod != null)
            {
                return targetCanExecuteMethod();
            }

            if (targetExecuteMethod != null)
            {
                return true;
            }

            return false;
        }

        public void Execute()
        {
            try
            {
                ExecuteBegin?.Invoke();
                targetExecuteMethod();
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Trace exception : {ex.Message}");
#endif
            }
            finally
            {
                ExecuteEnd?.Invoke();
            }
        }
        public void Execute(object parameter)
        {
            try
            {
                ExecuteBegin?.Invoke();
                targetExecuteMethodWithParam(parameter);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine($"Trace exception : {ex.Message}");
#endif
            }
            finally
            {
                ExecuteEnd?.Invoke();
            }
        }

        public event EventCallback ExecuteBegin;
        public event EventCallback ExecuteEnd;

        public event EventHandler CanExecuteChanged;

        public static RCommand CreateCommand(ref RCommand command, Action executeAction)
        {
            if (command == null)
                command = new RCommand(executeAction);

            return command;
        }

        public static RCommand CreateCommand(ref RCommand command, Action executeAction, Func<bool> predicateAction)
        {
            if (command == null)
                command = new RCommand(executeAction, predicateAction);

            return command;
        }

    }
}