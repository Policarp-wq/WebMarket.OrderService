using System;
using System.Collections.Generic;

namespace WebMarket.OrderService.Models;

public partial class CustomerHistory
{
    public int Id { get; set; }

    public int CutomerId { get; set; }

    public int ProductId { get; set; }

    public DateTime OrderDate { get; set; }
}
