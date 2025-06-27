using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace AlgoCraftV
{
    public class Variable
    {
        public string Nom { get; set; }
        public object Valeur { get; set; }
        public string Type { get; set; }

        public Variable(string nom, object valeur, string type)
        {
            Nom = nom;
            Valeur = valeur;
            Type = type;
        }

        public override string ToString()
        {
            return Valeur?.ToString() ?? "null";
        }
    }
}
