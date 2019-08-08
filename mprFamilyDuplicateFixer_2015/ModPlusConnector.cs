namespace mprFamilyDuplicateFixer
{
    using System;
    using System.Collections.Generic;
    using ModPlusAPI.Interfaces;

    public class ModPlusConnector : IModPlusFunctionInterface
    {
        public SupportedProduct SupportedProduct => SupportedProduct.Revit;

        public string Name => "mprFamilyDuplicateFixer";

#if R2015
        public string AvailProductExternalVersion => "2015";
#elif R2016
        public string AvailProductExternalVersion => "2016";
#elif R2017
        public string AvailProductExternalVersion => "2017";
#elif R2018
        public string AvailProductExternalVersion => "2018";
#elif R2019
        public string AvailProductExternalVersion => "2019";
#elif R2020
        public string AvailProductExternalVersion => "2020";
#endif

        public string FullClassName => throw new NotImplementedException();

        public string AppFullClassName => throw new NotImplementedException();

        public Guid AddInId => throw new NotImplementedException();

        public string LName => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public string Author => throw new NotImplementedException();

        public string Price => throw new NotImplementedException();

        public bool CanAddToRibbon => throw new NotImplementedException();

        public string FullDescription => throw new NotImplementedException();

        public string ToolTipHelpImage => throw new NotImplementedException();

        public List<string> SubFunctionsNames => throw new NotImplementedException();

        public List<string> SubFunctionsLames => throw new NotImplementedException();

        public List<string> SubDescriptions => throw new NotImplementedException();

        public List<string> SubFullDescriptions => throw new NotImplementedException();

        public List<string> SubHelpImages => throw new NotImplementedException();

        public List<string> SubClassNames => throw new NotImplementedException();
    }
}
