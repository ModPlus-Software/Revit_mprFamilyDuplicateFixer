namespace mprFamilyDuplicateFixer.ViewModel
{
    using System;
    using System.Windows.Input;
    using System.Windows.Threading;
    using ModPlusAPI.Mvvm;
    using View;

    /// <summary>
    /// Базовый частичный контекст
    /// </summary>
    public abstract class BaseSubContext : VmBase
    {
        private readonly MainWindow _mainWindow;
        private bool _isEnableControls = true;
        private string _progressText;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseSubContext"/> class.
        /// </summary>
        /// <param name="mainWindow"><see cref="MainWindow"/></param>
        protected BaseSubContext(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        /// <summary>
        /// Доступность взаимодействия с элементами окна
        /// </summary>
        public bool IsEnableControls
        {
            get => _isEnableControls;
            set
            {
                if (Equals(value, _isEnableControls))
                    return;
                _isEnableControls = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Progress text
        /// </summary>
        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Можно ли перейти к выполнению
        /// </summary>
        public abstract bool CanExecute { get; }

        /// <summary>
        /// Выполнить
        /// </summary>
        public abstract ICommand ExecuteCommand { get; }

        /// <summary>
        /// Set progress message
        /// </summary>
        /// <param name="message">Message</param>
        protected void SetProgressMessage(string message)
        {
            var dispatcher = _mainWindow.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    ProgressText = message;
                }));
            }
            else
            {
                ProgressText = message;
            }
        }
    }
}
