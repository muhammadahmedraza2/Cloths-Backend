using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public enum PaymentMethod
{
    CashOnDelivery = 1,
    Installment = 2,
    OnlineBankTransfer = 3,
    Card = 4
}
