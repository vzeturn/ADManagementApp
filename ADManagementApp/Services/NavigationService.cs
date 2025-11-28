using System;
using Microsoft.Extensions.DependencyInjection;
using ADManagementApp.ViewModels;

namespace ADManagementApp.Services
{
    /// <summary>
    /// Implementation of navigation service
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;
        private object? _currentViewModel;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                NavigationChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler? NavigationChanged;

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
            CurrentViewModel = viewModel;
        }

        public void NavigateTo(object viewModel)
        {
            CurrentViewModel = viewModel;
        }
    }
}