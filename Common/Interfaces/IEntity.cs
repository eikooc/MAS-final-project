using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IEntity
    {
        int uid { get; set; }
        int col { get; set; }
        int row { get; set; }

        IEntity Clone();
    }
}
