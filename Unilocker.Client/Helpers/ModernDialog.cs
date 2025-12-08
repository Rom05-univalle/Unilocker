using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Unilocker.Client.Helpers;

public static class ModernDialog
{
    public enum DialogType
    {
        Information,
        Warning,
        Error,
        Question,
        Success
    }

    public static void Show(string message, string title = "UniLocker", DialogType type = DialogType.Information)
    {
        var dialog = CreateDialog(message, title, type, false);
        dialog.ShowDialog();
    }

    public static bool ShowConfirm(string message, string title = "Confirmar", DialogType type = DialogType.Question)
    {
        var dialog = CreateDialog(message, title, type, true);
        var result = dialog.ShowDialog();
        return result == true;
    }

    private static Window CreateDialog(string message, string title, DialogType type, bool isConfirm)
    {
        var window = new Window
        {
            Title = title,
            Width = 550,
            MinHeight = 250,
            MaxHeight = 500,
            SizeToContent = SizeToContent.Height,
            WindowStartupLocation = Application.Current.MainWindow != null 
                ? WindowStartupLocation.CenterOwner 
                : WindowStartupLocation.CenterScreen,
            ResizeMode = ResizeMode.NoResize,
            WindowStyle = WindowStyle.None,
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            BorderBrush = GetColorForType(type),
            BorderThickness = new Thickness(2)
        };

        // Solo establecer Owner si hay una ventana principal disponible
        if (Application.Current.MainWindow != null && Application.Current.MainWindow != window)
        {
            window.Owner = Application.Current.MainWindow;
        }

        var mainGrid = new Grid
        {
            Margin = new Thickness(0)
        };

        // Definir filas
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) }); // Header
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Content
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(60) }); // Buttons

        // Header con título e ícono
        var headerBorder = new Border
        {
            Background = GetColorForType(type),
            Padding = new Thickness(20, 0, 20, 0)
        };

        var headerStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        var iconText = new TextBlock
        {
            Text = GetIconForType(type),
            FontSize = 24,
            Foreground = Brushes.White,
            Margin = new Thickness(0, 0, 15, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center
        };

        headerStack.Children.Add(iconText);
        headerStack.Children.Add(titleText);
        headerBorder.Child = headerStack;
        Grid.SetRow(headerBorder, 0);

        // Content
        var contentBorder = new Border
        {
            Padding = new Thickness(30, 25, 30, 25),
            Background = Brushes.White
        };

        var messageText = new TextBlock
        {
            Text = message,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Top,
            Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
            LineHeight = 20,
            MaxWidth = 480
        };

        contentBorder.Child = messageText;
        Grid.SetRow(contentBorder, 1);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(0, 0, 20, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        if (isConfirm)
        {
            var btnYes = CreateButton("Sí", true);
            btnYes.Click += (s, e) => { window.DialogResult = true; window.Close(); };

            var btnNo = CreateButton("No", false);
            btnNo.Click += (s, e) => { window.DialogResult = false; window.Close(); };

            buttonPanel.Children.Add(btnNo);
            buttonPanel.Children.Add(btnYes);
        }
        else
        {
            var btnOk = CreateButton("Aceptar", true);
            btnOk.Click += (s, e) => { window.DialogResult = true; window.Close(); };

            buttonPanel.Children.Add(btnOk);
        }

        Grid.SetRow(buttonPanel, 2);

        mainGrid.Children.Add(headerBorder);
        mainGrid.Children.Add(contentBorder);
        mainGrid.Children.Add(buttonPanel);

        window.Content = mainGrid;

        return window;
    }

    private static Button CreateButton(string content, bool isPrimary)
    {
        var button = new Button
        {
            Content = content,
            Width = 100,
            Height = 35,
            Margin = new Thickness(5, 0, 0, 0),
            FontSize = 13,
            Cursor = System.Windows.Input.Cursors.Hand
        };

        if (isPrimary)
        {
            button.Background = new SolidColorBrush(Color.FromRgb(0, 123, 255));
            button.Foreground = Brushes.White;
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 123, 255));
        }
        else
        {
            button.Background = new SolidColorBrush(Color.FromRgb(108, 117, 125));
            button.Foreground = Brushes.White;
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        button.BorderThickness = new Thickness(1);

        // Efecto hover
        button.MouseEnter += (s, e) =>
        {
            if (isPrimary)
                button.Background = new SolidColorBrush(Color.FromRgb(0, 105, 217));
            else
                button.Background = new SolidColorBrush(Color.FromRgb(90, 98, 104));
        };

        button.MouseLeave += (s, e) =>
        {
            if (isPrimary)
                button.Background = new SolidColorBrush(Color.FromRgb(0, 123, 255));
            else
                button.Background = new SolidColorBrush(Color.FromRgb(108, 117, 125));
        };

        return button;
    }

    private static SolidColorBrush GetColorForType(DialogType type)
    {
        return type switch
        {
            DialogType.Success => new SolidColorBrush(Color.FromRgb(40, 167, 69)),
            DialogType.Information => new SolidColorBrush(Color.FromRgb(23, 162, 184)),
            DialogType.Warning => new SolidColorBrush(Color.FromRgb(255, 193, 7)),
            DialogType.Error => new SolidColorBrush(Color.FromRgb(220, 53, 69)),
            DialogType.Question => new SolidColorBrush(Color.FromRgb(108, 117, 125)),
            _ => new SolidColorBrush(Color.FromRgb(23, 162, 184))
        };
    }

    private static string GetIconForType(DialogType type)
    {
        return type switch
        {
            DialogType.Success => "✓",
            DialogType.Information => "ℹ",
            DialogType.Warning => "⚠",
            DialogType.Error => "✕",
            DialogType.Question => "?",
            _ => "ℹ"
        };
    }
}
