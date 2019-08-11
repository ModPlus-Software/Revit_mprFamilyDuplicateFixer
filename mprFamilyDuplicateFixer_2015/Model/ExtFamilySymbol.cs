﻿namespace mprFamilyDuplicateFixer.Model
{
    using Autodesk.Revit.DB;
    using ModPlusAPI.Mvvm;

    public class ExtFamilySymbol : VmBase
    {
        public ExtFamilySymbol(FamilySymbol familySymbol)
        {
            FamilySymbol = familySymbol;
            Name = familySymbol.Name;
            _checked = true;
        }

        /// <summary>
        /// Revit's FamilySymbol
        /// </summary>
        public FamilySymbol FamilySymbol { get; }

        /// <summary>
        /// Имя типоразмера
        /// </summary>
        public string Name { get; }

        private bool _checked;

        /// <summary>Статус выбора в окне</summary>
        public bool Checked
        {
            get => _checked;
            set
            {
                if (Equals(value, _checked)) return;
                _checked = value;
                OnPropertyChanged();
            }
        }
    }
}
