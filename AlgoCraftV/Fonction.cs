using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoCraftV
{
    public class Fonction
    {
        public string Corps { get; set; }
        public string TypeRetour { get; set; }

        public Fonction(string corps, string typeRetour)
        {
            Corps = corps;
            TypeRetour = typeRetour;
        }
    }
}
