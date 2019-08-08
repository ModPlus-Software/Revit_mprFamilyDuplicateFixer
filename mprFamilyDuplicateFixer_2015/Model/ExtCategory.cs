namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.ObjectModel;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Категория Revit
    /// </summary>
    public class ExtCategory
    {
        public ExtCategory(Category category)
        {
            FamilyPairs = new ObservableCollection<ExtFamilyPair>();
            Id = category.Id.IntegerValue;
            Name = category.Name;
        }

        /// <summary>
        /// Category Id
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Имя категории
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Семейства категории
        /// </summary>
        public ObservableCollection<ExtFamilyPair> FamilyPairs { get; }
    }
}
