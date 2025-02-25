﻿namespace Luncher.Web.Models
{
    public record RestaurantResponse(string Name, string Type, ICollection<Food> Soaps, ICollection<Food> Meals, int Votes);

    public record Food(string Name);

}
