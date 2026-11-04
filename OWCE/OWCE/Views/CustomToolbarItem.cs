using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace OWCE.Views
{
	public enum CustomToolbarItemPosition
	{
		Left,
		Right,
	}

	public class CustomToolbarItem : ContentView
    {
		public static readonly BindableProperty PositionProperty = BindableProperty.Create(
			propertyName: nameof(Position),
			returnType: typeof(CustomToolbarItemPosition),
			declaringType: typeof(CustomToolbarItem),
			defaultValue: CustomToolbarItemPosition.Right);

		public CustomToolbarItemPosition Position
		{
			get { return (CustomToolbarItemPosition)GetValue(PositionProperty); }
			set { SetValue(PositionProperty, value); }
		}


		public static readonly BindableProperty PriorityProperty = BindableProperty.Create(
			propertyName: nameof(Priority),
			returnType: typeof(int),
			declaringType: typeof(CustomToolbarItem),
			defaultValue: 1);

		public int Priority
		{
			get { return (int)GetValue(PriorityProperty); }
			set { SetValue(PriorityProperty, value); }
		}


		public static readonly BindableProperty IconImageSourceProperty = BindableProperty.Create(
			propertyName: nameof(IconImageSource),
			returnType: typeof(ImageSource),
			declaringType: typeof(CustomToolbarItem));

		public ImageSource IconImageSource
		{
			get { return (ImageSource)GetValue(IconImageSourceProperty); }
			set { SetValue(IconImageSourceProperty, value); }
		}

        
        // Optional theme colours for the icon (names of theme colours, eg "OnMenuButtonColor"). When IconBackgroundKey is set the
        // icon is drawn on a circle of that colour. This is how the burger button follows the theme.
        public static readonly BindableProperty IconColorKeyProperty = BindableProperty.Create(
            propertyName: nameof(IconColorKey),
            returnType: typeof(string),
            declaringType: typeof(CustomToolbarItem));

        public string IconColorKey
        {
            get { return (string)GetValue(IconColorKeyProperty); }
            set { SetValue(IconColorKeyProperty, value); }
        }

        public static readonly BindableProperty IconBackgroundKeyProperty = BindableProperty.Create(
            propertyName: nameof(IconBackgroundKey),
            returnType: typeof(string),
            declaringType: typeof(CustomToolbarItem));

        public string IconBackgroundKey
        {
            get { return (string)GetValue(IconBackgroundKeyProperty); }
            set { SetValue(IconBackgroundKeyProperty, value); }
        }

        // A vector icon (see Icons) instead of an image, drawn with a theme brush such as "OnBackgroundBrush".
        public static readonly BindableProperty IconPathDataProperty = BindableProperty.Create(
            propertyName: nameof(IconPathData),
            returnType: typeof(string),
            declaringType: typeof(CustomToolbarItem));

        public string IconPathData
        {
            get { return (string)GetValue(IconPathDataProperty); }
            set { SetValue(IconPathDataProperty, value); }
        }

        public static readonly BindableProperty IconPathBrushKeyProperty = BindableProperty.Create(
            propertyName: nameof(IconPathBrushKey),
            returnType: typeof(string),
            declaringType: typeof(CustomToolbarItem),
            defaultValue: "OnBackgroundBrush");

        public string IconPathBrushKey
        {
            get { return (string)GetValue(IconPathBrushKeyProperty); }
            set { SetValue(IconPathBrushKeyProperty, value); }
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create(
            propertyName: nameof(Text),
            returnType: typeof(string),
            declaringType: typeof(CustomToolbarItem));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }



        public static readonly BindableProperty CommandProperty = BindableProperty.Create(
            propertyName: nameof(Command),
            returnType: typeof(ICommand),
            declaringType: typeof(CustomToolbarItem));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }


        Label _label;
        Image _image;

        public CustomToolbarItem()
		{
			MinimumWidthRequest = 30;
			MinimumHeightRequest = 30;
            VerticalOptions = LayoutOptions.Fill;

            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer()
            {
                Command = new Command(() =>
                {
                    if (this.Command != null && this.Command.CanExecute(null))
                    {
                        // Simulate click visual
                        this.Opacity = 0.6;
                        this.FadeToAsync(1.0);

                        this.Command.Execute(null);
                    }
                }),
            };

            GestureRecognizers.Add(tapGestureRecognizer);
		}

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (IsEnabledProperty.PropertyName.Equals(propertyName))
            {
                UpdateIsEnabled();
            }
            else if (IconImageSourceProperty.PropertyName.Equals(propertyName)
                || IconColorKeyProperty.PropertyName.Equals(propertyName)
                || IconBackgroundKeyProperty.PropertyName.Equals(propertyName))
            {
                if (IconImageSource != null)
                {
                    UpdateIconImage();
                }
            }
            else if (IconPathDataProperty.PropertyName.Equals(propertyName) || IconPathBrushKeyProperty.PropertyName.Equals(propertyName))
            {
                UpdateIconPath();
            }
            else if (TextProperty.PropertyName.Equals(propertyName))
            {
                UpdateText();
            }
        }

        void UpdateIsEnabled()
        {
            Opacity = IsEnabled ? 0.5 : 1.0;
        }

        void UpdateIconImage()
        {
            // If label exists lets nuke it.
            if (_label != null)
            {
                _label = null;
            }

            if (_image == null)
            {
                _image = new Image()
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
            }

            _image.Source = IconImageSource;

            // Tint the icon with a theme colour (the tint follows theme changes).
            _image.Behaviors.Clear();
            if (string.IsNullOrEmpty(IconColorKey) == false)
            {
                _image.Behaviors.Add(new ThemedIconTint() { ThemeColorKey = IconColorKey });
            }

            if (string.IsNullOrEmpty(IconBackgroundKey))
            {
                Content = _image;
            }
            else
            {
                // The icon sits on a circle in a theme colour, like the burger button.
                var circle = new Border()
                {
                    WidthRequest = 40,
                    HeightRequest = 40,
                    StrokeThickness = 0,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.Ellipse(),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    Content = _image,
                };
                circle.SetDynamicResource(VisualElement.BackgroundColorProperty, IconBackgroundKey);
                Content = circle;
            }
        }

        void UpdateIconPath()
        {
            if (string.IsNullOrEmpty(IconPathData))
            {
                return;
            }

            _label = null;
            _image = null;

            // A bigger touch area than the 24 unit icon.
            Content = new Grid()
            {
                WidthRequest = 48,
                HeightRequest = 48,
                Children = { Icons.Create(IconPathData, IconPathBrushKey, 24) },
            };
        }

        void UpdateText()
        {
            // If image exists lets nuke it.
            if (_image != null)
            {
                _image = null;
            }

            if (_label == null)
            {
                _label = new Label()
                {
                    FontSize = 18,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };
                // Actions on the right ("Save") are drawn in the accent colour, like text buttons.
                if (Position == CustomToolbarItemPosition.Right)
                {
                    _label.SetDynamicResource(Label.TextColorProperty, "AccentColor");
                    _label.FontAttributes = FontAttributes.Bold;
                    _label.FontSize = 16;
                }
                else
                {
                    _label.SetDynamicResource(Label.TextColorProperty, "OnBackgroundColor");
                }
                Content = _label;
            }
            _label.Text = Text;
        }
    }
}
