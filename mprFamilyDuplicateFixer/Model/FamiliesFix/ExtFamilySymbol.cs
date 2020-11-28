namespace mprFamilyDuplicateFixer.Model.FamiliesFix
{
    using Autodesk.Revit.DB;
    using Base;

    /// <inheritdoc />
    public class ExtFamilySymbol : BaseExtFamilySymbol
    {
        private bool _isVisibleCheckState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtFamilySymbol"/> class.
        /// </summary>
        /// <param name="extFamily">Parent family</param>
        /// <param name="familySymbol">Origin family symbol</param>
        public ExtFamilySymbol(ExtFamily extFamily, FamilySymbol familySymbol) 
            : base(familySymbol)
        {
            ParentFamily = extFamily;
        }

        /// <summary>
        /// Ссылка на родительское семейство
        /// </summary>
        public ExtFamily ParentFamily { get; }
        
        /// <summary>
        /// Видимость галочки выбора
        /// </summary>
        public bool IsVisibleCheckState
        {
            get => _isVisibleCheckState;
            set
            {
                if (_isVisibleCheckState == value)
                    return;
                _isVisibleCheckState = value;
                OnPropertyChanged();
            }
        }
    }
}