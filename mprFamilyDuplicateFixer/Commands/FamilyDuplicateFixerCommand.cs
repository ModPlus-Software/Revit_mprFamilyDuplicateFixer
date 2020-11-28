namespace mprFamilyDuplicateFixer.Commands
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using View;
    using ViewModel;

    /// <summary>
    /// Класс команды mprFamilyDuplicateFixer
    /// </summary>
    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class FamilyDuplicateFixerCommand : IExternalCommand
    {
        /// <inheritdoc />
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
#if !DEBUG
            ModPlusAPI.Statistic.SendCommandStarting(ModPlusConnector.Instance);
#endif

            var mainWindow = new MainWindow();
            var mainViewModel = new MainContext(commandData.Application, mainWindow);
            mainWindow.DataContext = mainViewModel;
            mainWindow.ContentRendered += (sender, args) => mainViewModel.LoadDataFromModel();
            mainWindow.ShowDialog();

            return Result.Succeeded;
        }
    }
}