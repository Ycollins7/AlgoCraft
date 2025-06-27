using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using MathNet.Numerics;

namespace AlgoCraftV
{
    public static class ExtensionManager
    {
        public static void AssocierFichierAGC()
        {
            try
            {
                string extension = ".agc";
                string fileType = "AlgoCraftVFile";
                string description = "Fichier AlgoCraft";
                string cheminExe = Application.ExecutablePath;
                string cheminIcone = Path.ChangeExtension(cheminExe, ".ico");

                // Associe l'extension à un type
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + extension, "", fileType);

                // Donne une description au type
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + fileType, "", description);

                // Définir l’icône
                if (File.Exists(cheminIcone))
                    Registry.SetValue(@"HKEY_CLASSES_ROOT\" + fileType + @"\DefaultIcon", "", cheminIcone);
                else
                    Registry.SetValue(@"HKEY_CLASSES_ROOT\" + fileType + @"\DefaultIcon", "", cheminExe + ",0"); // Utilise l'icône contenue dans l'exe

                // Définir la commande d’ouverture
                string commande = "\"" + cheminExe + "\" \"%1\"";
                Registry.SetValue(@"HKEY_CLASSES_ROOT\" + fileType + @"\shell\open\command", "", commande);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'association de l'extension :\n" + ex.Message,
                                "Erreur registre", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
