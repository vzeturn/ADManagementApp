namespace ADManagementApp.Services
{
    /// <summary>
    /// Service for managing navigation between views
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Gets the current view model
        /// </summary>
        object? CurrentViewModel { get; }

        /// <summary>
        /// Event fired when navigation occurs
        /// </summary>
        event System.EventHandler? NavigationChanged;

        /// <summary>
        /// Navigates to a specific view model
        /// </summary>
        void NavigateTo<TViewModel>() where TViewModel : ViewModels.BaseViewModel;

        /// <summary>
        /// Navigates to a specific view model instance
        /// </summary>
        void NavigateTo(object viewModel);
    }
}
