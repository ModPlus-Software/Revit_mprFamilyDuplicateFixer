namespace mprFamilyDuplicateFixer.Model
{
    using Autodesk.Revit.DB;

    public class ExtFamilySymbol
    {
        public ExtFamilySymbol(FamilySymbol familySymbol)
        {
            Name = familySymbol.Name;
        }

        public string Name { get; }
    }
}
