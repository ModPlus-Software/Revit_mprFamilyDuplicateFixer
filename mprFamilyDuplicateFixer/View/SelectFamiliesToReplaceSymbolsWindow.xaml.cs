namespace mprFamilyDuplicateFixer.View
{
    using System.Windows;

    /// <summary>
    /// Логика взаимодействия для SelectFamiliesToReplaceSymbolsWindow.xaml
    /// </summary>
    public partial class SelectFamiliesToReplaceSymbolsWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectFamiliesToReplaceSymbolsWindow"/> class.
        /// </summary>
        public SelectFamiliesToReplaceSymbolsWindow()
        {
            InitializeComponent();

            Title = ModPlusAPI.Language.GetItem("h16");
        }

        private void BtAccept_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
