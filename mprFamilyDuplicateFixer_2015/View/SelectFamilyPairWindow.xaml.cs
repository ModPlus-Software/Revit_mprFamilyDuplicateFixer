namespace mprFamilyDuplicateFixer.View
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Model;
    using Visibility = System.Windows.Visibility;

    /// <summary>
    /// Окно выбора пары семейств "Дубликат-Основное семейство"
    /// </summary>
    public partial class SelectFamilyPairWindow
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="uiApplication">UIApplication</param>
        /// <param name="skipFamilies">Список семейств, которые уже присутствуют в списке обработки</param>
        public SelectFamilyPairWindow(UIApplication uiApplication, List<string> skipFamilies)
        {
            InitializeComponent();
            Title = ModPlusAPI.Language.GetItem(ModPlusConnector.Instance.Name, "h6");

            Categories = new List<ExtCategoryForSelection>();

            var families = new FilteredElementCollector(uiApplication.ActiveUIDocument.Document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.FamilyCategory != null && !f.IsInPlace)
                .ToList();

            Dictionary<int, Category> categories = families
                .Select(f => f.FamilyCategory)
                .GroupBy(f => f.Id.IntegerValue)
                .ToDictionary(g => g.Key, g => g.First());

            var groupedByCategory = families.GroupBy(f => f.FamilyCategoryId.IntegerValue);
            foreach (IGrouping<int, Family> grouping in groupedByCategory)
            {
                var extCategoryForSelection = new ExtCategoryForSelection(categories[grouping.Key]);
                foreach (Family family in grouping)
                {
                    if (skipFamilies.Contains(family.Name))
                        continue;

                    var extFamilyForSelection = new ExtFamilyForSelection(extCategoryForSelection, family);
                    extCategoryForSelection.Families.Add(extFamilyForSelection);
                }
                
                if (extCategoryForSelection.Families.Count > 1)
                    Categories.Add(extCategoryForSelection);
            }

            TvAllFamilies.ItemsSource = Categories;
        }

        /// <summary>
        /// Все семейства по категориям
        /// </summary>
        public List<ExtCategoryForSelection> Categories { get; }

        /// <summary>
        /// Выбранная пара
        /// </summary>
        public ExtFamilyPair SelectedPair { get; set; }

        private void TvAllFamilies_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ExtFamilyForSelection extFamilyForSelection)
            {
                var similarFamilies = extFamilyForSelection.ParentCategory.Families
                    .Where(f => f != extFamilyForSelection && 
                                extFamilyForSelection.IsSimilarTo(f))
                    .ToList();
                LbDestinationFamilies.ItemsSource = similarFamilies;
                TbMessageAboutSearchingDuplicates.Visibility = similarFamilies.Any() ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                LbDestinationFamilies.ItemsSource = null;
                TbMessageAboutSearchingDuplicates.Visibility = Visibility.Collapsed;
            }
        }

        private void LbDestinationFamilies_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox lb)
            {
                BtAccept.IsEnabled = lb.SelectedIndex != -1;
            }
        }

        private void BtAccept_OnClick(object sender, RoutedEventArgs e)
        {
            if (TvAllFamilies.SelectedItem is ExtFamilyForSelection sourceFamilyForSelection &&
                LbDestinationFamilies.SelectedItem is ExtFamilyForSelection destFamilyForSelection)
            {
                SelectedPair = new ExtFamilyPair(
                    new ExtFamily(sourceFamilyForSelection),
                    new ExtFamily(destFamilyForSelection));
            }

            DialogResult = true;
        }
    }
}
