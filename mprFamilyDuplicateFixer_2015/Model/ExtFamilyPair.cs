namespace mprFamilyDuplicateFixer.Model
{
    /// <summary>
    /// Пара семейств
    /// </summary>
    public class ExtFamilyPair
    {
        public ExtFamilyPair(ExtFamily sourceFamily, ExtFamily destinationFamily)
        {
            sourceFamily.ParentFamilyPair = this;
            destinationFamily.ParentFamilyPair = this;
            SourceFamily = sourceFamily;
            DestinationFamily = destinationFamily;
            CategoryId = sourceFamily.CategoryId;
        }

        /// <summary>
        /// Идентификатор категории семейств
        /// </summary>
        public int CategoryId { get; }

        /// <summary>
        /// Семейство - источник (дубликат)
        /// </summary>
        public ExtFamily SourceFamily { get; }

        /// <summary>
        /// Семейство назначения
        /// </summary>
        public ExtFamily DestinationFamily { get; set; }
    }
}
