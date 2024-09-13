using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Client
{
    public partial class ProgressWindow : Window
    {
        private readonly Dictionary<string, ProgressBar> progress_bars;

        public ProgressWindow(IEnumerable<string> file_names)
        {
            InitializeComponent();
            progress_bars = new();

            foreach (string file_name in file_names)
            {
                ProgressBar progress_bar = new ProgressBar
                {
                    Maximum = 100,
                    Value = 0,
                    Height = 20,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                TextBlock text_block = new TextBlock
                {
                    Text = file_name,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var stack_panel = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };
                stack_panel.Children.Add(text_block);
                stack_panel.Children.Add(progress_bar);

                ProgressPanel.Children.Add(stack_panel);

                progress_bars[file_name] = progress_bar;
            }
        }

        public void UpdateProgress(string file_name, double progress_percentage)
        {
            Dispatcher.Invoke(() =>
            {
                if (progress_bars.TryGetValue(file_name, out var progress_bar))
                {
                    progress_bar.Value = progress_percentage;
                }
            });
                
        }
    }
}
