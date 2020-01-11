namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    using JetBrains.Annotations;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Семейство
    /// </summary>
    public class ExtFamily : VmBase
    {
        private readonly bool _onlyOneSymbolCanBeSelected;
        private bool? _checked;
        private bool _changeCheckStateProcess;

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        /// <param name="onlyOneSymbolCanBeSelected">Только один типоразмер может быть выбран</param>
        public ExtFamily(bool onlyOneSymbolCanBeSelected)
        {
            _onlyOneSymbolCanBeSelected = onlyOneSymbolCanBeSelected;
            FamilySymbols = new ObservableCollection<ExtFamilySymbol>();
            _checked = false;
        }

        /// <summary>
        /// Инициализация нового экземпляра <see cref="ExtFamily"/>
        /// </summary>
        /// <param name="family">Семейство Revit</param>
        /// <param name="onlyOneSymbolCanBeSelected">Только один типоразмер может быть выбран</param>
        public ExtFamily(Family family, bool onlyOneSymbolCanBeSelected)
            : this(onlyOneSymbolCanBeSelected)
        {
            Family = family;
            FillDataFromFamily();
        }

        /// <summary>
        /// Инициализация нового экземпляра <see cref="ExtFamily"/>
        /// </summary>
        /// <param name="extFamilyForSelection">Экземпляр <see cref="ExtFamilyForSelection"/></param>
        /// <param name="onlyOneSymbolCanBeSelected">Только один типоразмер может быть выбран</param>
        public ExtFamily(ExtFamilyForSelection extFamilyForSelection, bool onlyOneSymbolCanBeSelected)
            : this(onlyOneSymbolCanBeSelected)
        {
            Family = extFamilyForSelection.Family;
            FillDataFromFamily();
        }

        /// <summary>
        /// Родительская пара
        /// </summary>
        [CanBeNull]
        public ExtFamilyPair ParentFamilyPair { get; set; }

        /// <summary>Статус выбора в окне</summary>
        public bool? Checked
        {
            get => _checked;
            set
            {
                if (Equals(value, _checked))
                    return;
                if (value == null && FamilySymbols.All(s => s.Checked))
                    value = false;
                _checked = value;
                OnPropertyChanged();
                if (value != null && !_onlyOneSymbolCanBeSelected)
                    ChangeCheckedStateForFamilySymbols(value.Value);
            }
        }
        
        /// <summary>
        /// Revit's family
        /// </summary>
        public Family Family { get; }

        /// <summary>
        /// Family id
        /// </summary>
        public int FamilyId => Family.Id.IntegerValue;

        /// <summary>
        /// Имя семейства
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Идентификатор категории семейств
        /// </summary>
        public int CategoryId { get; private set; }

        /// <summary>
        /// Типоразмеры
        /// </summary>
        public ObservableCollection<ExtFamilySymbol> FamilySymbols { get; }
        
        private void FillDataFromFamily()
        {
            Name = Family.Name;
            CategoryId = Family.FamilyCategoryId.IntegerValue;
            foreach (var familySymbolId in Family.GetFamilySymbolIds())
            {
                if (Family.Document.GetElement(familySymbolId) is FamilySymbol familySymbol)
                {
                    var extFamilySymbol = new ExtFamilySymbol(this, familySymbol);
                    extFamilySymbol.PropertyChanged += SymbolOnPropertyChanged;
                    FamilySymbols.Add(extFamilySymbol);
                }
            }

            if (_onlyOneSymbolCanBeSelected && FamilySymbols.Any())
                FamilySymbols[0].Checked = true;
        }

        private void ChangeCheckedStateByFamilySymbols()
        {
            if (FamilySymbols.All(s => s.Checked))
                Checked = true;
            else if (FamilySymbols.All(s => !s.Checked))
                Checked = false;
            else 
                Checked = null;
        }

        private void ChangeCheckedStateForFamilySymbols(bool checkedState)
        {
            foreach (var extFamilySymbol in FamilySymbols)
            {
                extFamilySymbol.Checked = checkedState;
            }
        }
        
        private void SymbolOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ExtFamilySymbol familySymbol && e.PropertyName == "Checked")
            {
                if (!_onlyOneSymbolCanBeSelected)
                {
                    ChangeCheckedStateByFamilySymbols();
                }
                else
                {
                    if (_changeCheckStateProcess)
                        return;

                    _changeCheckStateProcess = true;
                    try
                    {
                        if (familySymbol.Checked)
                        {
                            foreach (var symbol in FamilySymbols)
                            {
                                if (symbol == familySymbol)
                                    continue;
                                symbol.Checked = false;
                            }
                        }
                        else
                        {
                            FamilySymbols[0].Checked = true;
                        }
                    }
                    catch
                    {
                        // ignore
                    }

                    _changeCheckStateProcess = false;
                }
            }
        }
    }
}