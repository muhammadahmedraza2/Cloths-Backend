using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public enum OrderStatus { Pending = 0, Confirmed = 1, Processing = 2, Packed = 3, Shipped = 4, Delivered = 5, Cancelled = 6, Returned = 7 }
