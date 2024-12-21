using System;
using System.Collections.Generic;

namespace MiracleLandBE.Models

public partial class AdminLog
{
    public Guid LogId { get; set; }

    public string AdminId { get; set; } = null!;

    public DateTime ActionDate { get; set; }

    public string Action { get; set; } = null!;
}
