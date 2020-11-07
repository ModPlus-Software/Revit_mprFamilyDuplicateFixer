namespace mprFamilyDuplicateFixer.View
{
    /// <summary>
    /// Main Window
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetFunctionLocalName(ModPlusConnector.Instance);
        }
    }
}
