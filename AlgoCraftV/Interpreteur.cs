using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;


namespace AlgoCraftV
{
    public class Interpreteur
    {
       
        private Dictionary<string, Fonction> fonctions = new Dictionary<string, Fonction>();

        //dictionnaire qui stocke les variables déclarées dans l'algorithme, avec leur nom comme clé et un objet Variable (contenant la valeur et le type) comme valeur.
        private Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
        private Console maConsole; //  référence à la fenêtre de console personnalisée qui affichera les résultats.
        private RichTextBox consoleErreur; // zone de texte utilisée pour afficher les erreurs de l'exécution.

        //stocker l’état courant des blocs conditionnels dans une pile
        private Stack<bool> pileConditions = new Stack<bool>();
        private bool executerBloc = true; // indique si la ligne courante doit être exécutée

        //Initialise l'interpréteur avec la console d'affichage et la zone de texte pour les erreurs.
        public Interpreteur(Console console, RichTextBox erreurs)
        {
            maConsole = console;
            consoleErreur = erreurs;
        }

        // Découpe le code en lignes. Ignore les lignes vides. Interprète chaque ligne via InterpreterLigneAsync.
        public async Task ExecuterAsync(string code)
        {
            variables.Clear();
            consoleErreur.Clear();
            maConsole.Clear();

            string[] lignes = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int i = 0;
            while (i < lignes.Length)
            {
                string ligne = lignes[i].Trim();

                if (string.IsNullOrEmpty(ligne))
                {
                    i++;
                    continue;
                }

                if (ligne.StartsWith("Si"))
                {
                    // Regrouper les lignes jusqu'à FinSi
                    List<string> bloc = new List<string>();
                    bloc.Add(ligne);

                    i++;
                    while (i < lignes.Length && !lignes[i].Trim().StartsWith("FinSi"))
                    {
                        bloc.Add(lignes[i].Trim());
                        i++;
                    }

                    if (i < lignes.Length)
                        bloc.Add(lignes[i].Trim()); // ajouter FinSi

                    await InterpreterBlocSiAsync(bloc);

                    i++; // passer à la ligne suivante après FinSi
                    continue;
                }
                else if (ligne.StartsWith("TantQue"))
                {
                    var bloc = ExtraireBloc(lignes, ref i, "TantQue", "FinTantQue");
                    await InterpreterBlocTantQueAsync(bloc);

                    i++; // passer à la ligne suivante après FinTantQue
                    continue;
                }
                else if (ligne.StartsWith("Pour"))
                {
                    var bloc = ExtraireBloc(lignes, ref i, "Pour", "FinPour");
                    await InterpreterBlocPourAsync(bloc);
                    continue;
                }
                else if (ligne.StartsWith("Repeter"))
                {
                    var bloc = ExtraireBloc(lignes, ref i, "Repeter", "Jusqu'à");
                    await InterpreterBlocRepeterAsync(bloc);
                    continue;
                }

                else
                {
                    await InterpreterLigneAsync(ligne);
                    i++;
                }
            }
        }

        private List<string> ExtraireBloc(string[] lignes, ref int index, string debut, string fin)
        {
            List<string> bloc = new List<string>();
            int compteur = 0;

            for (int i = index; i < lignes.Length; i++)
            {
                string ligne = lignes[i].Trim();

                if (ligne.StartsWith(debut))
                    compteur++;
                if (ligne.StartsWith(fin))
                    compteur--;

                bloc.Add(ligne);

                if (compteur == 0)
                {
                    index = i; // met à jour la position de la ligne courante
                    break;
                }
            }

            if (compteur != 0)
                throw new Exception($"Bloc {debut} ... {fin} mal fermé");

            return bloc;
        }

