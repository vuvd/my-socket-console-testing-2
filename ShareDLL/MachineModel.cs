using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareDLL
{
    [Serializable]
    public class MachineModel
    {
        public string MachineId { get; set; }
        public string MachineName { get; set; }
        public DateTime DateOfBuy { get; set; }
        public bool Status { get; set; }
    }
}
