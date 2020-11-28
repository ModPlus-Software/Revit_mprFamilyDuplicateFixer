namespace mprFamilyDuplicateFixer.Model.SymbolsFix
{
    using Autodesk.Revit.DB;
    using Base;

    /// <inheritdoc />
    public class ExtFamilySymbol : BaseExtFamilySymbol
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtFamilySymbol"/> class.
        /// </summary>
        /// <param name="parentFamily">Parent family</param>
        /// <param name="familySymbol">Origin family symbol</param>
        public ExtFamilySymbol(ExtFamily parentFamily, FamilySymbol familySymbol)
            : base(familySymbol)
        {
            ParentFamily = parentFamily;
        }

        /// <summary>
        /// Parent family
        /// </summary>
        public ExtFamily ParentFamily { get; }
    }
}
