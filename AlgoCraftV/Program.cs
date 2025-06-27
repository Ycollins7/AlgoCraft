using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlgoCraftV
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ExtensionManager.AssocierFichierAGC();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Form1 mainForm = new Form1();
            if (args.Length > 0 && File.Exists(args[0]))
            {
                string contenu = File.ReadAllText(args[0]);
                mainForm.ChargerContenuDepuisFichier(contenu);
                MessageBox.Show("Fichier AlgoCraft chargé avec succès :\n" + args[0], "Chargement réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Application.Run(new Form1());
        }
    }
}
