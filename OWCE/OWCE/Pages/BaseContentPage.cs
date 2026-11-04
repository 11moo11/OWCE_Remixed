using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using OWCE.Views;
namespace OWCE.Pages
{
    [ContentProperty("BaseContent")]
    public class BaseContentPage : ContentPage
    {
		public static readonly BindableProperty BaseContentProperty = BindableProperty.Create(
			nameof(BaseContent),
			typeof(View),
			typeof(BaseContentPage));

		public View BaseContent
		{
			get { return (View)GetValue(BaseContentProperty); }
			set { SetValue(BaseContentProperty, value); }
		}



		public static readonly BindableProperty NavigationBarHeightProperty = BindableProperty.Create(
			nameof(NavigationBarHeight),
			typeof(int),
			typeof(BaseContentPage));

		public int NavigationBarHeight
		{
			get { return (int)GetValue(NavigationBarHeightProperty); }
			set { SetValue(NavigationBarHeightProperty, value); }
		}


		public static readonly BindableProperty CustomToolbarItemsProperty = BindableProperty.Create(
			nameof(CustomToolbarItems),
			typeof(ObservableCollection<CustomToolbarItem>),
			typeof(BaseContentPage));

		public ObservableCollection<CustomToolbarItem> CustomToolbarItems
		{
			get { return (ObservableCollection<CustomToolbarItem>)GetValue(CustomToolbarItemsProperty); }
			set { SetValue(CustomToolbarItemsProperty, value); }
		}


		Grid _mainGrid;
		BoxView _navBackgroundView;
		Grid _navigationBarGrid;
		StackLayout _leftNavStackLayout;
		StackLayout _rightNavStackLayout;
		Label _titleLabel;

		public BaseContentPage()
		{
			CustomToolbarItems = new ObservableCollection<CustomToolbarItem>();

			Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);

			CustomToolbarItems.CollectionChanged += (sender, e) =>
			{
				if (e.Action == NotifyCollectionChangedAction.Add)
                {
					AddToolbarItems(e.NewItems);
				}
				else if (e.Action == NotifyCollectionChangedAction.Remove)
				{
					RemoveToolbarItems(e.OldItems);
				}
				else
                {
					RebuildToolbar();
                }
			};

			NavigationBarHeight = DeviceInfo.Current.Platform == DevicePlatform.iOS ? 44 : 50;

			// The custom navigation bar below fills the top of the page, so the page itself takes the navigation bar colour.
			// This is what shows behind the status bar / notch (the page content is laid out inside the safe area).
			// The colour comes from the theme, so it follows theme changes.
			SetDynamicResource(BackgroundColorProperty, "BackgroundGradientStart");

