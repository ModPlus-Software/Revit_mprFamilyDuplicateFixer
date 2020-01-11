namespace mprFamilyDuplicateFixer.Model
{
    using System;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    public class ExtFamilySymbol : VmBase
    {
        private bool _checked;
        private System.Windows.Visibility _checkStateVisibility = System.Windows.Visibility.Visible;

        public ExtFamilySymbol(ExtFamily extFamily, FamilySymbol familySymbol)
        {
            ParentFamily = extFamily;
            FamilySymbol = familySymbol;
            Name = familySymbol.Name;
            _checked = false;
        }

        /// <summary>
        /// On checked state changed
        /// </summary>
        public event EventHandler<bool> OnChecked;

        /// <summary>
        /// Ссылка на родительское семейство
        /// </summary>
        public ExtFamily ParentFamily { get; }

        /// <summary>
        /// Revit's FamilySymbol
        /// </summary>
        public FamilySymbol FamilySymbol { get; }

        /// <summary>
        /// Имя типоразмера
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Статус выбора в окне
        /// </summary>
        public bool Checked
        {
            get => _checked;
            set
            {
                if (Equals(value, _checked))
                    return;
                _checked = value;
                OnPropertyChanged();
                OnChecked?.Invoke(this, value);
            }
        }

        /// <summary>
        /// Видимость <see cref="Checked"/>
        /// </summary>
        public System.Windows.Visibility CheckStateVisibility
        {
            get => _checkStateVisibility;
            set
            {
                if (_checkStateVisibility == value)
                    return;
                _checkStateVisibility = value;
                OnPropertyChanged();
            }
        }
    }
}