using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViTAmin
{
    public class SinalListItem
    {
        public string Name { get; set; }
        public double Value { get; set; }

        public SinalListItem(string name, double value)
        {
            Name = name;
            Value = value;
        }
    }
}
