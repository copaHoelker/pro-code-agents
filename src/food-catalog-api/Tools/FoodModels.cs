using System.Collections.Generic;

namespace FoodApi;

/// <summary>
/// Collection wrapper returned by MCP tools for food items.
/// </summary>
public class FoodItemCollection
{
    public List<FoodItem> Items { get; set; } = new();
}