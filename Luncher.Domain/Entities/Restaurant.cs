using Luncher.Core.Entities;

namespace Luncher.Domain.Entities
{
    public class Restaurant
    {
        public static Restaurant Create(RestaurantType type, Menu menu, string? name) => new(type, menu, name);

        public Restaurant(RestaurantType type, Menu menu, string? name = null)
        {
            Type = type;
            Menu = menu;
            Name = name;
        }

        public string? Name { get; set; }
        public RestaurantType Type { get; }
        public Menu Menu { get; }
    }
}
