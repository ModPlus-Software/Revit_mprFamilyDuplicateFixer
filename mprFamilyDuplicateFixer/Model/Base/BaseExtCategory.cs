namespace mprFamilyDuplicateFixer.Model.Base
{
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Категория Revit
    /// </summary>
    public abstract class BaseExtCategory : VmBase
    {
        private bool? _checked;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseExtCategory"/> class.
        /// </summary>
        /// <param name="category"><see cref="Category"/></param>
        protected BaseExtCategory(Category category)
        {
            Id = category.Id.IntegerValue;
            Name = category.Name;
            _checked = false;
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
        /// Статус выбора в окне
        /// </summary>
        public bool? Checked
        {
            get => _checked;
            set
            {
                if (Equals(value, _checked)) 
                    return;
                _checked = value;
                OnPropertyChanged();
            }
        }
    }
}
