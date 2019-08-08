namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.ObjectModel;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Семейство
    /// </summary>
    public class ExtFamily
    {
        public ExtFamily(Family family)
        {
            FamilySymbols = new ObservableCollection<ExtFamilySymbol>();
            Name = family.Name;
            foreach (ElementId familySymbolId in family.GetFamilySymbolIds())
            {
                if (family.Document.GetElement(familySymbolId) is FamilySymbol familySymbol)
                {
                    FamilySymbols.Add(new ExtFamilySymbol(familySymbol));
                }
            }
        }

        /// <summary>
        /// Имя семейства
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Типоразмеры
        /// </summary>
        public ObservableCollection<ExtFamilySymbol> FamilySymbols { get; }
    }
}
