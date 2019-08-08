namespace mprFamilyDuplicateFixer.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Model;
    using ModPlusAPI.Mvvm;
    using View;

    /// <summary>
    /// Главная модель представления
    /// </summary>
    public class MainViewModel : VmBase
    {
        private readonly UIApplication _uiApplication;
        private readonly MainWindow _mainWindow;

        public MainViewModel(UIApplication uiApplication, MainWindow mainWindow)
        {
            _uiApplication = uiApplication;
            _mainWindow = mainWindow;
            FamiliesByCategories = new ObservableCollection<ExtCategory>();
        }

        public void ReadFamilies()
        {
            var families = new FilteredElementCollector(_uiApplication.ActiveUIDocument.Document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategory != null)
                .ToList();

            Dictionary<int, Category> categories = families
                .Select(f => f.FamilyCategory)
                .GroupBy(f => f.Id.IntegerValue)
                .ToDictionary(g => g.Key, g => g.First());

            Dictionary<Family, List<Family>> duplicateFamilies = new Dictionary<Family, List<Family>>();

            var groupedByCategory = families.GroupBy(f => f.FamilyCategoryId.IntegerValue);
            foreach (IGrouping<int, Family> grouping in groupedByCategory)
            {
                var familiesOfCategory = grouping.ToList();
                familiesOfCategory.Sort((f1, f2) => string.Compare(f1.Name, f2.Name, StringComparison.Ordinal));
                for (var i = 0; i < familiesOfCategory.Count; i++)
                {
                    if (familiesOfCategory[i] == null)
                        continue;
                    Family currentFamily = familiesOfCategory[i];
                    var pair = new KeyValuePair<Family, List<Family>>(currentFamily, new List<Family>());
                    List<Family> duplicates = new List<Family>();
                    for (var j = i + 1; j < familiesOfCategory.Count; j++)
                    {
                        if (familiesOfCategory[j] == null)
                            continue;
                        Family checkedFamily = familiesOfCategory[j];
                        if (IsMatch(currentFamily, checkedFamily))
                        {
                            pair.Value.Add(checkedFamily);
                            familiesOfCategory[j] = null;
                        }
                    }

                    if (pair.Value.Any())
                        duplicateFamilies.Add(pair.Key, pair.Value);
                }
            }

            foreach (KeyValuePair<Family, List<Family>> pair in duplicateFamilies)
            {
                Category category = categories[pair.Key.FamilyCategoryId.IntegerValue];
                ExtCategory extCategory = FamiliesByCategories.FirstOrDefault(c => c.Id == pair.Key.FamilyCategoryId.IntegerValue);
                if (extCategory == null)
                {
                    extCategory = new ExtCategory(category);
                    FamiliesByCategories.Add(extCategory);
                }

                foreach (Family dFamily in pair.Value)
                {
                    ExtFamilyPair extFamilyPair = new ExtFamilyPair(
                        new ExtFamily(dFamily),
                        new ExtFamily(pair.Key));
                    extCategory.FamilyPairs.Add(extFamilyPair);
                }
            }
        }

        public ObservableCollection<ExtCategory> FamiliesByCategories { get; }

        #region Helpers

        private bool IsMatch(Family currentFamily, Family checkedFamily)
        {
            if (checkedFamily.Name.StartsWith(currentFamily.Name))
            {
                if (int.TryParse(checkedFamily.Name.Remove(0, currentFamily.Name.Length), out _))
                    return true;
            }

            return false;
        }

        #endregion
    }
}
