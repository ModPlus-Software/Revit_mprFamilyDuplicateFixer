namespace mprFamilyDuplicateFixer.Commands
{
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using ModPlusAPI;
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
            Statistic.SendCommandStarting(new ModPlusConnector());

            var mainWindow = new MainWindow();
            var mainViewModel = new MainViewModel(commandData.Application, mainWindow);
            mainWindow.DataContext = mainViewModel;
            mainWindow.ContentRendered += (sender, args) => mainViewModel.ReadFamilies();
            mainWindow.ShowDialog();

            return Result.Succeeded;
        }
    }
}