namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Обертка на семейство для окна выбора
    /// </summary>
    public class ExtFamilyForSelection : VmBase
    {
        private List<FamilySymbol> _familySymbols;
        private bool _isChecked;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="parentCategory">Родительская категория</param>
        /// <param name="family">Семейство Revit</param>
        public ExtFamilyForSelection(ExtCategoryForSelection parentCategory, Family family)
        {
            ParentCategory = parentCategory;
            Family = family;
            Name = family.Name;
        }

        /// <summary>
        /// Родительская категория
        /// </summary>
        public ExtCategoryForSelection ParentCategory { get; }

        /// <summary>
        /// Семейство Revit
        /// </summary>
        public Family Family { get; }

        /// <summary>
        /// Имя семейства
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Семейство отмечено галкой
        /// </summary>
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                    return;
                _isChecked = value;
                OnPropertyChanged();
                ParentCategory.ChangeCheckedStatusWithoutSetter();
            }
        }

        /// <summary>
        /// Установить свойство <see cref="IsChecked"/> без использования сеттера
        /// </summary>
        /// <param name="isChecked">Is checked</param>
        public void SetIsCheckedWithoutSetter(bool isChecked)
        {
            _isChecked = isChecked;
            OnPropertyChanged(nameof(IsChecked));
        }

        /// <summary>
        /// Проверяет, имеет ли данное семейство типы, схожие с проверяемым семейством
        /// </summary>
        /// <param name="extFamily">Проверяемое семейство</param>
        public bool IsSimilarTo(ExtFamilyForSelection extFamily)
        {
            foreach (var familySymbol in GetFamilySymbols())
            {
                var parameterNames = new HashSet<string>(familySymbol.Parameters.Cast<Parameter>().Select(p => p.Definition.Name));

                foreach (var checkedFamilySymbol in extFamily.GetFamilySymbols())
                {
                    if (familySymbol.IsSimilarType(checkedFamilySymbol.Id))
                    {
                        var checkedParameterNames = checkedFamilySymbol.Parameters.Cast<Parameter>().Select(p => p.Definition.Name);
                        return parameterNames.SetEquals(checkedParameterNames);
                    }
                }
            }

            return false;
        }

        public List<FamilySymbol> GetFamilySymbols()
        {
            if (_familySymbols == null)
            {
                _familySymbols = new List<FamilySymbol>();
                foreach (var familySymbolId in Family.GetFamilySymbolIds())
                {
                    _familySymbols.Add((FamilySymbol)Family.Document.GetElement(familySymbolId));
                }
            }

            return _familySymbols;
        }
    }
}
