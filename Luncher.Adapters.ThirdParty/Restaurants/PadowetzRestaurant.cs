﻿using HtmlAgilityPack;
using Luncher.Adapters.ThirdParty.Restaurants;
using Luncher.Core.Entities;
using Luncher.Domain.Entities;
using System.Text;
using System.Text.RegularExpressions;

namespace Luncher.Adapters.ThirdParty
{
    internal class PadowetzRestaurant : RestaurantBase
    {
        private readonly HtmlWeb _htmlWeb;
        private string Url => $"http://www.restaurant-padowetz.cz/poledni-menu.html";

        public PadowetzRestaurant() : base(RestaurantType.Padowetz)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _htmlWeb = new HtmlWeb();
        }

        protected override async Task<Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken)
        {
            var htmlDocument = await _htmlWeb.LoadFromWebAsync(Url, cancellationToken);

            var todayMenuNode = htmlDocument.DocumentNode.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "col-md-7")
                .ToList()[((int)DateTime.Today.DayOfWeek - 1) % 5];;

            var soaps = todayMenuNode.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "col-sm-8 col-md-9")
                .Select(s => s.InnerText)
                .Select(Soap.Create)
                .Take(2)
                .ToList();

            var meals = todayMenuNode.Descendants("div")
                .Where(s => s.Attributes.Contains("class") && s.Attributes["class"].Value == "col-sm-8 col-md-9")
                .Select(s => Regex.Replace(s.InnerText, @"\s+", " "))
                .Select(Meal.Create)
                .TakeLast(5)
                .ToList();

            return Restaurant.Create(Type, Menu.Create(meals, soaps));
        }
    }
}
