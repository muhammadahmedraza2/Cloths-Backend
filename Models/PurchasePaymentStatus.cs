using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public enum PurchasePaymentStatus { Pending = 0, Paid = 1, PartiallyPaid = 2, Cancelled = 3 }
