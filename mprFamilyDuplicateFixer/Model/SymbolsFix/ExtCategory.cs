namespace mprFamilyDuplicateFixer.Model.SymbolsFix
{
    using System.Collections.ObjectModel;
    using Autodesk.Revit.DB;
    using Base;

    /// <inheritdoc />
    public class ExtCategory : BaseExtCategory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtCategory"/> class.
        /// </summary>
        /// <param name="category"><see cref="Category"/></param>
        public ExtCategory(Category category) 
            : base(category)
        {
            Families = new ObservableCollection<ExtFamily>();
        }

        /// <summary>
        /// Семейства
        /// </summary>
        public ObservableCollection<ExtFamily> Families { get; }
    }
}
