using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using SqlServerRunnerNet.Annotations;

namespace SqlServerRunnerNet.Infrastructure.Commands
{
	public class DelayableCommand : INotifyPropertyChanged
	{
		private Timer _timer;
		private CancellationTokenSource _cts;
		private readonly Func<CancellationToken, Task> _action;
		private bool _isBusy;

		public TimeSpan Delay { get; private set; }

		public ICommand ReStartCommand { get; private set; }

		public bool IsBusy
		{
			get { return _isBusy; }
			private set
			{
				if (value.Equals(_isBusy)) return;
				_isBusy = value;
				OnPropertyChanged();
			}
		}

		public DelayableCommand(Func<CancellationToken, Task> action, TimeSpan delay)
		{
			Delay = delay;
			ReStartCommand = new DelegateCommand(ReStartCommandExecute);
			_action = action;

			_cts = new CancellationTokenSource();
			ReSetTimer();
		}

		private void ReStartCommandExecute()
		{
			CancelCurrent();
			ReSetTimer();
		}

		private void CancelCurrent()
		{
			if (_cts != null)
			{
				_cts.Cancel();
				_cts.Dispose();

				_cts = new CancellationTokenSource();
			}
		}

		private async void TimerCallback(object state)
		{
			DisposeTimer();

			IsBusy = true;

			try
			{
				await _action(_cts.Token);
			}
			catch (OperationCanceledException)
			{
				// do nothing here
			}

			IsBusy = false;
		}

		private void ReSetTimer()
		{
			if (_timer != null)
			{
				_timer.Change(Delay, TimeSpan.FromMilliseconds(-1));
			}
			else
			{
				_timer = new Timer(TimerCallback, null, Delay, TimeSpan.FromMilliseconds(-1));
			}
		}

		private void DisposeTimer()
		{
			if (_timer != null)
			{
				_timer.Dispose();
				_timer = null;
			}
		}

		#region INotifyPropertyChanged interface implementation

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
	}
}
