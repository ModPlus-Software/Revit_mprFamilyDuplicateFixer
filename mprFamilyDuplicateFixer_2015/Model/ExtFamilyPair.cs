namespace mprFamilyDuplicateFixer.Model
{
    /// <summary>
    /// Пара семейств
    /// </summary>
    public class ExtFamilyPair
    {
        public ExtFamilyPair(ExtFamily sourceFamily, ExtFamily destinationFamily)
        {
            SourceFamily = sourceFamily;
            DestinationFamily = destinationFamily;
        }

        /// <summary>
        /// Семейство - источник (дубликат)
        /// </summary>
        public ExtFamily SourceFamily { get; }

        /// <summary>
        /// Семейство назначения
        /// </summary>
        public ExtFamily DestinationFamily { get; }
    }
}
