using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareDLL
{
    [Serializable]
    public class MessageModel
    {
        public string Message { get; set; }
        public bool Status { get; set; }
    }
}
