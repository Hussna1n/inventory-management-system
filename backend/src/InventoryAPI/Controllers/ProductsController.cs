using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.Models;

namespace InventoryAPI.Controllers;

[ApiController, Route("api/products")]
public class ProductsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category, [FromQuery] string? search,
        [FromQuery] bool? lowStock, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var query = db.Products
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(category)) query = query.Where(p => p.Category == category);
        if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Name.Contains(search) || p.SKU.Contains(search));
        if (lowStock == true) query = query.Where(p => p.Stock <= p.MinStock);

        var total = await query.CountAsync();
        var products = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return Ok(new { products, total, pages = (int)Math.Ceiling(total / (double)limit) });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await db.Products
            .Include(p => p.Supplier)
            .Include(p => p.Warehouse)
            .Include(p => p.StockMovements.OrderByDescending(m => m.CreatedAt).Take(10))
            .FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest req)
    {
        var product = new Product {
            Name = req.Name, SKU = req.SKU, Barcode = req.Barcode ?? req.SKU,
            Price = req.Price, CostPrice = req.CostPrice, Stock = req.InitialStock,
            MinStock = req.MinStock, MaxStock = req.MaxStock, Category = req.Category,
            SupplierId = req.SupplierId, WarehouseId = req.WarehouseId
        };
        db.Products.Add(product);

        if (req.InitialStock > 0)
        {
            db.StockMovements.Add(new StockMovement {
                Product = product, Type = "in", Quantity = req.InitialStock,
                Reason = "Initial stock", PreviousStock = 0, NewStock = req.InitialStock
            });
        }
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPost("{id}/stock")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] StockAdjustmentRequest req)
    {
        var product = await db.Products.FindAsync(id);
        if (product is null) return NotFound();

        var prev = product.Stock;
        if (req.Type == "in") product.Stock += req.Quantity;
        else if (req.Type == "out") product.Stock -= req.Quantity;
        else product.Stock = req.Quantity;

        db.StockMovements.Add(new StockMovement {
            ProductId = id, Type = req.Type, Quantity = req.Quantity,
            Reason = req.Reason, PreviousStock = prev, NewStock = product.Stock
        });
        await db.SaveChangesAsync();
        return Ok(new { newStock = product.Stock });
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var totalProducts = await db.Products.CountAsync(p => p.IsActive);
        var lowStock = await db.Products.Where(p => p.Stock <= p.MinStock && p.IsActive).ToListAsync();
        var totalValue = await db.Products.Where(p => p.IsActive).SumAsync(p => p.Stock * p.CostPrice);
        var byCategory = await db.Products
            .Where(p => p.IsActive)
            .GroupBy(p => p.Category)
            .Select(g => new { category = g.Key, count = g.Count(), value = g.Sum(p => p.Stock * p.CostPrice) })
            .ToListAsync();
        var recentMovements = await db.StockMovements
            .Include(m => m.Product)
            .OrderByDescending(m => m.CreatedAt).Take(10).ToListAsync();

        return Ok(new { totalProducts, lowStockCount = lowStock.Count, lowStockItems = lowStock.Take(5), totalValue, byCategory, recentMovements });
    }
}

public record CreateProductRequest(string Name, string SKU, string? Barcode, decimal Price, decimal CostPrice,
    int InitialStock, int MinStock, int MaxStock, string? Category, int? SupplierId, int? WarehouseId);
public record StockAdjustmentRequest(string Type, int Quantity, string? Reason);
