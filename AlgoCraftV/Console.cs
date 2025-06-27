using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using MathNet.Numerics;

namespace AlgoCraftV
{
    public partial class Console : Form
    {
        public TaskCompletionSource<string> inputTCS;

        public Console()
        {
            InitializeComponent();

            // Assure-toi que l'événement KeyDown est bien relié au TextBox
            txtInput.KeyDown += TxtInput_KeyDown;
            txtInput.ReadOnly = true;
        }

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && inputTCS != null)
            {
                inputTCS.TrySetResult(txtInput.Text);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        public async Task<string> LireDepuisConsole(string variable)
        {
            //AfficherLigne($"⏳ Entrez une valeur pour {variable} :", Color.Orange);
            inputTCS = new TaskCompletionSource<string>();

            txtInput.ReadOnly = false;
            txtInput.Focus();

            string valeur = await inputTCS.Task;

            txtInput.Text = "";
            txtInput.ReadOnly = true;

            return valeur;
        }

        public void AfficherLigne(string texte, Color? couleur = null)
        {
            rtbConsole.SelectionStart = rtbConsole.TextLength;
            rtbConsole.SelectionLength = 0;

            rtbConsole.SelectionColor = couleur ?? Color.Lime;
            rtbConsole.AppendText(texte + Environment.NewLine);
            rtbConsole.SelectionColor = rtbConsole.ForeColor;
            rtbConsole.ScrollToCaret();
        }

        public void Clear()
        {
            rtbConsole.Clear();
        }
    }
}