        // interpreteur de code pour la condition
        private async Task InterpreterBlocSiAsync(List<string> bloc)
        {
            string conditionLigne = bloc[0];
            var match = Regex.Match(conditionLigne, @"Si\s+(.+)\s+Alors");

            if (!match.Success)
                throw new Exception("Syntaxe incorrecte pour Si ... Alors");

            string condition = match.Groups[1].Value;

            // ✅ Remplacements pour compatibilité avec DataTable
            condition = condition.Replace("!=", "<>")
                                 .Replace("==", "=")
                                 .Replace(" ET ", " AND ")
                                 .Replace(" OU ", " OR ")
                                 .Replace("\"", "'") // très important
                                 .Replace("vrai", "true")
                                 .Replace("faux", "false")
                                 .Replace("VRAI", "true")
                                 .Replace("FAUX", "false");

            // ✅ Remplacer noms de variables par leur valeur
            foreach (var v in variables)
            {
                string valeurTexte;
                if (v.Value.Type == "char")
                    valeurTexte = $"'{v.Value.Valeur}'"; // utilise apostrophes pour DataTable
                else if (v.Value.Type == "booleen")
                    valeurTexte = v.Value.Valeur.ToString().ToLower();
                else
                    valeurTexte = v.Value.Valeur.ToString();

                condition = Regex.Replace(condition, $@"\b{v.Key}\b", valeurTexte);
            }

            // ✅ Évaluation de la condition
            bool conditionVraie;
            try
            {
                object res = new DataTable().Compute(condition, "");
                conditionVraie = Convert.ToBoolean(res);
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur d’évaluation de la condition : " + condition + "\n" + ex.Message);
            }

            // ✅ Exécution du bon bloc
            bool dansSinon = false;

            for (int j = 1; j < bloc.Count - 1; j++) // ignore Si et FinSi
            {
                string ligne = bloc[j].Trim();

                if (ligne.StartsWith("Sinon"))
                {
                    dansSinon = true;
                    continue;
                }

                if ((conditionVraie && !dansSinon) || (!conditionVraie && dansSinon))
                {
                    await InterpreterLigneAsync(ligne);
                }
            }
        }

