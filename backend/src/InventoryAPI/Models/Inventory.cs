namespace InventoryAPI.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string SKU { get; set; }
    public required string Barcode { get; set; }
    public decimal Price { get; set; }
    public decimal CostPrice { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public int MaxStock { get; set; }
    public string? Category { get; set; }
    public string? Unit { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public int? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class Supplier
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
}

public class Warehouse
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Location { get; set; }
    public int Capacity { get; set; }
    public ICollection<Product> Products { get; set; } = [];
}

public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string Type { get; set; } = "in"; // "in"|"out"|"adjustment"
    public int Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Reference { get; set; }
    public int PreviousStock { get; set; }
    public int NewStock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PurchaseOrder
{
    public int Id { get; set; }
    public required string OrderNumber { get; set; }
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public string Status { get; set; } = "pending"; // pending|ordered|received|cancelled
    public decimal TotalAmount { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PurchaseOrderItem> Items { get; set; } = [];
}

public class PurchaseOrderItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
