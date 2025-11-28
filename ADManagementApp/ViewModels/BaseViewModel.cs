using CommunityToolkit.Mvvm.ComponentModel;

namespace ADManagementApp.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels using CommunityToolkit.Mvvm
    /// Provides automatic property change notification
    /// </summary>
    public abstract class BaseViewModel : ObservableObject
    {
        private bool _isBusy;
        private string _busyMessage = string.Empty;

        /// <summary>
        /// Indicates if the ViewModel is performing a long-running operation
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        /// <summary>
        /// Message to display while busy
        /// </summary>
        public string BusyMessage
        {
            get => _busyMessage;
            set => SetProperty(ref _busyMessage, value);
        }

        /// <summary>
        /// Sets the busy state with a message
        /// </summary>
        protected void SetBusy(bool isBusy, string message = "")
        {
            IsBusy = isBusy;
            BusyMessage = message;
        }
    }
}