        //Crée une entrée Variable vide dans le dictionnaire avec un type.
        private async Task InterpreterLigneAsync(string ligne)
        {
            ligne = ligne.Trim();

            if (!executerBloc)
                return;
            #region fonctions et procedures
            if (ligne.StartsWith("FONCTION"))
            {
                // Extrait nom et type retour : FONCTION ObtenirNombre() : entier
                var match = Regex.Match(ligne, @"FONCTION\s+(\w+)\s*\(\)\s*:\s*(\w+)");
                if (match.Success)
                {
                    string nom = match.Groups[1].Value;
                    string typeRetour = match.Groups[2].Value;

                    string corps = "";
                    while (true)
                    {
                        string ligneSuivante = await maConsole.LireDepuisConsole("..."); // Demande manuelle ligne par ligne
                        if (ligneSuivante.Trim().ToUpper() == "FIN") break;
                        corps += ligneSuivante + Environment.NewLine;
                    }

                    fonctions[nom] = new Fonction(corps, typeRetour);
                    return;
                }
            }
            #endregion
            // Reconnaître et exécuter les blocs Si, Sinon, FinSi
            if (ligne.StartsWith("Si"))
            {
                string condition = ligne.Substring(2).Replace("Alors", "").Trim();
                bool resultat = EvaluerCondition(condition);
                pileConditions.Push(resultat);
                executerBloc = resultat;
            }
            else if (ligne.StartsWith("Sinon"))
            {
                if (pileConditions.Count > 0)
                {
                    bool precedent = pileConditions.Pop();
                    pileConditions.Push(!precedent);
                    executerBloc = !precedent;
                }
            }
            else if (ligne.StartsWith("FinSi"))
            {
                if (pileConditions.Count > 0)
                    pileConditions.Pop();
                executerBloc = pileConditions.Count == 0 || pileConditions.Peek();
            }



            if (ligne.StartsWith("entier ") || ligne.StartsWith("reel ") || ligne.StartsWith("char ") || ligne.StartsWith("booleen "))
            {
                string[] parts = ligne.Split(new[] { ' ' }, 2);
                string type = parts[0];
                string nom = parts[1].Trim();

                if (!variables.ContainsKey(nom))
                    variables[nom] = new Variable(nom, null, type);
                return;
            }
            if (ligne.StartsWith("Ecrire"))
            {
                var match = Regex.Match(ligne, @"Ecrire\s*\((.*)\)");
                if (match.Success)
                {
                    string contenu = match.Groups[1].Value;
                    string[] morceaux = SplitArguments(contenu); // on utilise une fonction pour bien séparer les arguments
                    string resultat = "";

                    foreach (var morceau in morceaux)
                    {
                        string element = morceau.Trim();

                        // Si c'est un char entre guillemets
                        if (element.StartsWith("\"") && element.EndsWith("\""))
                        {
                            resultat += element.Trim('"');
                        }
                        // Fonction : exemple ObtenirNombre()
                        else if (Regex.IsMatch(element, @"^(\w+)\(\)$"))
                        {
                            string nomFonction = element.Replace("()", "");
                            if (fonctions.ContainsKey(nomFonction))
                            {
                                Fonction f = fonctions[nomFonction];
                                string corps = f.Corps;
                                string typeRetour = f.TypeRetour;
                                string valRetour = ExtraireValeurRetour(corps);
                                resultat += valRetour;
                            }

                            else
                            {
                                throw new Exception($"Fonction inconnue : {nomFonction}");
                            }
                        }
                        // Sinon on suppose que c’est une variable
                        else if (variables.ContainsKey(element))
                        {
                            resultat += variables[element].Valeur?.ToString() ?? "null";
                        }
                        else
                        {
                            throw new Exception($"Élément inconnu ou non déclaré : {element}");
                        }
                    }

                    maConsole.AfficherLigne(resultat);
                }
                else
                {
                    throw new Exception("Syntaxe Ecrire incorrecte");
                }
            }
            else if (ligne.StartsWith("Lire"))
            {
                var match = Regex.Match(ligne, @"Lire\s*\(\s*""?(.*?)""?\s*\)");
                if (match.Success)
                {
                    string variable = match.Groups[1].Value;
                    string saisie = await maConsole.LireDepuisConsole(variable);
                    if (variables.ContainsKey(variable))
                    {
                        var varObj = variables[variable];
                        switch (varObj.Type)
                        {
                            case "entier":
                                varObj.Valeur = int.Parse(saisie);
                                break;
                            case "reel":
                                varObj.Valeur = double.Parse(saisie);
                                break;
                            case "booleen":
                                varObj.Valeur = bool.Parse(saisie);
                                break;
                            case "char":
                                varObj.Valeur = saisie;
                                break;
                        }
                    }
                }
                else
                {
                    throw new Exception("Syntaxe Lire incorrecte");
                }
            }
            else if (ligne.Contains("←"))
            {
                var match = Regex.Match(ligne, @"^(\w+)\s*←\s*(.+)$");
                if (match.Success)
                {
                    string nomVar = match.Groups[1].Value;
                    string expression = match.Groups[2].Value.Trim();

                    if (!variables.ContainsKey(nomVar))
                        throw new Exception($"Variable non déclarée : {nomVar}");

                    var variable = variables[nomVar];

                    // Remplacer les noms de variables dans l'expression par leur valeur
                    foreach (var v in variables)
                    {
                        if (v.Value.Valeur != null)
                        {
                            string valStr = v.Value.Type == "char" ? $"\"{v.Value.Valeur}\"" : v.Value.Valeur.ToString();
                            expression = Regex.Replace(expression, $@"\b{v.Key}\b", valStr);
                        }
                    }

                    // Évaluer l'expression
                    object resultat;
                    try
                    {
                        resultat = new DataTable().Compute(expression, "");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Erreur dans l'évaluation de l'expression '{expression}' : {ex.Message}");
                    }

                    if (Regex.IsMatch(expression, @"^(\w+)\(\)$"))
                    {
                        string nomFonction = expression.Replace("()", "");
                        if (fonctions.ContainsKey(nomFonction))
                        {
                            Fonction f = fonctions[nomFonction];
                            string corps = f.Corps;
                            string typeRetour = f.TypeRetour;
                            expression = ExtraireValeurRetour(corps);

                            // Convertir selon le type
                            switch (variable.Type)
                            {
                                case "entier":
                                    variable.Valeur = Convert.ToInt32(resultat);
                                    break;
                                case "reel":
                                    variable.Valeur = Convert.ToDouble(resultat);
                                    break;
                                case "booleen":
                                    variable.Valeur = Convert.ToBoolean(resultat);
                                    break;
                                case "char":
                                    variable.Valeur = resultat.ToString();
                                    break;
                                default:
                                    throw new Exception("Type non reconnu");
                            }
                            maConsole.AfficherLigne($"✅ {nomVar} = {variable.Valeur}");
                            return;  // Quitter la méthode ici, car l'affectation est faite
                        }
                    }
                    else
                    {
                        throw new Exception("Syntaxe d'affectation invalide");
                    }
                }
            }


            else
            {
                consoleErreur.AppendText("⛔ Instruction inconnue : " + ligne + "\n");
            }
        }

