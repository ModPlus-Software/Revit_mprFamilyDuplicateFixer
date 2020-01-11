namespace mprFamilyDuplicateFixer.ViewModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;
    using System.Windows.Threading;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Events;
    using Autodesk.Revit.UI;
    using Model;
    using ModPlusAPI;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Windows;
    using ModPlusStyle.Controls.Dialogs;
    using View;

    /// <summary>
    /// Главная модель представления
    /// </summary>
    public class MainViewModel : VmBase
    {
        private readonly string _langItem;
        private readonly MainWindow _mainWindow;
        private readonly UIApplication _uiApplication;
        private Dictionary<int, Category> _categories;
        private bool _changeFamilyInstancesSymbol;
        private bool _copyFamilySymbolParameters;
        private bool _copyFamilySymbols;
        private bool _copyFamilyInstanceParameters;
        private bool _deleteDuplicateFamilies;
        private bool _setFamilyInstanceSymbolFromSourceFamily;
        private bool _isEnableControls = true;
        private string _progressText;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="uiApplication">UIApplication</param>
        /// <param name="mainWindow">Ссылка на главное окно</param>
        public MainViewModel(UIApplication uiApplication, MainWindow mainWindow)
        {
            _langItem = ModPlusConnector.Instance.Name;
            _uiApplication = uiApplication;
            _mainWindow = mainWindow;
            FamiliesByCategories = new ObservableCollection<ExtCategory>();

            if (bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyFamilySymbols)), out var b))
                CopyFamilySymbols = b;
            if (bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyFamilySymbolParameters)), out b))
                CopyFamilySymbolParameters = b;
            if (bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(ChangeFamilyInstancesSymbol)), out b))
                ChangeFamilyInstancesSymbol = b;
            if (ChangeFamilyInstancesSymbol)
            {
                if (bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(SetFamilyInstanceSymbolFromSourceFamily)), out b))
                    SetFamilyInstanceSymbolFromSourceFamily = b;
                if (bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(CopyFamilyInstanceParameters)), out b))
                    CopyFamilyInstanceParameters = b;
                if (bool.TryParse(UserConfigFile.GetValue(_langItem, nameof(DeleteDuplicateFamilies)), out b))
                    DeleteDuplicateFamilies = b;
            }
        }

        /// <summary>
        /// Семейства по категориям
        /// </summary>
        public ObservableCollection<ExtCategory> FamiliesByCategories { get; }

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
        /// Копировать типоразмеры
        /// </summary>
        public bool CopyFamilySymbols
        {
            get => _copyFamilySymbols;
            set
            {
                if (Equals(value, _copyFamilySymbols))
                    return;
                _copyFamilySymbols = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanExecute));
                UserConfigFile.SetValue(_langItem, nameof(CopyFamilySymbols), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать значения параметров
        /// </summary>
        public bool CopyFamilySymbolParameters
        {
            get => _copyFamilySymbolParameters;
            set
            {
                if (Equals(value, _copyFamilySymbolParameters))
                    return;
                _copyFamilySymbolParameters = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanExecute));
                UserConfigFile.SetValue(_langItem, nameof(CopyFamilySymbolParameters), value.ToString(), true);
            }
        }

        /// <summary>
        /// Заменить типоразмер у экземпляров семейств
        /// </summary>
        public bool ChangeFamilyInstancesSymbol
        {
            get => _changeFamilyInstancesSymbol;
            set
            {
                if (Equals(value, _changeFamilyInstancesSymbol))
                    return;
                _changeFamilyInstancesSymbol = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanExecute));
                if (!value)
                {
                    SetFamilyInstanceSymbolFromSourceFamily = false;
                    DeleteDuplicateFamilies = false;
                    CopyFamilyInstanceParameters = false;
                }

                UserConfigFile.SetValue(_langItem, nameof(ChangeFamilyInstancesSymbol), value.ToString(), true);
            }
        }

        /// <summary>
        /// Установить типоразмер из основного семейства
        /// </summary>
        public bool SetFamilyInstanceSymbolFromSourceFamily
        {
            get => _setFamilyInstanceSymbolFromSourceFamily;
            set
            {
                if (_setFamilyInstanceSymbolFromSourceFamily == value)
                    return;
                _setFamilyInstanceSymbolFromSourceFamily = value;
                OnPropertyChanged();
                
                SetCheckVisibilityForDestinationFamilies(value);

                UserConfigFile.SetValue(_langItem, nameof(SetFamilyInstanceSymbolFromSourceFamily), value.ToString(), true);
            }
        }

        /// <summary>
        /// Копировать параметры экземпляра
        /// </summary>
        public bool CopyFamilyInstanceParameters
        {
            get => _copyFamilyInstanceParameters;
            set
            {
                if (Equals(value, _copyFamilyInstanceParameters))
                    return;
                _copyFamilyInstanceParameters = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(CopyFamilyInstanceParameters), value.ToString(), true);
            }
        }

        /// <summary>
        /// Удалить дубликаты
        /// </summary>
        public bool DeleteDuplicateFamilies
        {
            get => _deleteDuplicateFamilies;
            set
            {
                if (Equals(value, _deleteDuplicateFamilies))
                    return;
                _deleteDuplicateFamilies = value;
                OnPropertyChanged();
                UserConfigFile.SetValue(_langItem, nameof(DeleteDuplicateFamilies), value.ToString(), true);
            }
        }

        /// <summary>
        /// Можно ли перейти к выполнению
        /// </summary>
        public bool CanExecute => CopyFamilySymbols || CopyFamilySymbolParameters || ChangeFamilyInstancesSymbol;

        /// <summary>
        /// Команда добавления новой пары семейств
        /// </summary>
        public ICommand AddNewFamilyPairCommand => new RelayCommandWithoutParameter(AddNewFamilyPair);

        /// <summary>
        /// Выполнить
        /// </summary>
        public ICommand ExecuteCommand => new RelayCommandWithoutParameter(Execute);

        #region Methods

        /// <summary>
        /// Получение дубликатов семейств из текущего документа по стандартным правилам
        /// </summary>
        public void ReadFamilies()
        {
            var families = new FilteredElementCollector(_uiApplication.ActiveUIDocument.Document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategory != null && !f.IsInPlace)
                .ToList();

            _categories = families
                .Select(f => f.FamilyCategory)
                .GroupBy(f => f.Id.IntegerValue)
                .ToDictionary(g => g.Key, g => g.First());

            var duplicateFamilies = new Dictionary<Family, List<Family>>();

            var groupedByCategory = families.GroupBy(f => f.FamilyCategoryId.IntegerValue);
            foreach (var grouping in groupedByCategory)
            {
                var familiesOfCategory = grouping.ToList();
                familiesOfCategory.Sort((f1, f2) => string.Compare(f1.Name, f2.Name, StringComparison.Ordinal));
                for (var i = 0; i < familiesOfCategory.Count; i++)
                {
                    if (familiesOfCategory[i] == null)
                        continue;
                    var currentFamily = familiesOfCategory[i];
                    var pair = new KeyValuePair<Family, List<Family>>(currentFamily, new List<Family>());
                    for (var j = i + 1; j < familiesOfCategory.Count; j++)
                    {
                        if (familiesOfCategory[j] == null)
                            continue;
                        var checkedFamily = familiesOfCategory[j];
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

            foreach (var pair in duplicateFamilies)
            {
                var category = _categories[pair.Key.FamilyCategoryId.IntegerValue];
                var extCategory = FamiliesByCategories.FirstOrDefault(c => c.Id == pair.Key.FamilyCategoryId.IntegerValue);
                if (extCategory == null)
                {
                    extCategory = new ExtCategory(category);
                    FamiliesByCategories.Add(extCategory);
                }

                foreach (var dFamily in pair.Value)
                {
                    var sourceFamily = new ExtFamily(dFamily, false);
                    var destinationFamily = new ExtFamily(pair.Key, true);

                    // если в семействе назначения есть такие типоразмеры, то по-умолчанию в семействе источнике снимаю галочку
                    foreach (var sourceFamilyFamilySymbol in sourceFamily.FamilySymbols)
                    {
                        if (destinationFamily.FamilySymbols.Any(fs => fs.Name == sourceFamilyFamilySymbol.Name))
                            sourceFamilyFamilySymbol.Checked = false;
                    }

                    var extFamilyPair = new ExtFamilyPair(sourceFamily, destinationFamily);
                    foreach (var extFamilySymbol in sourceFamily.FamilySymbols)
                    {
                        extFamilySymbol.OnChecked += FamilySymbolOnChecked;
                    }

                    extCategory.AddFamilyPair(extFamilyPair);
                }
            }

            SetCheckVisibilityForDestinationFamilies(SetFamilyInstanceSymbolFromSourceFamily);
        }

        private void AddNewFamilyPair()
        {
            var skipFamilies = FamiliesByCategories.SelectMany(c => c.FamilyPairs.Select(p => p.SourceFamily.Name)).ToList();
            var selectFamilyPairWindow = new SelectFamilyPairWindow(_uiApplication, skipFamilies);
            if (selectFamilyPairWindow.ShowDialog() == true)
            {
                var selectedPair = selectFamilyPairWindow.SelectedPair;
                if (selectedPair != null)
                {
                    foreach (var extFamilySymbol in selectedPair.SourceFamily.FamilySymbols)
                    {
                        extFamilySymbol.OnChecked += FamilySymbolOnChecked;
                    }

                    var category = FamiliesByCategories.FirstOrDefault(c => c.Id == selectedPair.CategoryId);
                    if (category == null)
                    {
                        category = new ExtCategory(_categories[selectedPair.CategoryId]);
                        FamiliesByCategories.Add(category);
                    }

                    category.AddFamilyPair(selectedPair);
                }
            }
        }

        /// <summary>
        /// Обработка события выбор типоразмера - чтобы нельзя было выбрать два одинаковых типоразмера двух
        /// разных дубликатов, которые будут копироваться в одно главное семейство
        /// </summary>
        private void FamilySymbolOnChecked(object sender, bool e)
        {
            if (sender is ExtFamilySymbol extFamilySymbol && e)
            {
                foreach (var extFamily in FamiliesByCategories
                    .SelectMany(c => c.FamilyPairs)
                    .Where(p => p.SourceFamily != extFamilySymbol.ParentFamily &&
                                p.DestinationFamily.Name == extFamilySymbol.ParentFamily.ParentFamilyPair?.DestinationFamily.Name)
                    .Select(p => p.SourceFamily))
                {
                    foreach (var familySymbol in extFamily.FamilySymbols)
                    {
                        if (familySymbol.Name == extFamilySymbol.Name &&
                            familySymbol.Checked)
                            familySymbol.Checked = false;
                    }
                }
            }
        }

        private bool IsMatch(Family currentFamily, Family checkedFamily)
        {
            if (checkedFamily.Name.StartsWith(currentFamily.Name))
            {
                if (int.TryParse(checkedFamily.Name.Remove(0, currentFamily.Name.Length), out _))
                {
                    var currentFamilySymbolId = currentFamily.GetFamilySymbolIds().FirstOrDefault();
                    var checkedFamilySymbolId = checkedFamily.GetFamilySymbolIds().FirstOrDefault();
                    if (currentFamilySymbolId != null && checkedFamilySymbolId != null)
                    {
                        var currentFamilySymbol = (FamilySymbol)currentFamily.Document.GetElement(currentFamilySymbolId);
                        var checkedFamilySymbol = (FamilySymbol)checkedFamily.Document.GetElement(checkedFamilySymbolId);
                        if (currentFamilySymbol.IsSimilarType(checkedFamilySymbolId))
                        {
                            return new HashSet<string>(currentFamilySymbol.Parameters.Cast<Parameter>().Select(p => p.Definition.Name))
                                .SetEquals(checkedFamilySymbol.Parameters.Cast<Parameter>().Select(p => p.Definition.Name));
                        }
                    }
                }
            }

            return false;
        }

        private async void Execute()
        {
            try
            {
                if (!IsAnyChecked())
                {
                    await _mainWindow.ShowMessageAsync(Language.GetItem(_langItem, "m1"), string.Empty);
                    return;
                }

                IsEnableControls = false;

                var trName = Language.GetFunctionLocalName(ModPlusConnector.Instance.Name, ModPlusConnector.Instance.LName);
                if (string.IsNullOrEmpty(trName))
                    trName = "mprFamilyDuplicateFixer";

                var errors = new List<string>();

                using (var transactionGroup = new TransactionGroup(_uiApplication.ActiveUIDocument.Document, trName))
                {
                    transactionGroup.Start();

                    if (CopyFamilySymbols)
                        errors.AddRange(CopySymbols());

                    if (CopyFamilySymbolParameters)
                        errors.AddRange(CopySymbolsParameters());

                    if (ChangeFamilyInstancesSymbol)
                        errors.AddRange(ChangeInstances());

                    if (DeleteDuplicateFamilies)
                        errors.AddRange(DeleteDuplicates());

                    transactionGroup.Assimilate();
                }

                if (errors.Any())
                {
                    ModPlusAPI.IO.String.ShowTextWithNotepad(string.Join(Environment.NewLine, errors), trName);
                }

                _mainWindow.Close();
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }
        }

        private IEnumerable<string> CopySymbols()
        {
            var errors = new List<string>();
            var doc = _uiApplication.ActiveUIDocument.Document;
            foreach (var extCategory in GetCheckedCategories())
            {
                foreach (IGrouping<int, ExtFamilyPair> extFamilyPairGroup in GetCheckedFamilies(extCategory).GroupBy(p => p.DestinationFamily.FamilyId))
                {
                    if (!extFamilyPairGroup.Any())
                        continue;

                    try
                    {
                        var familyDoc = doc.EditFamily(extFamilyPairGroup.First().DestinationFamily.Family);
                        if (familyDoc == null)
                            continue;
                    }
                    catch (Exception exception)
                    {
                        ExceptionBox.Show(exception);
                    }
                }

                foreach (var extFamilyPair in GetCheckedFamilies(extCategory))
                {
                    try
                    {
                        var familyDoc = doc.EditFamily(extFamilyPair.DestinationFamily.Family);
                        if (familyDoc == null) 
                            continue;

                        var familyManager = familyDoc.FamilyManager;
                        using (var tr = new Transaction(familyDoc, "Create types"))
                        {
                            tr.Start();

                            foreach (var extFamilySymbol in GetSourceCheckedSymbols(extFamilyPair))
                            {
                                if (extFamilyPair.DestinationFamily.FamilySymbols.Any(s => s.Name == extFamilySymbol.Name))
                                    continue;

                                // Creating type {0} for family {1}
                                SetProgressMessage(string.Format(
                                    Language.GetItem(_langItem, "m2"),
                                    extFamilySymbol.Name,
                                    extFamilyPair.DestinationFamily.Name));

                                var hasSameTypeName = false;
                                foreach (FamilyType familyManagerType in familyManager.Types)
                                {
                                    if (familyManagerType.Name == extFamilySymbol.Name)
                                    {
                                        hasSameTypeName = true;
                                        break;
                                    }
                                }

                                if (hasSameTypeName)
                                    continue;

                                var t = familyManager.NewType(extFamilySymbol.Name);
                                if (t == null)
                                {
                                    // Type "{0}" was not created in the family "{1}"
                                    errors.Add(string.Format(
                                        Language.GetItem(_langItem, "e1"),
                                        extFamilySymbol.Name,
                                        extFamilyPair.DestinationFamily.Name));
                                }
                            }

                            tr.Commit();
                        }

                        // This overload is necessary for reloading an edited family
                        // back into the source document from which it was extracted
                        var f = familyDoc.LoadFamily(doc, new LoadOpts());
                        if (f != null)
                        {
                            extFamilyPair.DestinationFamily = new ExtFamily(f, true);
                            using (var tr = new Transaction(doc, "Activate symbols"))
                            {
                                tr.Start();

                                foreach (var s in f.GetFamilySymbolIds())
                                {
                                    if (doc.GetElement(s) is FamilySymbol symbol && !symbol.IsActive)
                                        symbol.Activate();
                                }

                                tr.Commit();
                            }
                        }
                        else
                        {
                            // The family "{0}" could not be loaded into the document
                            errors.Add(string.Format(Language.GetItem(_langItem, "e2"), extFamilyPair.DestinationFamily.Name));
                        }
                    }
                    catch (Exception exception)
                    {
                        ExceptionBox.Show(exception);
                    }
                }
            }

            return errors;
        }

        private IEnumerable<string> CopySymbolsParameters()
        {
            var errors = new List<string>();
            var doc = _uiApplication.ActiveUIDocument.Document;
            _uiApplication.Application.FailuresProcessing += ApplicationOnFailuresProcessing;

            foreach (var extCategory in GetCheckedCategories())
            {
                foreach (var extFamilyPair in GetCheckedFamilies(extCategory))
                {
                    try
                    {
                        var sourceCheckedSymbols = GetSourceCheckedSymbols(extFamilyPair).ToList();
                        if (sourceCheckedSymbols.Any())
                        {
                            using (var tr = new Transaction(doc, "Copy Parameters"))
                            {
                                tr.Start();

                                var destFamilySymbols = extFamilyPair.DestinationFamily.FamilySymbols.Select(s => s.FamilySymbol).ToList();
                                foreach (var extFamilySymbol in sourceCheckedSymbols)
                                {
                                    var destFamilySymbol = destFamilySymbols.FirstOrDefault(s => s.Name == extFamilySymbol.Name);
                                    if (destFamilySymbol != null)
                                    {
                                        // Copying parameter values ​​of frame type "{0}" from the family "{1}" to the family "{2}"
                                        SetProgressMessage(string.Format(
                                            Language.GetItem(_langItem, "m3"),
                                            extFamilySymbol.Name,
                                            extFamilyPair.SourceFamily.Name,
                                            extFamilyPair.DestinationFamily.Name));

                                        foreach (Parameter sourceParameter in extFamilySymbol.FamilySymbol.Parameters)
                                        {
                                            if (sourceParameter.IsReadOnly)
                                                continue;

                                            var destParameter = destFamilySymbol.LookupParameter(sourceParameter.Definition.Name);

                                            if (destParameter != null)
                                            {
                                                CopyParameterValue(sourceParameter, destParameter);
                                            }
                                            else
                                            {
                                                // The parameter "{0}" was not found in the type "{1}" of the family "{2}"
                                                errors.Add(string.Format(
                                                    Language.GetItem(_langItem, "e3"),
                                                    sourceParameter.Definition.Name,
                                                    extFamilyPair.SourceFamily.Name,
                                                    extFamilyPair.DestinationFamily.Name));
                                            }
                                        }
                                    }
                                }

                                tr.Commit();
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        ExceptionBox.Show(exception);
                    }
                }
            }

            _uiApplication.Application.FailuresProcessing += ApplicationOnFailuresProcessing;
            return errors;
        }

        private IEnumerable<string> ChangeInstances()
        {
            var errors = new List<string>();
            var doc = _uiApplication.ActiveUIDocument.Document;
            var familyInstances = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .ToList();

            var groupedFamilyInstances = familyInstances
                .GroupBy(f => f.Symbol.Family.Name).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var extCategory in GetCheckedCategories())
            {
                foreach (ExtFamilyPair extFamilyPair in GetCheckedFamilies(extCategory))
                {
                    try
                    {
                        if (!groupedFamilyInstances.ContainsKey(extFamilyPair.SourceFamily.Name))
                            continue;

                        foreach (var familyInstance in groupedFamilyInstances[extFamilyPair.SourceFamily.Name])
                        {
                            var familySymbol = SetFamilyInstanceSymbolFromSourceFamily 
                                ? extFamilyPair.DestinationFamily.FamilySymbols.FirstOrDefault(s => s.Checked)
                                : extFamilyPair.DestinationFamily.FamilySymbols.FirstOrDefault(s => s.Name == familyInstance.Symbol.Name);

                            if (familySymbol != null)
                            {
                                List<ParameterDataHolder> parameterDataHolder = null;
                                if (CopyFamilyInstanceParameters)
                                {
                                    parameterDataHolder = new List<ParameterDataHolder>();
                                    foreach (Parameter parameter in familyInstance.Parameters)
                                    {
                                        if (!ParameterDataHolder.IsAllowableParameter(parameter))
                                            continue;
                                        
                                        var dataHolder = new ParameterDataHolder(parameter);
                                        Debug.Print($"Name {dataHolder.Name}, DoubleValue {dataHolder.DoubleValue}");
                                        parameterDataHolder.Add(dataHolder);
                                    }
                                }

                                // Replacing the type for an instance of the family "{0}" {1}
                                SetProgressMessage(string.Format(
                                    Language.GetItem(_langItem, "m4"),
                                    familyInstance.Name,
                                    $"[{familyInstance.Id.IntegerValue}]"));

                                using (var changeSymbol = new Transaction(doc, "Change Symbol Assignment"))
                                {
                                    changeSymbol.Start();
                                    familyInstance.Symbol = familySymbol.FamilySymbol;
                                    changeSymbol.Commit();
                                }

                                if (parameterDataHolder != null && parameterDataHolder.Any())
                                {
                                    using (var changeSymbol = new Transaction(doc, "Copy instance parameter"))
                                    {
                                        changeSymbol.Start();

                                        try
                                        {
                                            foreach (Parameter parameter in familyInstance.Parameters)
                                            {
                                                if (!ParameterDataHolder.IsAllowableParameter(parameter))
                                                    continue;

                                                parameterDataHolder.FirstOrDefault(p => p.Name == parameter.Definition.Name)?.SetTo(parameter);
                                            }
                                        }
                                        catch (Exception exception)
                                        {
                                            ExceptionBox.Show(exception);
                                        }

                                        changeSymbol.Commit();
                                    }
                                }
                            }
                            else
                            {
                                // Type "{0}" not found in family "{1}"
                                errors.Add(string.Format(
                                    Language.GetItem(_langItem, "e4"),
                                    familyInstance.Symbol.Name,
                                    extFamilyPair.DestinationFamily.Name));
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        ExceptionBox.Show(exception);
                    }
                }
            }

            return errors;
        }

        private IEnumerable<string> DeleteDuplicates()
        {
            var errors = new List<string>();
            var doc = _uiApplication.ActiveUIDocument.Document;
            var familyInstances = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .ToList();

            var groupedFamilyInstances = familyInstances
                .GroupBy(f => f.Symbol.Family.Name).ToDictionary(g => g.Key, g => g.ToList());

            var idsToDelete = new List<ElementId>();
            foreach (var extCategory in GetCheckedCategories())
            {
                foreach (var extFamilyPair in GetCheckedFamilies(extCategory))
                {
                    if (groupedFamilyInstances.ContainsKey(extFamilyPair.SourceFamily.Name))
                    {
                        // Failed to delete family "{0}" because instances of this family are present in the model
                        errors.Add(string.Format(Language.GetItem(_langItem, "e5"), extFamilyPair.SourceFamily.Name));
                    }
                    else
                    {
                        idsToDelete.Add(extFamilyPair.SourceFamily.Family.Id);
                    }
                }
            }

            if (!idsToDelete.Any())
                return errors;

            try
            {
                using (var transaction = new Transaction(doc, "Delete"))
                {
                    transaction.Start();
                    doc.Delete(idsToDelete);
                    transaction.Commit();
                }
            }
            catch (Exception exception)
            {
                ExceptionBox.Show(exception);
            }

            return errors;
        }

        private static void CopyParameterValue(Parameter sourceParameter, Parameter destParameter)
        {
            switch (sourceParameter.StorageType)
            {
                case StorageType.Double:
                    destParameter.Set(sourceParameter.AsDouble());
                    break;
                case StorageType.Integer:
                    destParameter.Set(sourceParameter.AsInteger());
                    break;
                case StorageType.String:
                    destParameter.Set(sourceParameter.AsString());
                    break;
                case StorageType.ElementId:
                    destParameter.Set(sourceParameter.AsElementId());
                    break;
            }
        }

        private void ApplicationOnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            e.GetFailuresAccessor().DeleteAllWarnings();
        }

        private static IEnumerable<ExtFamilyPair> GetCheckedFamilies(ExtCategory extCategory)
        {
            return extCategory.FamilyPairs.Where(p => p.SourceFamily.Checked == null || p.SourceFamily.Checked.Value);
        }

        private IEnumerable<ExtCategory> GetCheckedCategories()
        {
            return FamiliesByCategories.Where(c => c.Checked == null || c.Checked.Value);
        }

        private static IEnumerable<ExtFamilySymbol> GetSourceCheckedSymbols(ExtFamilyPair extFamilyPair)
        {
            return extFamilyPair.SourceFamily.FamilySymbols.Where(s => s.Checked);
        }

        private void SetCheckVisibilityForDestinationFamilies(bool isVisible)
        {
            foreach (var category in FamiliesByCategories)
            {
                foreach (var familyPair in category.FamilyPairs)
                {
                    foreach (var familySymbol in familyPair.DestinationFamily.FamilySymbols)
                    {
                        familySymbol.CheckStateVisibility = isVisible 
                            ? System.Windows.Visibility.Visible 
                            : System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }

        private bool IsAnyChecked()
        {
            List<bool> checkedSymbols = FamiliesByCategories
                .SelectMany(c => c.FamilyPairs
                    .SelectMany(p => p.SourceFamily.FamilySymbols
                        .Select(s => s.Checked)))
                .ToList();
            return checkedSymbols.Any(b => b);
        }

        private void SetProgressMessage(string msg)
        {
            var dispatcher = _mainWindow.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                {
                    ProgressText = msg;
                }));
            }
            else
            {
                ProgressText = msg;
            }
        }

        #endregion

        /// <summary>
        /// Опции загрузки семейств в документ
        /// </summary>
        internal class LoadOpts : IFamilyLoadOptions
        {
            /// <inheritdoc/>
            public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
            {
                overwriteParameterValues = true;
                return true;
            }

            /// <inheritdoc />
            public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues)
            {
                source = FamilySource.Family;
                overwriteParameterValues = true;
                return true;
            }
        }

        /// <summary>
        /// Вспомогательный объект хранения значений параметров
        /// </summary>
        internal class ParameterDataHolder
        {
            public ParameterDataHolder(Parameter parameter)
            {
                Name = parameter.Definition.Name;
                StorageType = parameter.StorageType;
                switch (StorageType)
                {
                    case StorageType.Double:
                        DoubleValue = parameter.AsDouble();
                        break;
                    case StorageType.Integer:
                        IntegerValue = parameter.AsInteger();
                        break;
                    case StorageType.String:
                        StringValue = parameter.AsString() ?? string.Empty;
                        break;
                    case StorageType.ElementId:
                        ElementIdValue = parameter.AsElementId();
                        break;
                }
            }

            public string Name { get; }

            public StorageType StorageType { get; }

            public double DoubleValue { get; }

            public int IntegerValue { get; }

            public string StringValue { get; }

            public ElementId ElementIdValue { get; }

            /// <summary>
            /// Является ли параметр допустимым для обработки копирования
            /// </summary>
            /// <param name="parameter">Проверяемый параметр</param>
            public static bool IsAllowableParameter(Parameter parameter)
            {
                if (parameter.IsReadOnly ||
                    parameter.StorageType == StorageType.None)
                    return false;
                if (parameter.Definition is InternalDefinition internalDefinition)
                {
                    if (internalDefinition.BuiltInParameter == BuiltInParameter.ELEM_TYPE_PARAM ||
                        internalDefinition.BuiltInParameter == BuiltInParameter.ELEM_FAMILY_PARAM ||
                        internalDefinition.BuiltInParameter == BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM)
                        return false;
                }

                return true;
            }

            public void SetTo(Parameter parameter)
            {
                switch (StorageType)
                {
                    case StorageType.Double:
                        if (Math.Abs(parameter.AsDouble() - DoubleValue) < 0.0001)
                            return;
                        parameter.Set(DoubleValue);
                        break;
                    case StorageType.Integer:
                        if (parameter.AsInteger() == IntegerValue)
                            return;
                        parameter.Set(IntegerValue);
                        break;
                    case StorageType.String:
                        if (parameter.AsString() == StringValue)
                            return;
                        parameter.Set(StringValue);
                        break;
                    case StorageType.ElementId:
                        if (parameter.AsElementId() == ElementIdValue)
                            return;
                        parameter.Set(ElementIdValue);
                        break;
                }
            }
        }
    }
}