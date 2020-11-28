namespace mprFamilyDuplicateFixer.ViewModel
{
    using Autodesk.Revit.UI;
    using View;

    /// <summary>
    /// Главный контекст
    /// </summary>
    public class MainContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainContext"/> class.
        /// </summary>
        /// <param name="uiApplication"><see cref="UIApplication"/></param>
        /// <param name="mainWindow">Ссылка на главное окно</param>
        public MainContext(UIApplication uiApplication, MainWindow mainWindow)
        {
            FamiliesFixContext = new FamiliesFixContext(uiApplication, mainWindow);
            SymbolsFixContext = new SymbolsFixContext(uiApplication, mainWindow);
        }

        /// <summary>
        /// Контекст "Исправление дубликатов семейств"
        /// </summary>
        public FamiliesFixContext FamiliesFixContext { get; }

        /// <summary>
        /// Контекст "Исправление дубликатов типоразмеров"
        /// </summary>
        public SymbolsFixContext SymbolsFixContext { get; }

        /// <summary>
        /// Загрузка данных из модели
        /// </summary>
        public void LoadDataFromModel()
        {
            FamiliesFixContext.ReadFamilies();
            SymbolsFixContext.ReadSymbols();
        }
    }
}
