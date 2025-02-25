using System.Globalization;
using HtmlAgilityPack;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal class KocourekRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;

        public KocourekRestaurant() : base(RestaurantType.Kocourek, "Zážitková Jídelna Kocourek")
        {
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Domain.Entities.Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            var currentYear = DateTime.Now.Year;
            var currentDayInYear = DateTime.Now.DayOfYear;
            var currentWeekInYear = currentDayInYear / 5;

            var url = $"http://www.jidelnakocourek.cz/jidelny-listek/{currentYear}{currentWeekInYear}";
            
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(url, cancellationToken);
            int dayOfWeek = (int)DateTime.Now.DayOfWeek - 1;

            var mainDiv = htmlDocument.DocumentNode
                .Descendants("div").First(s => s.Attributes.Contains("id") && s.Attributes["id"].Value == "maincontent");
            
            var tables = mainDiv.Descendants("table").ToList();
            var todayTable = tables[dayOfWeek];
            
            var menuItems = todayTable.Descendants("tr").ToList();

            var soups = new List<Soap> { Soap.Create(menuItems[0].Descendants("td").First().Descendants("b").Last().InnerText.Trim()) };
            var meals = menuItems.Skip(1)
                .Select(tr => tr.Descendants("td").Last().InnerText.Trim())
                .Where(text => text.Any(char.IsLetterOrDigit))
                .Select(Meal.Create)
                .ToList();

            if (soups.Count == 0)
            {
                return Restaurant.Create(Type, Menu.Create(meals), Name);
            }
            return Restaurant.Create(Type, Menu.Create(meals, soups), Name);
        }

        private string GetToday()
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
            var culture = new System.Globalization.CultureInfo("cs-CZ");
            return culture.DateTimeFormat.GetDayName(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone).DayOfWeek);
        }
    }
}