        #region fonctions et procedures
            private string ExtraireValeurRetour(string corps)
        {
            foreach (var ligne in corps.Split('\n'))
            {
                if (ligne.Trim().StartsWith("RETOURNER"))
                {
                    return ligne.Substring("RETOURNER".Length).Trim();
                }
            }
            return "0"; // valeur par défaut
        }
        #endregion

        private string[] SplitArguments(string input)
        {
            var result = new List<string>();
            var current = "";
            bool inString = false;

            foreach (char c in input)
            {
                if (c == '"')
                {
                    inString = !inString;
                    current += c;
                }
                else if (c == ',' && !inString)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrWhiteSpace(current))
                result.Add(current);

            return result.ToArray();
        }

        private object EvaluerExpression(string expression)
        {
            // Remplacer les noms de variables par leurs valeurs
            foreach (var variable in variables)
            {
                expression = Regex.Replace(expression, $@"\b{variable.Key}\b", variable.Value.ToString());
            }

            try
            {
                var result = new System.Data.DataTable().Compute(expression, "");
                return result;
            }
            catch
            {
                throw new Exception("Expression arithmétique invalide : " + expression);
            }
        }
        // permet d'évaluer une condition avec comparateurs
        private bool EvaluerCondition(string condition)
        {
            foreach (var variable in variables)
            {
                if (variable.Value.Valeur != null)
                {
                    condition = Regex.Replace(condition, $@"\b{variable.Key}\b", variable.Value.Valeur.ToString());
                }
            }

            condition = condition.Replace("=", "==").Replace("<>", "!="); // gestion des opérateurs

            try
            {
                var result = new DataTable().Compute(condition, "");
                return Convert.ToBoolean(result);
            }
            catch
            {
                throw new Exception("Condition invalide : " + condition);
            }
        }

        private bool EvaluerConditionManuellement(string condition)
        {
            // Remplacement logique
            condition = condition.Replace(" ET ", " && ")
                                 .Replace(" et ", " && ")
                                 .Replace(" OU ", " || ")
                                 .Replace(" ou ", " || ")
                                 .Replace("VRAI", "true")
                                 .Replace("FAUX", "false")
                                 .Replace("vrai", "true")
                                 .Replace("faux", "false");

            // Remplacer les comparateurs personnalisés sans conflit
            condition = condition.Replace("!=", "#diff#")
                                 .Replace(">=", "#ge#")
                                 .Replace("<=", "#le#")
                                 .Replace("=", "==")
                                 .Replace("#diff#", "!=")
                                 .Replace("#ge#", ">=")
                                 .Replace("#le#", "<=");

            // Remplacer les variables par leurs valeurs
            foreach (var v in variables)
            {
                string nom = v.Key;
                object val = v.Value.Valeur;
                string type = v.Value.Type;

                if (val == null)
                    continue;

                string valeurTexte;
                if (type == "char")
                    valeurTexte = $"'{val}'"; // ⚠️ Utiliser apostrophes pour ne pas casser Compute()
                else if (type == "booleen")
                    valeurTexte = val.ToString().ToLower();
                else
                    valeurTexte = val.ToString();

                condition = Regex.Replace(condition, $@"\b{nom}\b", valeurTexte);
            }

            try
            {
                var res = new DataTable().Compute(condition, "");
                return Convert.ToBoolean(res);
            }
            catch (Exception ex)
            {
                throw new Exception("Erreur d’évaluation de la condition : " + condition + "\n" + ex.Message);
            }
        }

