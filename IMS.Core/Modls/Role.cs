using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Core.Modls
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        //public ICollection<Person> Persons { get; set; } = new List<Person>();
    }
}
