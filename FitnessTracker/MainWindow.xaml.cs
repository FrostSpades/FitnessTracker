using System.Windows;
using FitnessTracker.ViewModels;

namespace FitnessTracker
{
    /// <summary>Shell window – DataContext is supplied by DI.</summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}