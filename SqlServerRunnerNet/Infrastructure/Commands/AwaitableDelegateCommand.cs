using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SqlServerRunnerNet.Infrastructure.Commands
{
	public class AwaitableDelegateCommand : AwaitableDelegateCommand<object>, IAsyncCommand
	{
		public AwaitableDelegateCommand(Func<Task> executeMethod)
			: base(o => executeMethod())
		{
		}

		public AwaitableDelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
			: base(o => executeMethod(), o => canExecuteMethod())
		{
		}
	}

	public class AwaitableDelegateCommand<T> : IAsyncCommand<T>, ICommand
	{
		private readonly Func<T, Task> _executeMethod;
		private readonly DelegateCommand<T> _underlyingCommand;

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


		public AwaitableDelegateCommand(Func<T, Task> executeMethod)
			: this(executeMethod, _ => true)
		{
		}

		public AwaitableDelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
		{
			_executeMethod = executeMethod;
			_underlyingCommand = new DelegateCommand<T>(x => { }, canExecuteMethod);
		}

		public bool IsExecuting { get; private set; }
		
		public async Task ExecuteAsync(T obj)
		{
			var args = new CancelCommandEventArgs();
			OnExecuting(args);

			if (args.Cancel)
				return;

			try
			{
				IsExecuting = true;
				RaiseCanExecuteChanged();
				await _executeMethod(obj);
			}
			finally
			{
				IsExecuting = false;
				RaiseCanExecuteChanged();
			}

			OnExecuted();
		}

		public ICommand Command { get { return this; } }

		public bool CanExecute(object parameter)
		{
			return !IsExecuting && _underlyingCommand.CanExecute((T)parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add { _underlyingCommand.CanExecuteChanged += value; }
			remove { _underlyingCommand.CanExecuteChanged -= value; }
		}

		public async void Execute(object parameter)
		{
			await ExecuteAsync((T)parameter);
		}

		public void RaiseCanExecuteChanged()
		{
			_underlyingCommand.RaiseCanExecuteChanged();
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
