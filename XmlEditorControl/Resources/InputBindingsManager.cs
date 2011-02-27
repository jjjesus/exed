using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace TreeListControl.Resources
{
	public static class InputBindingsManager
	{
		#region Static Fields

		public static readonly DependencyProperty UpdatePropertySourceWhenEnterPressedProperty = DependencyProperty.
		    RegisterAttached(
		    "UpdatePropertySourceWhenEnterPressedProperty", typeof (DependencyProperty), typeof (InputBindingsManager),
		    new PropertyMetadata(null, OnUpdatePropertySourceWhenEnterPressedPropertyChanged));

		#endregion Static Fields

		#region Private Methods

		private static void DoUpdateSource(object source)
		{
			var property =
			    GetUpdatePropertySourceWhenEnterPressed(source as DependencyObject);

			if (property == null) return;

			var elt = source as UIElement;

			if (elt == null) return;

			var binding = BindingOperations.GetBindingExpression(elt, property);

			if (binding != null) binding.UpdateSource();
		}

		private static void HandlePreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter) return;
			DoUpdateSource(e.Source);
			e.Handled = true;
		}

		private static void OnUpdatePropertySourceWhenEnterPressedPropertyChanged(DependencyObject dp,
			DependencyPropertyChangedEventArgs e)
		{
			var element = dp as UIElement;

			if (element == null) return;

			if (e.OldValue != null) element.PreviewKeyDown -= HandlePreviewKeyDown;

			if (e.NewValue != null) element.PreviewKeyDown += HandlePreviewKeyDown;
		}

		#endregion Private Methods

		#region Public Methods

		public static DependencyProperty GetUpdatePropertySourceWhenEnterPressed(DependencyObject dp)
		{
			return (DependencyProperty) dp.GetValue(UpdatePropertySourceWhenEnterPressedProperty);
		}

		public static void SetUpdatePropertySourceWhenEnterPressed(DependencyObject dp, DependencyProperty value)
		{
			dp.SetValue(UpdatePropertySourceWhenEnterPressedProperty, value);
		}

		#endregion Public Methods
	}
}