using HtmlAgilityPack;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;

namespace Luncher.Adapters.ThirdParty.Restaurants;

internal class TesarRestaurant : RestaurantBase
{
    private readonly HtmlWeb _htmlWeb;
    private string Url => "https://www.utesare.cz/poledni-nabidka/";

    public TesarRestaurant() : base(RestaurantType.Tesar, "U Tesaře")
    {
        _htmlWeb = new HtmlWeb();
    }

    protected override async Task<Domain.Entities.Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
    {
        var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);

        var container = htmlDocument.DocumentNode
            .SelectSingleNode("//div[contains(@class, 'elementor-widget-container')]");

        var soups = new List<Soap>();
        var meals = new List<Meal>();

        if (container != null)
        {
            var nodes = container.ChildNodes
                .Where(n => n.Name == "p" || n.Name == "ul")
                .ToList();

            string currentSection = string.Empty;

            foreach (var node in nodes)
            {
                if (node.Name == "p")
                {
                    var text = HtmlEntity.DeEntitize(node.InnerText).Trim().ToLower();

                    if (text.Contains("polévky"))
                        currentSection = "soups";
                    else if (text.Contains("hlavní chody"))
                        currentSection = "meals";
                    else if (text.Contains("dezert") || text.Contains("doporučujeme") || text.Contains("nabídka"))
                        currentSection = string.Empty; // Ignore desserts, recommendations, etc.
                }

                else if (node.Name == "ul" && !string.IsNullOrEmpty(currentSection))
                {
                    var items = node.Elements("li").ToList();
                    if (items == null || items.Count == 0) continue;

                    foreach (var item in items)
                    {
                        // Extract main item text
                        var mainText = HtmlEntity.DeEntitize(item.InnerText).Trim();
                        mainText = CleanMenuText(mainText);

                        if (string.IsNullOrWhiteSpace(mainText)) continue;

                        if (currentSection == "soups")
                            soups.Add(Soap.Create(mainText));
                        else if (currentSection == "meals")
                            meals.Add(Meal.Create(mainText));
                    }
                }
            }
        }

        return soups.Count == 0
            ? Restaurant.Create(Type, Menu.Create(meals), Name)
            : Restaurant.Create(Type, Menu.Create(meals, soups), Name);
    }

    private string CleanMenuText(string input)
    {
        // Remove prices like "89,-", "169,-", "249,-"
        var cleaned = System.Text.RegularExpressions.Regex.Replace(input, @"\s*\d{1,3},-\s*", "");

        // Remove allergen numbers like "(1,3,7,10)"
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\(\d+(,\d+)*\)", "");

        // Remove weights like "150g", "120g", "1ks", case-insensitive
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\b\d{1,3}g\b|\b1ks\b", "",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Normalize whitespace
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ");

        return cleaned.Trim();
    }
}