namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Семейство
    /// </summary>
    public class ExtFamily : VmBase
    {
        private bool? _checked;

        /// <summary>
        /// Базовый конструктор
        /// </summary>
        public ExtFamily()
        {
            FamilySymbols = new ObservableCollection<ExtFamilySymbol>();
            _checked = true;
        }

        /// <summary>
        /// Инициализация нового экземпляра <see cref="ExtFamily"/>
        /// </summary>
        /// <param name="family">Семейство Revit</param>
        public ExtFamily(Family family)
            : this()
        {
            Family = family;
            FillDataFromFamily();
        }

        /// <summary>
        /// Инициализация нового экземпляра <see cref="ExtFamily"/>
        /// </summary>
        /// <param name="extFamilyForSelection">Экземпляр <see cref="ExtFamilyForSelection"/></param>
        public ExtFamily(ExtFamilyForSelection extFamilyForSelection)
            : this()
        {
            Family = extFamilyForSelection.Family;
            FillDataFromFamily();
        }

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
                if (value != null)
                    ChangeCheckedStateForFamilySymbols(value.Value);
            }
        }

        /// <summary>
        /// Revit's family
        /// </summary>
        public Family Family { get; }

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
                    var extFamilySymbol = new ExtFamilySymbol(familySymbol);
                    extFamilySymbol.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                    {
                        if (args.PropertyName == "Checked")
                            ChangeCheckedStateByFamilySymbols();
                    };
                    FamilySymbols.Add(extFamilySymbol);
                }
            }
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
    }
}