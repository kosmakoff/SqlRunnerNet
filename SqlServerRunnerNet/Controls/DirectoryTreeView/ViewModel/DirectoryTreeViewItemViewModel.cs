using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlServerRunnerNet.Controls.DirectoryTreeView.Data;
using SqlServerRunnerNet.Infrastructure;
using SqlServerRunnerNet.Utils;
using SqlServerRunnerNet.ViewModel;

namespace SqlServerRunnerNet.Controls.DirectoryTreeView.ViewModel
{
	public class DirectoryTreeViewItemViewModel : NotifyPropertyChangedBase
	{
		#region Data

		private static readonly DirectoryTreeViewItemViewModel DummyChild = new DirectoryTreeViewItemViewModel();

		private readonly List<DirectoryTreeViewItemViewModel> _children;
		private readonly DirectoryTreeViewItemViewModel _parent;

		private bool _isExpanded;
		private bool _isSelected;

		private bool? _isChecked;

		#endregion // Data

		#region Constructors

		public DirectoryTreeViewItemViewModel(DirectoryTreeViewItemViewModel parent, bool lazyLoad)
		{
			_parent = parent;
			_children = new List<DirectoryTreeViewItemViewModel>();

			if (lazyLoad)
				_children.Add(DummyChild);

			_isChecked = false;
		}

		// This is used to create the DummyChild instance.
		private DirectoryTreeViewItemViewModel()
		{
		}

		#endregion // Constructors

		#region Presentation Members

		#region Children

		/// <summary>
		///     Returns the logical child items of this object.
		/// </summary>
		public List<DirectoryTreeViewItemViewModel> Children
		{
			get { return _children; }
		}

		#endregion // Children

		#region HasLoadedChildren

		/// <summary>
		///     Returns true if this object's Children have not yet been populated.
		/// </summary>
		public bool HasDummyChild
		{
			get { return Children.Count == 1 && Children[0] == DummyChild; }
		}

		#endregion // HasLoadedChildren

		#region IsExpanded

		/// <summary>
		///     Gets/sets whether the TreeViewItem
		///     associated with this object is expanded.
		/// </summary>
		public bool IsExpanded
		{
			get { return _isExpanded; }
			set
			{
				bool propertyChanged = false;

				if (value != _isExpanded)
				{
					_isExpanded = value;
					propertyChanged = true;
				}

				// Expand all the way up to the root.
				if (_isExpanded && _parent != null) _parent.IsExpanded = true;

				// Lazy load the child items, if necessary.
				EnsureItemsLoaded();

				if (_isExpanded && !Children.Any())
				{
					_isExpanded = false;
					propertyChanged = true;
				}

				if (propertyChanged)
					OnPropertyChanged();
			}
		}

		public void EnsureItemsLoaded()
		{
			if (HasDummyChild)
			{
				Children.Remove(DummyChild);
				LoadChildren();
			}
		}

		#endregion // IsExpanded

		#region IsSelected

		/// <summary>
		///     Gets/sets whether the TreeViewItem
		///     associated with this object is selected.
		/// </summary>
		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value != _isSelected)
				{
					if (value && Parent != null)
						Parent.IsExpanded = true;

					_isSelected = value;
					OnPropertyChanged();
				}
			}
		}

		#endregion // IsSelected

		#region IsChecked

		public bool? IsChecked
		{
			get { return _isChecked; }
			set { SetIsChecked(value, false, false); }
		}

		private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
		{
			if (value == _isChecked)
				return;

			_isChecked = value;

			if (updateChildren && _isChecked.HasValue)
			{
				EnsureItemsLoaded();
				Children.ToList().ForEach(c => c.SetIsChecked(_isChecked, true, false));
			}

			if (updateParent && _parent != null)
				_parent.VerifyCheckState();

			OnPropertyChanged("IsChecked");
		}

		private void VerifyCheckState()
		{
			bool? state = null;
			for (int i = 0; i < Children.Count; ++i)
			{
				bool? current = Children[i].IsChecked;
				if (i == 0)
				{
					state = current;
				}
				else if (state != current)
				{
					state = null;
					break;
				}
			}
			SetIsChecked(state, false, true);
		}

		#endregion

		#region LoadChildren

		/// <summary>
		///     Invoked when the child items need to be loaded on demand.
		///     Subclasses can override this to populate the Children collection.
		/// </summary>
		protected virtual void LoadChildren()
		{
			Children.AddRange(
				new DirectoryInfo(FullPath).EnumerateDirectories()
					.Where(dirInfo => FileSystemUtils.CanReadDirectory(dirInfo.FullName))
					.OrderBy(dirInfo => dirInfo.Name, new StringWithNumericsComparer())
					.Select(dirInfo => new FolderViewModel(new Folder(dirInfo.FullName), this)));

			foreach (var model in Children.OfType<FolderViewModel>())
			{
				var dirInfo = new DirectoryInfo(model.FullPath);

				if (!FileSystemUtils.CanReadDirectory(dirInfo.FullName) || !dirInfo.EnumerateDirectories().Any())
					model.Children.Clear();
			}
		}

		#endregion // LoadChildren

		#region Parent

		public DirectoryTreeViewItemViewModel Parent
		{
			get { return _parent; }
		}

		#endregion // Parent

		#endregion // Presentation Members

		public virtual string FullPath
		{
			get { return string.Empty; }
		}

		public virtual string DisplayPath
		{
			get { return string.Empty; }
		}

		public override string ToString()
		{
			return FullPath;
		}
	}
}
