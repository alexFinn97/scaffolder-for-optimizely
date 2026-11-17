using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.PlatformUI;

namespace ScaffolderForOptimizely.UI;

public partial class ScaffoldDialog : DialogWindow
{
    internal ScaffoldDialog(ScaffoldDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) =>
        {
            NameBox.Focus();
            NameBox.SelectAll();
        };
    }

    private void OnAddClick(object sender, RoutedEventArgs e) => DialogResult = true;
}

/// <summary>Picks the option row template whose resource key matches <see cref="OptionViewModel.Kind"/>.</summary>
public sealed class OptionTemplateSelector : DataTemplateSelector
{
    public override DataTemplate? SelectTemplate(object item, DependencyObject container) =>
        item is OptionViewModel option && container is FrameworkElement element
            ? element.TryFindResource(option.Kind) as DataTemplate
            : base.SelectTemplate(item, container);
}
