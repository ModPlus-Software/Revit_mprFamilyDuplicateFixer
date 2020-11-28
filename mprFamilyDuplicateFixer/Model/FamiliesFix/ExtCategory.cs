namespace mprFamilyDuplicateFixer.Model.FamiliesFix
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Base;

    /// <summary>
    /// Категория Revit, содержащая <see cref="ExtFamilyPair"/>
    /// </summary>
    public class ExtCategory : BaseExtCategory
    {
        private readonly ObservableCollection<ExtFamilyPair> _familyPairs;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtCategory"/> class.
        /// </summary>
        /// <param name="category"><see cref="Category"/></param>
        public ExtCategory(Category category) 
            : base(category)
        {
            _familyPairs = new ObservableCollection<ExtFamilyPair>();
            FamilyPairs = new ReadOnlyObservableCollection<ExtFamilyPair>(_familyPairs);
            PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Семейства категории
        /// </summary>
        public ReadOnlyObservableCollection<ExtFamilyPair> FamilyPairs { get; }
        
        /// <summary>
        /// Добавление пары семейств с подпиской на событие выбора
        /// </summary>
        /// <param name="extFamilyPair">Экземпляр пары <see cref="ExtFamilyPair"/></param>
        public void AddFamilyPair(ExtFamilyPair extFamilyPair)
        {
            extFamilyPair.SourceFamily.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Checked")
                    ChangeCheckedStateByFamilies();
            };
            _familyPairs.Add(extFamilyPair);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Checked))
            {
                if (Checked == null && FamilyPairs.All(s => s.SourceFamily.Checked != null && s.SourceFamily.Checked.Value))
                    Checked = false;
                if (Checked != null)
                    ChangeCheckedStateForFamilies(Checked.Value);
            }
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