        private async Task InterpreterBlocTantQueAsync(List<string> bloc)
        {
            string ligneCondition = bloc[0];
            var match = Regex.Match(ligneCondition, @"TantQue\s+(.+)\s+Faire");
            if (!match.Success)
                throw new Exception("Syntaxe incorrecte pour TantQue ... Faire");

            string condition = match.Groups[1].Value;

            while (true)
            {
                string conditionEval = condition;

                // Remplacer les variables par leurs valeurs actuelles
                foreach (var v in variables)
                {
                    string valStr;
                    if (v.Value.Type == "chaîne")
                        valStr = $"\"{v.Value.Valeur}\"";
                    else if (v.Value.Type == "booléen")
                        valStr = v.Value.Valeur.ToString().ToLower();
                    else
                        valStr = v.Value.Valeur.ToString();

                    conditionEval = Regex.Replace(conditionEval, $@"\b{v.Key}\b", valStr);
                }

                bool conditionVraie;
                try
                {
                    var res = new DataTable().Compute(conditionEval, "");
                    conditionVraie = Convert.ToBoolean(res);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erreur d’évaluation de la condition dans TantQue : {conditionEval}\n{ex.Message}");
                }

                if (!conditionVraie)
                    break;

                // Exécuter toutes les instructions sauf la première (TantQue) et la dernière (FinTantQue)
                for (int i = 1; i < bloc.Count - 1; i++)
                {
                    await InterpreterLigneAsync(bloc[i]);
                }
            }
        }

        private async Task InterpreterBlocPourAsync(List<string> bloc)
        {
            // Exemple de ligne : "Pour i ← 0 À 10 Faire"
            string ligneDebut = bloc[0];
            var match = Regex.Match(ligneDebut, @"Pour\s+(\w+)\s*←\s*(.+)\s+À\s+(.+)\s+Faire");

            if (!match.Success)
                throw new Exception("Syntaxe incorrecte pour Pour ... Faire");

            string varCompteur = match.Groups[1].Value;
            string valeurDebutStr = match.Groups[2].Value;
            string valeurFinStr = match.Groups[3].Value;

            // Remplacer variables dans les expressions si besoin
            foreach (var v in variables)
            {
                if (v.Value.Valeur != null)
                {
                    valeurDebutStr = Regex.Replace(valeurDebutStr, $@"\b{v.Key}\b", v.Value.Valeur.ToString());
                    valeurFinStr = Regex.Replace(valeurFinStr, $@"\b{v.Key}\b", v.Value.Valeur.ToString());
                }
            }

            int valeurDebut = Convert.ToInt32(new DataTable().Compute(valeurDebutStr, ""));
            int valeurFin = Convert.ToInt32(new DataTable().Compute(valeurFinStr, ""));

            // Initialiser la variable compteur si elle n'existe pas
            if (!variables.ContainsKey(varCompteur))
                variables[varCompteur] = new Variable(varCompteur, valeurDebut, "entier");
            else
                variables[varCompteur].Valeur = valeurDebut;

            // Exécuter la boucle
            for (int i = valeurDebut; i <= valeurFin; i++)
            {
                variables[varCompteur].Valeur = i;

                for (int j = 1; j < bloc.Count - 1; j++) // Exclure la ligne Pour ... Faire et FinPour
                {
                    string ligne = bloc[j].Trim();
                    await InterpreterLigneAsync(ligne);
                }
            }
        }

