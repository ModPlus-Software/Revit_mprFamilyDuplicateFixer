namespace mprFamilyDuplicateFixer.Model
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Категория Revit
    /// </summary>
    public class ExtCategory : VmBase
    {
        private bool? _checked;

        public ExtCategory(Category category)
        {
            FamilyPairs = new ObservableCollection<ExtFamilyPair>();
            Id = category.Id.IntegerValue;
            Name = category.Name;
            _checked = true;
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
        
        /// <summary>Статус выбора в окне</summary>
        public bool? Checked
        {
            get => _checked;
            set
            {
                if (Equals(value, _checked)) 
                    return;
                _checked = value;
                if (value == null && FamilyPairs.All(s => s.SourceFamily.Checked != null && s.SourceFamily.Checked.Value))
                    value = false;
                OnPropertyChanged();
                if (value != null)
                    ChangeCheckedStateForFamilies(value.Value);
            }
        }

        /// <summary>
        /// Добавление пары семейств с подпиской на событие выбора
        /// </summary>
        /// <param name="extFamilyPair"></param>
        public void AddFamilyPair(ExtFamilyPair extFamilyPair)
        {
            extFamilyPair.SourceFamily.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Checked")
                    ChangeCheckedStateByFamilies();
            };
            FamilyPairs.Add(extFamilyPair);
        }

        private void ChangeCheckedStateByFamilies()
        {
            if (FamilyPairs.All(f => f.SourceFamily.Checked != null && f.SourceFamily.Checked.Value))
                Checked = true;
            else if (FamilyPairs.All(f => f.SourceFamily.Checked != null && !f.SourceFamily.Checked.Value))
                Checked = false;
            else 
                Checked = null;
        }

        private void ChangeCheckedStateForFamilies(bool checkedState)
        {
            foreach (var extFamilyPair in FamilyPairs)
            {
                extFamilyPair.SourceFamily.Checked = checkedState;
            }
        }
    }
}
