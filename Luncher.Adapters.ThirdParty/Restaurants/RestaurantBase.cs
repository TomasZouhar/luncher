using Luncher.Core.Entities;
using Luncher.Domain.Entities;

namespace Luncher.Adapters.ThirdParty.Restaurants
{
    internal abstract class RestaurantBase : Domain.Contracts.IRestaurant
    {
        public RestaurantType Type { get; }
        public string? Name { get; set; }

        public RestaurantBase(RestaurantType restaurantType, string? name)
        {
            Type = restaurantType;
            Name = name;
        }

        public async Task<Restaurant> GetInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await GetInfoCoreAsync(cancellationToken);
            }
            catch (Exception)
            {
                //Log
                return Restaurant.Create(Type, Menu.Empty, null);
            }
        }

        protected abstract Task<Restaurant> GetInfoCoreAsync(CancellationToken cancellationToken);


    }
}
