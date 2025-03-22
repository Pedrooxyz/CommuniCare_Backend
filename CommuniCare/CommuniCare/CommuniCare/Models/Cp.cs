using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Cp
{
    public int Cpid { get; set; }

    public int? DescCp { get; set; }

    public virtual ICollection<Morada> Morada { get; set; } = new List<Morada>();
}
