namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Категория с семействами для окна выбора
    /// </summary>
    public class ExtCategoryForSelection : VmBase
    {
        private bool? _isChecked = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtCategoryForSelection"/> class.
        /// </summary>
        /// <param name="category"><see cref="Category"/></param>
        public ExtCategoryForSelection(Category category)
        {
            Families = new ObservableCollection<ExtFamilyForSelection>();
            Name = category.Name;
            Id = category.Id.IntegerValue;
            Category = category;
        }

        /// <summary>
        /// Имя категории
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Id категории
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Категория Revit
        /// </summary>
        public Category Category { get; }

        /// <summary>
        /// Список семейств
        /// </summary>
        public ObservableCollection<ExtFamilyForSelection> Families { get; }

        /// <summary>
        /// Категория отмечена галочкой
        /// </summary>
        public bool? IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value)
                    return;
                _isChecked = value;
                OnPropertyChanged();
                ChangeCheckStatusForFamilies();
            }
        }

        /// <summary>
        /// Изменить состояние свойства <see cref="IsChecked"/> без использования сеттера
        /// </summary>
        public void ChangeCheckedStatusWithoutSetter()
        {
            if (Families.All(f => f.IsChecked))
                _isChecked = true;
            else if (Families.All(f => !f.IsChecked))
                _isChecked = false;
            else
                _isChecked = null;

            OnPropertyChanged(nameof(IsChecked));
        }

        private void ChangeCheckStatusForFamilies()
        {
            if (IsChecked == null)
                return;
            
            foreach (var family in Families)
            {
                family.SetIsCheckedWithoutSetter(IsChecked.Value);
            }
        }
    }
}