        private async Task InterpreterBlocRepeterAsync(List<string> bloc)
        {
            // La dernière ligne est de la forme "Jusqu'à condition"
            string derniereLigne = bloc.Last().Trim();

            var match = Regex.Match(derniereLigne, @"Jusqu'à\s+(.+)", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new Exception("Syntaxe incorrecte pour Repeter ... Jusqu'à");

            string condition = match.Groups[1].Value;

            // On récupère les instructions entre Repeter et Jusqu'à
            var instructions = bloc.Skip(1).Take(bloc.Count - 2).ToList();

            bool conditionVraie = false;

            do
            {
                // Exécuter toutes les instructions
                foreach (var ligne in instructions)
                {
                    await InterpreterLigneAsync(ligne);
                }

                // Évaluer la condition, remplacer variables par leurs valeurs
                string conditionEval = condition;
                foreach (var v in variables)
                {
                    string valeurTexte = v.Value.Valeur == null ? "null" :
                        (v.Value.Type == "chaîne" ? $"\"{v.Value.Valeur}\"" : v.Value.Valeur.ToString());

                    conditionEval = Regex.Replace(conditionEval, $@"\b{v.Key}\b", valeurTexte);
                }

                try
                {
                    object res = new DataTable().Compute(conditionEval, "");
                    conditionVraie = Convert.ToBoolean(res);
                }
                catch (Exception ex)
                {
                    throw new Exception("Erreur d’évaluation de la condition dans Jusqu'à : " + ex.Message);
                }

            } while (!conditionVraie);
        }

        #region les fonctions mathematiques simples
        private object EvaluerFonctionMath(string expression)
        {
            // Support de : abs(), sqrt(), pow(), min(), max(), round()
            expression = expression.ToLower();

            try
            {
                if (expression.StartsWith("abs("))
                {
                    double x = ExtraireUnArgument(expression);
                    return Math.Abs(x);
                }
                else if (expression.StartsWith("sqrt("))
                {
                    double x = ExtraireUnArgument(expression);
                    return Math.Sqrt(x);
                }
                else if (expression.StartsWith("round("))
                {
                    double x = ExtraireUnArgument(expression);
                    return Math.Round(x);
                }
                else if (expression.StartsWith("pow("))
                {
                    double[] args = ExtraireDeuxArguments(expression);
                    return Math.Pow(args[0], args[1]);
                }
                else if (expression.StartsWith("min("))
                {
                    double[] args = ExtraireDeuxArguments(expression);
                    return Math.Min(args[0], args[1]);
                }
                else if (expression.StartsWith("max("))
                {
                    double[] args = ExtraireDeuxArguments(expression);
                    return Math.Max(args[0], args[1]);
                }

                throw new Exception("Fonction mathématique non supportée : " + expression);
            }
            catch
            {
                throw new Exception("Erreur dans l’évaluation de : " + expression);
            }
        }
        private double ExtraireUnArgument(string expression)
        {
            int debut = expression.IndexOf('(') + 1;
            int fin = expression.IndexOf(')');
            string contenu = expression.Substring(debut, fin - debut);
            return double.Parse(contenu.Trim());
        }

        private double[] ExtraireDeuxArguments(string expression)
        {
            int debut = expression.IndexOf('(') + 1;
            int fin = expression.IndexOf(')');
            string contenu = expression.Substring(debut, fin - debut);
            string[] parties = contenu.Split(',');

            if (parties.Length != 2)
                throw new Exception("Nombre d'arguments invalide");

            return new double[]
            {
        double.Parse(parties[0].Trim()),
        double.Parse(parties[1].Trim())
            };
        }

        #endregion


    }
}
