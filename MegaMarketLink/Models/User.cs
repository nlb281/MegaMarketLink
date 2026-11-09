using System;
using System.Collections.Generic;

namespace MegaMarketLink.Models;

public partial class User
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string CompanyName { get; set; } = null!;

    public string Inn { get; set; } = null!;

    public int RoleId { get; set; }

    public bool IsBanned { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual Role Role { get; set; } = null!;
}
