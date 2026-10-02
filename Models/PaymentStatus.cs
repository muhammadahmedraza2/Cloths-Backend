using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public enum PaymentStatus
{
    Pending = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Failed = 4,
    Cancelled = 5
}
