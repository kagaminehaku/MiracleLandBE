using System;
using System.Collections.Generic;

namespace MiracleLandBE;

public partial class UserAccount
{
    public Guid Uid { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public bool IsActive { get; set; }

    public string? Avt { get; set; }

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}
