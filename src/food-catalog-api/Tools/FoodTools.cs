using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;

namespace FoodApi;

/// <summary>
/// MCP tools exposing core Food Catalog operations.
/// All writes persist to the underlying EF Core database.
/// </summary>
[McpServerToolType]
internal class FoodTools(FoodDBContext db, ILogger<FoodTools> logger)
{
    private readonly FoodDBContext _db = db ?? throw new ArgumentNullException(nameof(db));
    private readonly ILogger<FoodTools> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    [McpServerTool]
    [Description("Lists all food items in the catalog.")]
    public async Task<FoodItemCollection> ListFood()
    {
        var items = await _db.Food.AsNoTracking().ToListAsync();
        return new FoodItemCollection { Items = items };
    }

    [McpServerTool]
    [Description("Searches food items by name, code or description substring.")]
    public async Task<FoodItemCollection> SearchFood(
        [Description("Term to match against name, code or description")] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new FoodItemCollection { Items = [] };
        }
        var term = searchTerm.Trim();
        var matches = await _db.Food.AsNoTracking()
            .Where(f => f.Name.Contains(term) || f.Code.Contains(term) || f.Description.Contains(term))
            .ToListAsync();
        return new FoodItemCollection { Items = matches };
    }

    [McpServerTool]
    [Description("Adds a new food item to the catalog")]
    public async Task<string> AddFood(
        [Description("Display name of the food item")] string name,
        [Description("Unique short code (optional)")] string? code = null,
        [Description("Marketing description (optional)")] string? description = null,
        [Description("Unit price (decimal)")] decimal price = 0m,
        [Description("Initial stock quantity")] int inStock = 0,
        [Description("Minimum stock for alerts")] int minStock = 0,
        [Description("Picture URL (optional)")] string? pictureUrl = null)
    {
        var entity = new FoodItem
        {
            Name = name?.Trim() ?? string.Empty,
            Code = code?.Trim() ?? string.Empty,
            Description = description?.Trim() ?? string.Empty,
            Price = price,
            InStock = inStock,
            MinStock = minStock,
            PictureUrl = pictureUrl?.Trim() ?? string.Empty
        };

        _db.Food.Add(entity);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Added food item {Name} (ID {Id})", entity.Name, entity.ID);
        return $"Added food item '{entity.Name}' with id {entity.ID}.";
    }

    [McpServerTool]
    [Description("Updates the stock level for a food item")]
    public async Task<string> UpdateStock(
        [Description("Existing food item id")] int id,
        [Description("Amount to add (can be negative)")] int amount)
    {
        var item = await _db.Food.FirstOrDefaultAsync(f => f.ID == id);
        if (item == null)
        {
            return $"Food item with id {id} not found.";
        }
        item.InStock += amount;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated stock for {Name} (ID {Id}) by {Amount}. New stock: {Stock}", item.Name, item.ID, amount, item.InStock);
        return $"Updated stock for '{item.Name}' to {item.InStock}.";
    }

    [McpServerTool]
    [Description("Removes a food item by id")]
    public async Task<string> RemoveFood(
        [Description("Food item id to remove")] int id)
    {
        var item = await _db.Food.FirstOrDefaultAsync(f => f.ID == id);
        if (item == null)
        {
            return $"Food item with id {id} not found.";
        }
        _db.Food.Remove(item);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Removed food item {Name} (ID {Id})", item.Name, item.ID);
        return $"Removed food item '{item.Name}'.";
    }
}