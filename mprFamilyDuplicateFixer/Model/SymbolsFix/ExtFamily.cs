namespace mprFamilyDuplicateFixer.Model.SymbolsFix
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Обертка на семейство для замены типоразмеров
    /// </summary>
    public class ExtFamily : VmBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtFamily"/> class.
        /// </summary>
        /// <param name="family"><see cref="Autodesk.Revit.DB.Family"/></param>
        public ExtFamily(Family family)
        {
            FamilySymbolPairs = new ObservableCollection<ExtFamilySymbolsPair>();
            Family = family;
            FillDataFromFamily();
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
        /// Типоразмеры семейства
        /// </summary>
        public ObservableCollection<ExtFamilySymbolsPair> FamilySymbolPairs { get; }

        private void FillDataFromFamily()
        {
            Name = Family.Name;
            CategoryId = Family.FamilyCategoryId.IntegerValue;

            var familySymbols = new List<ExtFamilySymbol>();
            foreach (var familySymbolId in Family.GetFamilySymbolIds())
            {
                if (Family.Document.GetElement(familySymbolId) is FamilySymbol familySymbol)
                {
                    var extFamilySymbol = new ExtFamilySymbol(this, familySymbol);
                    familySymbols.Add(extFamilySymbol);
                }
            }

            for (var i = 0; i < familySymbols.Count; i++)
            {
                var extFamilySymbol = familySymbols[i];
                var replacingFamilySymbols = new List<ExtFamilySymbol>();
                for (var j = 0; j < familySymbols.Count; j++)
                {
                    if (i == j)
                        continue;
                    replacingFamilySymbols.Add(familySymbols[j]);
                }

                FamilySymbolPairs.Add(new ExtFamilySymbolsPair(extFamilySymbol, replacingFamilySymbols));
            }
        }
    }
}
