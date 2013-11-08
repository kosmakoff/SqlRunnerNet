using System;
using System.ComponentModel;
using System.Windows.Input;

namespace SqlServerRunnerNet.Infrastructure.Commands
{
	public class DelegateCommand : DelegateCommand<object>
	{
		public DelegateCommand(Action executeMethod)
			: base(o => executeMethod())
		{
		}

		public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
			: base(o => executeMethod(), o => canExecuteMethod())
		{
		}
	}

	/// <summary>
	/// A command that calls the specified delegate when the command is executed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class DelegateCommand<T> : ICommand, IRaiseCanExecuteChanged
	{
		private readonly Func<T, bool> _canExecuteMethod;
		private readonly Action<T> _executeMethod;

		public delegate void CancelCommandEventHandler(object sender, CancelCommandEventArgs eventArgs);

		public delegate void CommandEventHandler(object sender);

		public event CancelCommandEventHandler Executing;

		public event CommandEventHandler Executed;

		protected virtual void OnExecuting(CancelCommandEventArgs eventargs)
		{
			CancelCommandEventHandler handler = Executing;
			if (handler != null) handler(this, eventargs);
		}

		protected virtual void OnExecuted()
		{
			CommandEventHandler handler = Executed;
			if (handler != null) handler(this);
		}

		public DelegateCommand(Action<T> executeMethod)
			: this(executeMethod, null)
		{
		}

		public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
		{
			if ((executeMethod == null) && (canExecuteMethod == null))
			{
				throw new ArgumentNullException("executeMethod", @"Execute Method cannot be null");
			}
			_executeMethod = executeMethod;
			_canExecuteMethod = canExecuteMethod;
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				CommandManager.RequerySuggested -= value;
			}
		}

		public bool IsExecuting { get; private set; }

		public void RaiseCanExecuteChanged()
		{
			CommandManager.InvalidateRequerySuggested();
		}

		bool ICommand.CanExecute(object parameter)
		{
			return !IsExecuting && CanExecute((T)parameter);
		}

		void ICommand.Execute(object parameter)
		{
			var args = new CancelCommandEventArgs();
			OnExecuting(args);

			if (args.Cancel)
				return;
			
			IsExecuting = true;
			try
			{
				RaiseCanExecuteChanged();
				Execute((T)parameter);
			}
			finally
			{
				IsExecuting = false;
				RaiseCanExecuteChanged();
			}

			OnExecuted();
		}

		public bool CanExecute(T parameter)
		{
			if (_canExecuteMethod == null)
				return true;

			return _canExecuteMethod(parameter);
		}

		public void Execute(T parameter)
		{
			_executeMethod(parameter);
		}

		public class CancelCommandEventArgs : EventArgs
		{
			public bool Cancel { get; set; }

			public CancelCommandEventArgs()
			{
				Cancel = false;
			}
		}
	}
}
