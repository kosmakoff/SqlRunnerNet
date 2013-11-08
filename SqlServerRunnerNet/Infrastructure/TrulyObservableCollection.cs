using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SqlServerRunnerNet.Infrastructure
{
	public sealed class TrulyObservableCollection<T> : ObservableCollection<T> where T : class, INotifyPropertyChanged, new()
	{
		private readonly object _lock = new object();
		private volatile bool _willNotify = true;

		public event EventHandler<ItemChangedEventArgs> ItemChanged;

		private void OnItemChangedEventHandler(ItemChangedEventArgs e)
		{
			EventHandler<ItemChangedEventArgs> handler = ItemChanged;
			if (handler != null) handler(this, e);
		}

		public TrulyObservableCollection()
		{
			CollectionChanged += OnCollectionChanged;
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Remove)
			{
				foreach (T item in notifyCollectionChangedEventArgs.OldItems)
				{
					//Removed items
					item.PropertyChanged -= ItemPropertyChanged;
				}
			}
			else if (notifyCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
			{
				foreach (T item in notifyCollectionChangedEventArgs.NewItems)
				{
					//Added items
					item.PropertyChanged += ItemPropertyChanged;
				}
			}
		}

		private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			lock (_lock)
			{
				if (_willNotify)
					OnItemChangedEventHandler(new ItemChangedEventArgs());
			}
		}

		public class ItemChangedEventArgs : EventArgs
		{
		}

		public void InvokeWithoutNotify(Action<T> action)
		{
			if (action == null)
				return;

			lock (_lock)
			{
				_willNotify = false;

				Items.ToList().ForEach(action);

				_willNotify = true;
			}
		}
	}
}
