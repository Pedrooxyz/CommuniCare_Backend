using System;
using System.Collections.Generic;

namespace CommuniCare.Models;

public partial class Cp
{
    public int Cpid { get; set; }

    public int? DescCp { get; set; }

    public virtual ICollection<Moradum> Morada { get; set; } = new List<Moradum>();
}
