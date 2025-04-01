using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Cp
{
    public int Cpid { get; set; }

    public string? DescCP { get; set; }

    public virtual ICollection<Morada> Morada { get; set; } = new List<Morada>();
}
