namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.ObjectModel;
    using Autodesk.Revit.DB;

    /// <summary>
    /// Категория с семействами для окна выбора
    /// </summary>
    public class ExtCategoryForSelection
    {
        public ExtCategoryForSelection(Category category)
        {
            Families = new ObservableCollection<ExtFamilyForSelection>();
            Name = category.Name;
        }

        /// <summary>
        /// Имя категории
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Список семейств
        /// </summary>
        public ObservableCollection<ExtFamilyForSelection> Families { get; }
    }
}
