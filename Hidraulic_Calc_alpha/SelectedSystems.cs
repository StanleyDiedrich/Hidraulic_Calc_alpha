using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hidraulic_Calc_alpha
{
    public  class SelectedSystems
    {
        public string system {  get; set; }
        public List<string> systems {  get; set; }
        public List<string> preparedsystems { get; set; }

        public SelectedSystems() 
        {
            systems = new List<string>();
            preparedsystems = new List<string>();
        }
    }
    public class SelectedSystem
    {
        public string name { get; set; }
        public bool selected { get; set; }
        public SelectedSystem(Object Soc)
        {
            name = Soc.ToString();
            selected = false;
        }
    }
}
