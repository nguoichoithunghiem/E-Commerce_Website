using demo.Models;

public class CartItemModel
{
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Size { get; set; }  // Add Size property
    public string? Color { get; set; } // Add Color property

    public decimal Total
    {
        get { return Quantity * Price; }
    }

    public string? Image { get; set; }

    // Default constructor
    public CartItemModel()
    {
    }

    // Constructor accepting a product and size, color parameters
    public CartItemModel(ProductModel product, string size, string color)
    {
        ProductId = product.Id;
        ProductName = product.Name;
        Price = product.Price;
        Quantity = 1;  // Default quantity
        Image = product.Image;
        Size = size;  // Set size
        Color = color; // Set color
    }
}
