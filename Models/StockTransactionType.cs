using System.ComponentModel.DataAnnotations.Schema;

namespace ClothingErp.Api.Models;

public enum StockTransactionType { Purchase = 0, Sale = 1, Return = 2, Adjustment = 3, Damage = 4 }
