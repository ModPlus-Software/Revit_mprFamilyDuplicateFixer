namespace mprFamilyDuplicateFixer.Model.Base
{
    using System;
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    /// <summary>
    /// Типоразмер семейства
    /// </summary>
    public abstract class BaseExtFamilySymbol : VmBase
    {
        private bool _checked;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseExtFamilySymbol"/> class.
        /// </summary>
        /// <param name="familySymbol">Origin family symbol</param>
        protected BaseExtFamilySymbol(FamilySymbol familySymbol)
        {
            FamilySymbol = familySymbol;
            Name = familySymbol.Name;
        }

        /// <summary>
        /// On checked state changed
        /// </summary>
        public event EventHandler<bool> OnChecked;

        /// <summary>
        /// Revit FamilySymbol
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
    }
}