			_mainGrid = new Grid()
			{
				RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition() { Height = new GridLength(NavigationBarHeight, GridUnitType.Absolute) },
					new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },
				},
				RowSpacing = 0,
				ColumnSpacing = 0,
			};

			_navBackgroundView = new BoxView()
			{
			};
			_navBackgroundView.SetDynamicResource(VisualElement.BackgroundColorProperty, "BackgroundGradientStart");
			Grid.SetRow(_navBackgroundView, 0);
			_mainGrid.Children.Add(_navBackgroundView);

			_navigationBarGrid = new Grid();
			Grid.SetRow(_navigationBarGrid, 0);
			_mainGrid.Children.Add(_navigationBarGrid);

			_titleLabel = new Label()
			{
				FontAttributes = FontAttributes.Bold,
				FontSize = 18,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
			};
			_titleLabel.SetDynamicResource(Label.TextColorProperty, "OnBackgroundColor");

            Grid.SetRow(_titleLabel, 0);
			_mainGrid.Children.Add(_titleLabel);

			Content = _mainGrid;
		}


		protected override void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanging(propertyName);

			if (BaseContentProperty.PropertyName.Equals(propertyName))
			{
				if (BaseContent != null)
				{
					_mainGrid.Children.Remove(BaseContent);
				}
			}
		}

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

			if (BaseContentProperty.PropertyName.Equals(propertyName))
			{
				Grid.SetRow(BaseContent, _useLargeTitle ? 2 : 1);
				_mainGrid.Children.Add(BaseContent);
			}
			else if (TitleProperty.PropertyName.Equals(propertyName))
            {
				_titleLabel.Text = Title;
			}
		}

		bool _useLargeTitle = false;

		/// <summary>
		/// Settings style header: a small bar with the back button and actions, and the title below it in large type, left aligned.
		/// The normal header has the title centred in the bar.
		/// </summary>
		public bool UseLargeTitle
		{
			get { return _useLargeTitle; }
			set
			{
				if (_useLargeTitle == value)
				{
					return;
				}

				_useLargeTitle = value;
				ApplyTitleStyle();
			}
		}

		void ApplyTitleStyle()
		{
			if (_useLargeTitle)
			{
				_mainGrid.RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition() { Height = new GridLength(56, GridUnitType.Absolute) },
					new RowDefinition() { Height = GridLength.Auto },
					new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },
				};

				Grid.SetRow(_titleLabel, 1);
				_titleLabel.HorizontalOptions = LayoutOptions.Start;
				_titleLabel.VerticalOptions = LayoutOptions.Center;
				_titleLabel.FontAttributes = FontAttributes.None;
				_titleLabel.FontSize = 34;
				_titleLabel.Margin = new Thickness(20, 4, 20, 20);
			}
			else
			{
				_mainGrid.RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition() { Height = new GridLength(NavigationBarHeight, GridUnitType.Absolute) },
					new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) },
				};

				Grid.SetRow(_titleLabel, 0);
				_titleLabel.HorizontalOptions = LayoutOptions.Center;
				_titleLabel.VerticalOptions = LayoutOptions.Center;
				_titleLabel.FontAttributes = FontAttributes.Bold;
				_titleLabel.FontSize = 18;
				_titleLabel.Margin = new Thickness(0);
			}

			if (BaseContent != null)
			{
				Grid.SetRow(BaseContent, _useLargeTitle ? 2 : 1);
			}
		}

		void AddToolbarItems(IList newItems)
		{
			foreach (CustomToolbarItem customToolbarItem in newItems)
			{
				AddToolbarItem(customToolbarItem);
			}
		}

		void AddToolbarItem(CustomToolbarItem newItem)
		{
			if (newItem.Parent != null)
            {
				return;
            }

			if (newItem.Position == CustomToolbarItemPosition.Left)
            {
				SetupLeftNavStackLayout();

				if (_leftNavStackLayout.Children.Count == 0)
                {
					_leftNavStackLayout.Children.Add(newItem);
                }
				else
                {
					for (int i = 0; i < _leftNavStackLayout.Children.Count; ++i)
                    {
						if (_leftNavStackLayout.Children[i] is CustomToolbarItem customToolbarItem)
                        {
							if (newItem.Priority > customToolbarItem.Priority)
                            {
								_leftNavStackLayout.Children.Insert(i, newItem);
								break;
                            }
                        }
                    }

					if (newItem.Parent != null)
                    {
						_leftNavStackLayout.Children.Add(newItem);
                    }
                }
            }
			else if (newItem.Position == CustomToolbarItemPosition.Right)
            {
				SetupRightNavStackLayout();

				if (_rightNavStackLayout.Children.Count == 0)
                {
					_rightNavStackLayout.Children.Add(newItem);
                }
                else
                {
					for (int i = _rightNavStackLayout.Children.Count - 1; i >= 0; --i)
					{
						if (_rightNavStackLayout.Children[i] is CustomToolbarItem customToolbarItem)
						{
							if (newItem.Priority > customToolbarItem.Priority)
							{
								_rightNavStackLayout.Children.Insert(i + 1, newItem);
								break;
							}
						}
					}

					if (newItem.Parent != null)
					{
						_rightNavStackLayout.Children.Add(newItem);
					}
				}
			}
		}

		void RemoveToolbarItems(IList oldItems)
		{
			foreach (CustomToolbarItem customToolbarItem in oldItems)
			{
				RemoveToolbarItem(customToolbarItem);
			}
		}

		void RemoveToolbarItem(CustomToolbarItem oldItem)
		{
			if (oldItem.Parent is StackLayout stackLayout)
            {
				stackLayout.Children.Remove(oldItem);
            }
		}

		void RebuildToolbar()
		{
			_leftNavStackLayout?.Children.Clear();
			_rightNavStackLayout?.Children.Clear();

			AddToolbarItems(CustomToolbarItems);
		}

		private void SetupLeftNavStackLayout()
		{
			if (_leftNavStackLayout != null)
			{
				return;
			}

			_leftNavStackLayout = new StackLayout()
			{
				HorizontalOptions = LayoutOptions.Start,
				Spacing = 8,
				Margin = new Thickness(16, 0, 0, 0),
			};

			_navigationBarGrid.Children.Insert(0, _leftNavStackLayout);
		}

		private void SetupRightNavStackLayout()
		{
			if (_rightNavStackLayout != null)
			{
				return;
			}

			_rightNavStackLayout = new StackLayout()
			{
				HorizontalOptions = LayoutOptions.End,
				Spacing = 8,
				Margin = new Thickness(0, 0, 16, 0),
			};

			_navigationBarGrid.Children.Add(_rightNavStackLayout);
		}

		/// <summary>Puts a view in the middle of the navigation bar (where the title is).</summary>
		protected void SetCenterContent(View view)
		{
			view.HorizontalOptions = LayoutOptions.Center;
			view.VerticalOptions = LayoutOptions.Center;
			Grid.SetRow(view, 0);
			_mainGrid.Children.Add(view);
		}

		protected Label GetTitleLabel()
		{
			return _titleLabel;
		}

	}
}
