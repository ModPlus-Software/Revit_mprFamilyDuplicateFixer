namespace mprFamilyDuplicateFixer.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Model;
    using Model.SymbolsFix;
    using ModPlusAPI;
    using ModPlusAPI.Enums;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Services;
    using ModPlusAPI.Windows;
    using View;

    /// <summary>
    /// Контекст "Исправление дубликатов типоразмеров"
    /// </summary>
    public class SymbolsFixContext : BaseSubContext
    {
        private readonly UIApplication _uiApplication;
        private readonly MainWindow _mainWindow;
        private Dictionary<int, Category> _categories;
        private bool _removeReplacedFamilySymbols;

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolsFixContext"/> class.
        /// </summary>
        /// <param name="uiApplication"><see cref="UIApplication"/></param>
        /// <param name="mainWindow">Ссылка на главное окно</param>
        public SymbolsFixContext(UIApplication uiApplication, MainWindow mainWindow)
            : base(mainWindow)
        {
            _uiApplication = uiApplication;
            _mainWindow = mainWindow;
            Categories = new ObservableCollection<ExtCategory>();

            if (bool.TryParse(UserConfigFile.GetValue(ModPlusConnector.Instance.Name, nameof(RemoveReplacedFamilySymbols)), out var b))
                RemoveReplacedFamilySymbols = b;
        }

        /// <summary>
        /// Категории семейств
        /// </summary>
        public ObservableCollection<ExtCategory> Categories { get; }

        /// <inheritdoc/>
        public override bool CanExecute => true;

        /// <inheritdoc/>
        public override ICommand ExecuteCommand => new RelayCommandWithoutParameter(Execute);

        /// <summary>
        /// Удалить замененные типоразмеры из семейства
        /// </summary>
        public bool RemoveReplacedFamilySymbols
        {
            get => _removeReplacedFamilySymbols;
            set
            {
                if (_removeReplacedFamilySymbols == value)
                    return;
                _removeReplacedFamilySymbols = value;
                OnPropertyChanged();

                UserConfigFile.SetValue(
                    ModPlusConnector.Instance.Name, nameof(RemoveReplacedFamilySymbols), value.ToString(), true);
            }
        }

        /// <summary>
        /// Добавить семейства в список
        /// </summary>
        public ICommand AddFamiliesCommand => new RelayCommandWithoutParameter(AddFamilies);

        /// <summary>
        /// Загрузка типоразмеров по стандартным правилам
        /// </summary>
        public void ReadSymbols()
        {
            var families = new FilteredElementCollector(_uiApplication.ActiveUIDocument.Document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategory != null)
                .ToList();

            _categories = families
                .Select(f => f.FamilyCategory)
                .GroupBy(f => f.Id.IntegerValue)
                .ToDictionary(g => g.Key, g => g.First());

            var groupedByCategory = families.GroupBy(f => f.FamilyCategoryId.IntegerValue);
            foreach (var grouping in groupedByCategory)
            {
                var familiesOfCategory = grouping.ToList();
                familiesOfCategory.Sort((f1, f2) => string.Compare(f1.Name, f2.Name, StringComparison.Ordinal));

                var category = _categories[grouping.Key];
                var extCategory = new ExtCategory(category);

                foreach (var family in familiesOfCategory.Where(IsMatch))
                {
                    var extFamily = new ExtFamily(family);
                    SuggestReplacement(extFamily);
                    extCategory.Families.Add(extFamily);
                }

                if (extCategory.Families.Any())
                    Categories.Add(extCategory);
            }
        }

        private void AddFamilies()
        {
            try
            {
                var families = new FilteredElementCollector(_uiApplication.ActiveUIDocument.Document)
                    .OfClass(typeof(Family))
                    .Cast<Family>()
                    .Where(f => f.FamilyCategory != null)
                    .ToList();

                var groupedByCategory = families.GroupBy(f => f.FamilyCategoryId.IntegerValue);
                var addedIds = Categories.SelectMany(c => c.Families.Select(f => f.FamilyId)).ToList();

                var categoriesForSelection = new List<ExtCategoryForSelection>();
                foreach (var grouping in groupedByCategory)
                {
                    var familiesOfCategory = grouping.ToList();
                    familiesOfCategory.Sort((f1, f2) => string.Compare(f1.Name, f2.Name, StringComparison.Ordinal));

                    var category = _categories[grouping.Key];
                    var categoryForSelection = new ExtCategoryForSelection(category);

                    foreach (var family in familiesOfCategory.Where(f => !addedIds.Contains(f.Id.IntegerValue)))
                    {
                        if (family.GetFamilySymbolIds().Count > 1)
                            categoryForSelection.Families.Add(new ExtFamilyForSelection(categoryForSelection, family));
                    }

                    if (categoryForSelection.Families.Any())
                        categoriesForSelection.Add(categoryForSelection);
                }

                var win = new SelectFamiliesToReplaceSymbolsWindow
                {
                    TvAllFamilies = { ItemsSource = categoriesForSelection }
                };

                if (win.ShowDialog() != true)
                    return;

                foreach (var categoryForSelection in categoriesForSelection.Where(c => c.IsChecked != false))
                {
                    var category = Categories.FirstOrDefault(c => c.Id == categoryForSelection.Id);
                    if (category == null)
                    {
                        category = new ExtCategory(categoryForSelection.Category);
                        foreach (var familyForSelection in categoryForSelection.Families.Where(f => f.IsChecked))
                        {
                            var family = new ExtFamily(familyForSelection.Family);
                            SuggestReplacement(family);
                            category.Families.Add(family);
                        }

                        if (category.Families.Any())
                            Categories.Add(category);
                    }
                    else
                    {
                        foreach (var familyForSelection in categoryForSelection.Families.Where(f => f.IsChecked))
                        {
                            category.Families.Add(new ExtFamily(familyForSelection.Family));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private void Execute()
        {
            try
            {
                IsEnableControls = false;

                var trName = Language.GetItem("h15");
                if (string.IsNullOrEmpty(trName))
                    trName = "mprFamilyDuplicateFixer";

                var resultService = new ResultService();

                using (var tr = new Transaction(_uiApplication.ActiveUIDocument.Document, trName))
                {
                    tr.Start();

                    foreach (var category in Categories)
                    {
                        foreach (var family in category.Families)
                        {
                            ReplaceSymbols(family, resultService);
                            if (RemoveReplacedFamilySymbols)
                                RemoveReplacedSymbols(family, resultService);
                        }
                    }

                    tr.Commit();
                }

                _mainWindow.Close();
                resultService.ShowByType();
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private void ReplaceSymbols(ExtFamily family, ResultService resultService)
        {
            var doc = _uiApplication.ActiveUIDocument.Document;
            var familyInstances = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(i => i.Symbol.Family.Id.IntegerValue == family.FamilyId)
                .ToList();

            foreach (var familyInstance in familyInstances)
            {
                foreach (var familySymbolsPair in family.FamilySymbolPairs)
                {
                    try
                    {
                        if (familyInstance.Symbol.Name == familySymbolsPair.FamilySymbolToReplace.Name &&
                            familySymbolsPair.ReplacingFamilySymbol != null)
                        {
                            // Замена типоразмера"{0}" на типоразмер "{1}" в семействе "{2}"
                            SetProgressMessage(string.Format(
                                Language.GetItem("m5"),
                                familySymbolsPair.FamilySymbolToReplace.Name,
                                familySymbolsPair.ReplacingFamilySymbol.Name,
                                family.Name));

                            familyInstance.Symbol = familySymbolsPair.ReplacingFamilySymbol.FamilySymbol;

                            // Типоразмер "{0}" заменен на типоразмер "{1}"
                            resultService.Add(
                                string.Format(
                                    Language.GetItem("m6"),
                                    familySymbolsPair.FamilySymbolToReplace.Name,
                                    familySymbolsPair.ReplacingFamilySymbol.Name),
                                familyInstance.Id.IntegerValue.ToString());
                        }
                    }
                    catch (Exception exception)
                    {
                        // Ошибка при замене типоразмера в семействе "{0}": {1}
                        resultService.Add(
                            string.Format(Language.GetItem("e6"), family.Name, exception.Message),
                            familyInstance.Id.IntegerValue.ToString(),
                            ResultItemType.Error);
                    }
                }
            }
        }

        private void RemoveReplacedSymbols(ExtFamily family, ResultService resultService)
        {
            foreach (var familySymbolPair in family.FamilySymbolPairs.Where(s => s.ReplacingFamilySymbol != null))
            {
                try
                {
                    // Удаление типоразмера "{0}" в семействе "{1}"
                    SetProgressMessage(string.Format(
                        Language.GetItem("m7"),
                        familySymbolPair.FamilySymbolToReplace.Name,
                        family.Name));

                    _uiApplication.ActiveUIDocument.Document.Delete(
                        familySymbolPair.FamilySymbolToReplace.FamilySymbol.Id);

                    // Типоразмер "{0}" удален в семействе "{1}"
                    resultService.Add(
                        string.Format(
                            Language.GetItem("m8"),
                            familySymbolPair.FamilySymbolToReplace.Name,
                            family.Name),
                        family.FamilyId.ToString());
                }
                catch (Exception exception)
                {
                    // Ошибка удаления типоразмера "{0}" в семействе "{1}": {2}
                    resultService.Add(
                        string.Format(
                            Language.GetItem("e7"),
                            familySymbolPair.FamilySymbolToReplace.Name,
                            family.Name,
                            exception.Message),
                        null,
                        ResultItemType.Error);
                }
            }
        }

        private bool IsMatch(Family family)
        {
            var familySymbols = family.GetFamilySymbolIds()
                .Select(s => family.Document.GetElement(s))
                .OfType<FamilySymbol>()
                .ToList();
            for (var i = 0; i < familySymbols.Count; i++)
            {
                for (var j = 0; j < familySymbols.Count; j++)
                {
                    if (i == j)
                        continue;
                    var firstName = familySymbols[i].Name;
                    var secondName = familySymbols[j].Name;
                    if (firstName.Length > secondName.Length)
                    {
                        if (firstName.StartsWith(secondName) &&
                            char.IsDigit(firstName.ToCharArray().LastOrDefault()))
                            return true;
                    }
                    else
                    {
                        if (secondName.StartsWith(firstName) &&
                            char.IsDigit(secondName.ToCharArray().LastOrDefault()))
                            return true;
                    }
                }
            }

            return false;
        }

        private void SuggestReplacement(ExtFamily family)
        {
            foreach (var familySymbolPair in family.FamilySymbolPairs)
            {
                if (char.IsDigit(familySymbolPair.FamilySymbolToReplace.Name.ToCharArray().Last()))
                {
                    familySymbolPair.ReplacingFamilySymbol = familySymbolPair.ReplacingFamilySymbols.FirstOrDefault(s =>
                        familySymbolPair.FamilySymbolToReplace.Name.StartsWith(s.Name));
                }
            }
        }
    }
}
