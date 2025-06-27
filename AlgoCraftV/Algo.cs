using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace AlgoCraftV
{
    class Algo
    {
        // Description de l'algorithme (s'affiche en haut)
        public string Description { get; set; }

        // Le code principal de l'algorithme écrit par l'utilisateur
        public string Contenu { get; set; }

        // Méthode qui ajoute une instruction dans l'algorithme
        public void AjouterInstruction(string ligne)
        {
            Contenu += ligne + Environment.NewLine;
        }

        // Méthode qui vérifie si l'algorithme est "valide"
        // (dans ce cas très simple, on vérifie juste qu'il n'est pas vide)
        public bool EstValide()
        {
            return !string.IsNullOrWhiteSpace(Contenu);
        }
    }
}
