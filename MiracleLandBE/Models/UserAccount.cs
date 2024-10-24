using System;
using System.Collections.Generic;

namespace MiracleLandBE.Models;

public partial class UserAccount
{
    public Guid Uid { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}
