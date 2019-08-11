namespace mprFamilyDuplicateFixer.Model
{
    using Autodesk.Revit.DB;

    /// <summary>
    /// Обертка на семейство для окна выбора
    /// </summary>
    public class ExtFamilyForSelection
    {
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
    }
}
