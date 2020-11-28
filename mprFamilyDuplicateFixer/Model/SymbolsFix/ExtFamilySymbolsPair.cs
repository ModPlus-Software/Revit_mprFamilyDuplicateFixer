namespace mprFamilyDuplicateFixer.Model.SymbolsFix
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Пара типоразмеров
    /// </summary>
    public class ExtFamilySymbolsPair : VmBase
    {
        private ExtFamilySymbol _replacingFamilySymbol;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtFamilySymbolsPair"/> class.
        /// </summary>
        /// <param name="familySymbolToReplace">Типоразмер для замены</param>
        /// <param name="replacingFamilySymbols">Заменяющие типоразмеры</param>
        public ExtFamilySymbolsPair(ExtFamilySymbol familySymbolToReplace, List<ExtFamilySymbol> replacingFamilySymbols)
        {
            FamilySymbolToReplace = familySymbolToReplace;
            ReplacingFamilySymbols = replacingFamilySymbols;
        }

        /// <summary>
        /// Типоразмер для замены
        /// </summary>
        public ExtFamilySymbol FamilySymbolToReplace { get; }

        /// <summary>
        /// Заменяющий типоразмер
        /// </summary>
        public ExtFamilySymbol ReplacingFamilySymbol
        {
            get => _replacingFamilySymbol;
            set
            {
                if (_replacingFamilySymbol == value)
                    return;
                _replacingFamilySymbol = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Заменяющие типоразмеры
        /// </summary>
        public List<ExtFamilySymbol> ReplacingFamilySymbols { get; }

        /// <summary>
        /// Сбросить замещающий типоразмер
        /// </summary>
        public ICommand ResetReplacingFamilySymbol => new RelayCommandWithoutParameter(() => ReplacingFamilySymbol = null);
    }
}
