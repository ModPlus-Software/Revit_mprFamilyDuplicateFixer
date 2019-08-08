namespace mprFamilyDuplicateFixer.Commands
{
    using System;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using ModPlusAPI;
    using View;
    using ViewModel;

    [Regeneration(RegenerationOption.Manual)]
    [Transaction(TransactionMode.Manual)]
    public class FamilyDuplicateFixerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // todo statistic
            ////Statistic.SendCommandStarting(new ModPlusConnector());
            
            MainWindow mainWindow = new MainWindow();
            MainViewModel mainViewModel = new MainViewModel(commandData.Application, mainWindow);
            mainWindow.DataContext = mainViewModel;
            mainWindow.ContentRendered += delegate
            {
                mainViewModel.ReadFamilies();
            };
            mainWindow.ShowDialog();

            return Result.Succeeded;
        }
    }
}
