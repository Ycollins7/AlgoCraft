using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MathNet.Numerics;

namespace AlgoCraftV
{
    public partial class Form1 : Form
    {
        private bool themeSombre = false;
        private Interpreteur interpreteur;

        private bool drag = false;
        private Point startPoint = new Point(0, 0);

        private Console console;
        // Instance de la classe Algo pour gérer le contenu courant
        private Algo monAlgo; 

        public Form1()
        {
            InitializeComponent();
            monAlgo = new Algo();  // Crée un nouvel objet Algo 
            rtbCode.ScrollBars = RichTextBoxScrollBars.Vertical;

            #region Démarrage du logiciel
                //Affichage des panels au lancement du logiciel selon l'ordre choisis par moi.
                panelPrincipale.Visible = false;
                panelStartPage.Visible = true;
                panelCréationProjet.Visible = false;
            #endregion
        }

        #region Gère le responsive sur tous les écrans
            private void panelEntete_MouseDown(object sender, MouseEventArgs e)
            {
                drag = true;
                startPoint = new Point(e.X, e.Y);
            }
            private void panelEntete_MouseUp(object sender, MouseEventArgs e)
            {
                drag = false;
            }
            private void panelEntete_MouseMove(object sender, MouseEventArgs e)
            {
                if (drag)
                {
                    Point p = PointToScreen(e.Location);
                    this.Location = new Point(p.X - startPoint.X, p.Y - startPoint.Y);
                }
            }
            private void panelEntete_MouseDoubleClick(object sender, MouseEventArgs e)
            {
                if (this.WindowState == FormWindowState.Normal)
                    this.WindowState = FormWindowState.Maximized;
                else
                    this.WindowState = FormWindowState.Normal;
            }
        #endregion
        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x00020000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            #region Responsive
                //this.FormBorderStyle = FormBorderStyle.None;
                //btnFermer.Anchor = AnchorStyles.Right;
                //btnCadrer.Anchor = AnchorStyles.Right;
                //btnReduire.Anchor = AnchorStyles.Right;

                //rtbCode.Dock = DockStyle.Bottom;
                //rtbCode.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                //panelBoutons.Dock = DockStyle.Bottom;
                //panelBoutons.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
                //rtbErreur.Dock = DockStyle.Bottom;
                //rtbErreur.Anchor =  AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                //btnTest.Dock = DockStyle.Bottom;
                //btnTest.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;

                //menuStrip1.Dock = DockStyle.Top;
                //menuStrip1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                //panelPremier.Dock = DockStyle.Fill;
                //panelPremier.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom |AnchorStyles.Top;
                //panelPrincipale.Dock = DockStyle.Fill;
                //panelPrincipale.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
                //panelEntete.Dock = DockStyle.Fill;
                //panelEntete.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                //txtApropos.Dock = DockStyle.Fill;
                //txtApropos.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            #endregion

            // Syntaxe de base à afficher au lancement
            string syntaxeInitiale = "ALGORITHME" + Environment.NewLine + "VAR" + Environment.NewLine + "DEBUT" + Environment.NewLine + Environment.NewLine + "FIN";

            // On remplit la zone de texte
            rtbCode.Text = syntaxeInitiale;

            // On l'enregistre aussi dans l'objet Algo
            monAlgo.Contenu = syntaxeInitiale;

            console = new Console();
            // console.Show();

            console = new Console();
            interpreteur = new Interpreteur(console, rtbErreur);

            panelBoutons.Visible = true;
            panelFonction.Visible = false;
            panelProcedure.Visible = false;
        }

        #region Bouton Réduire, Agrandir et Fermer
            private void btnFermer_Click(object sender, EventArgs e)
            {
                DialogResult result = MessageBox.Show("Voulez-vous vraiment quitter l'application ?", "Quitter", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result== DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            private void btnReduire_Click(object sender, EventArgs e)
            {
                this.WindowState = FormWindowState.Minimized;
            }
            private void btnCadrer_Click(object sender, EventArgs e)
            {
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                }
                else
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }
        #endregion

        #region Quitter, Nouveau, Ouvrir
            private void quitterToolStripMenuItem_Click(object sender, EventArgs e)
            {
                DialogResult result = MessageBox.Show("Voulez-vous vraiment quitter l'application ?", "Quitter", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            private void nouveauToolStripMenuItem_Click(object sender, EventArgs e)
            {
                rtbCode.Clear();
            }
            private void ouvrirToolStripMenuItem_Click(object sender, EventArgs e)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Fichiers AlgoCraft (*.agc)|*.agc";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Title = "Ouvrir un fichier AlgoCraft";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    panelCréationProjet.Visible = true;
                    panelPrincipale.Visible = true;
                    panelStartPage.Visible = true;

                    try
                    {
                        string contenuCode = File.ReadAllText(openFileDialog.FileName);
                        rtbCode.Text = contenuCode;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Une erreur s'est produite lors du chargement du fichier : " + ex.Message,
                            "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        #endregion

        #region Ajouter du code
            private void AjouterDansTexte(string texte)//dans la zone de texte
            {
                rtbCode.AppendText(texte + Environment.NewLine);
            }
            private void rtbCode_TextChanged(object sender, EventArgs e)
            {
                panelLignes.Invalidate();
                if (!modificationAutomatique)
                {
                    historiqueAnnuler.Push(rtbCode.Text);
                    historiqueRetablir.Clear(); // on vide le redo après chaque nouvelle saisie
                }

                int selectionStart = rtbCode.SelectionStart;
                int selectionLength = rtbCode.SelectionLength;

                // Sauvegarder le texte
                string texte = rtbCode.Text;

                // Liste des mots-clés
                string[] motsCles = { "ALGORITHME", "VAR", "Const", "DEBUT", "FIN", "Ecrire", "Lire", "Si", "Alors", "Sinon", "FinSi", "Pour", "FinPour", "TantQue", "FinTantQue", "Repeter", "Jusqu'à", "←", "Alors","ET","OU", "PROCEDURE", "FONCTION", "RETOURNER" };

                // Désactiver le redraw (meilleure perf)
                rtbCode.SuspendLayout();

                // Enlever la mise en forme
                rtbCode.SelectAll();
                rtbCode.SelectionColor = Color.Black;

                // Appliquer la couleur aux mots-clés
                foreach (string mot in motsCles)
                {
                    MatchCollection matches = Regex.Matches(texte, $@"\b{mot}\b");
                    foreach (Match m in matches)
                    {
                        rtbCode.Select(m.Index, m.Length);
                        rtbCode.SelectionColor = Color.Blue;
                    }
                }

                // Rétablir la sélection originale
                rtbCode.Select(selectionStart, selectionLength);
                rtbCode.SelectionColor = Color.Black;

                // Réactiver le redraw
                rtbCode.ResumeLayout();
            }
        #endregion

        #region Bouton pour panelBoutons
            private void btnEcrire_Click(object sender, EventArgs e)
            {
                string code = "Ecrire (\" \");\n";
                rtbCode.SelectedText = code;
            }
            private void btnLire_Click(object sender, EventArgs e)
            {
                string code = "Lire ( variable );\n";
                rtbCode.SelectedText = code;
            }
            private void btnSi_Click(object sender, EventArgs e)
            {
                string code = "Si condition Alors\n    \nFinSi\n";
                rtbCode.SelectedText = code;
            }
            private void btnSi_Sinon_Click(object sender, EventArgs e)
            {
                string code = "Si condition Alors\nSinon    \nFinSi\n";
                rtbCode.SelectedText = code;
            }
            private void btnPour_Click(object sender, EventArgs e)
            {
                 string code = "Pour i allant de 1 à n Faire\n    \nFinPour\n";
                 rtbCode.SelectedText = code;
            }
            private void btnTanque_Click(object sender, EventArgs e)
            {
                string code = "TantQue condition Faire\n    \nFinTantQue\n";
                rtbCode.SelectedText = code;
            }
            private void btnRepeter_Click(object sender, EventArgs e)
            {
                string code = "Repeter  \n    \nJusqu'à condition";
                rtbCode.SelectedText = code;
            }
            private void btnCommentaire_Click(object sender, EventArgs e)
            {
                // Trouver le début et la fin de la ligne courante
                int debutLigne = rtbCode.GetFirstCharIndexOfCurrentLine();
                int finLigne = rtbCode.Text.IndexOf('\n', debutLigne);
                if (finLigne == -1) finLigne = rtbCode.Text.Length;

                // Insérer "// " au début de la ligne
                rtbCode.Text = rtbCode.Text.Insert(debutLigne, "// ");

                // Appliquer la couleur verte à la partie commentée
                rtbCode.Select(debutLigne, rtbCode.GetFirstCharIndexOfCurrentLine() + 3);
                rtbCode.SelectionColor = Color.Green;

                // Remettre le curseur juste après le commentaire ajouté
                rtbCode.SelectionStart = debutLigne + 3;
                rtbCode.SelectionLength = 0;
                rtbCode.Focus();
            }
            private void btnAffectation_Click(object sender, EventArgs e)
            {
                string code = " ← ";
                rtbCode.SelectedText = code;
            }
        #endregion

        #region Exécution du code(Algorithme)
            private async void btnTest_Click(object sender, EventArgs e)
            {
                if (console == null || console.IsDisposed)
                {
                    console = new Console();
                    interpreteur = new Interpreteur(console, rtbErreur); // On reconnecte la nouvelle console à l'interpréteur
                }

                console.Show();
                console.BringToFront(); // Pour la faire apparaître au-dessus
                try
                {
                    await interpreteur.ExecuterAsync(rtbCode.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("❌ Une erreur s’est produite :\n" + ex.Message);
                }
            }
            private void btnTester_Click(object sender, EventArgs e)
        {
            rtbErreur.Clear();
            var lignes = rtbCode.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var ligne in lignes)
            {
                if (!string.IsNullOrWhiteSpace(ligne))
                {
                    rtbErreur.AppendText("→ " + ligne + Environment.NewLine);
                    System.Threading.Thread.Sleep(500); // simulation pas à pas
                    Application.DoEvents();
                }
            }
        }
            private void compilerToolStripMenuItem_Click(object sender, EventArgs e)
            {
                
            }
        #endregion

        private void exporterLeCodeVersUnFichierTexteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Fichier texte (*.txt)|*.txt";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, rtbCode.Text);
                MessageBox.Show("Code exporté avec succès !");
            }
        }
        private void ExecuterCode()
        {
            if (console == null || console.IsDisposed)
                console = new Console();

            console.Clear(); // Nettoie la console
            console.Show();

            string[] lignes = rtbCode.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (string ligne in lignes)
            {
                string trimmed = ligne.Trim();

                // On exécute uniquement les lignes valides
                if (trimmed.StartsWith("Ecrire"))
                {
                    Match match = Regex.Match(trimmed, @"Ecrire\s*\(\s*""(.*?)""\s*\)");
                    if (match.Success)
                    {
                        string message = match.Groups[1].Value;
                        console.AfficherLigne(message); // affiche uniquement le texte dans la console
                    }
                }

                // Tu pourras plus tard ajouter ici: Lire, Si, etc.
            }
        }
        private Stack<string> historiqueAnnuler = new Stack<string>();
        private Stack<string> historiqueRetablir = new Stack<string>();
        private bool modificationAutomatique = false;

        #region Bouton avec Image
            private void btnNouveau_Click(object sender, EventArgs e)
        {
            rtbCode.Clear();
        }
            private void btnOuvrir_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Fichiers texte (*.txt)|*.txt|Tous les fichiers (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string contenuCode = File.ReadAllText(openFileDialog.FileName);
                    rtbCode.Text = contenuCode;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur s'est produite lors du chargement du fichier : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
            private void btnEnregistrer_Click(object sender, EventArgs e)
        {

        }
            private void btnretour_Click(object sender, EventArgs e)
        {
            Annuler();
        }
            private void btnRetablir_Click(object sender, EventArgs e)
        {
            Retablir();
        }
            private void Annuler()
        {
            if (historiqueAnnuler.Count > 1)
            {
                modificationAutomatique = true;

                string etatActuel = historiqueAnnuler.Pop();
                historiqueRetablir.Push(etatActuel);

                rtbCode.Text = historiqueAnnuler.Peek();
                rtbCode.SelectionStart = rtbCode.Text.Length;

                modificationAutomatique = false;
            }
        }
            private void Retablir()
        {
            if (historiqueRetablir.Count > 0)
            {
                modificationAutomatique = true;

                string prochain = historiqueRetablir.Pop();
                historiqueAnnuler.Push(prochain);

                rtbCode.Text = prochain;
                rtbCode.SelectionStart = rtbCode.Text.Length;

                modificationAutomatique = false;
            }
        }
        #endregion

        private void rtbCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                Annuler();
             //   e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                Retablir();
                e.SuppressKeyPress = true;
            }
        }
        #region panel pour incrementer chaque ligne de code
        private void panelLignes_Paint(object sender, PaintEventArgs e)
        {
            int firstCharIndex = rtbCode.GetCharIndexFromPosition(new Point(0, 0));
            int firstLineIndex = rtbCode.GetLineFromCharIndex(firstCharIndex);

            int currentY = 0;
            int lineHeight = TextRenderer.MeasureText("A", rtbCode.Font).Height;

            int totalLines = rtbCode.Lines.Length;

            for (int i = firstLineIndex; i < totalLines; i++)
            {
                int charIndex = rtbCode.GetFirstCharIndexFromLine(i);
                Point position = rtbCode.GetPositionFromCharIndex(charIndex);

                currentY = position.Y;

                // Empêche d'afficher en dehors du Panel
                if (currentY > panelLignes.Height)
                    break;

                string lineNumber = (i + 1).ToString();
                e.Graphics.DrawString(lineNumber, rtbCode.Font, Brushes.Lime, new PointF(panelLignes.Width - 25, currentY));
            }
        }
        private void rtbCode_Resize(object sender, EventArgs e)
        {
            panelLignes.Invalidate();
        }

        private void rtbCode_VScroll(object sender, EventArgs e)
        {
            panelLignes.Invalidate();
        }

        public void ChargerContenuDepuisFichier(string contenu)
        {
            rtbCode.Text = contenu; // Ou le nom exact de ton RichTextBox
        }
        #endregion
        private void enregistrerSousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNomProjetAffichage.Text))
            {
                MessageBox.Show("Veuillez entrer le nom du projet.");
            }
            else
            {
                string dateEmission = DateTime.Now.ToString("dd-MM-yyyy");
                string nomFichier = "Algo_" + txtNomProjetAffichage.Text + "_" + dateEmission;
                int numSequence = 1;
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Fichier AlgoCraft (*.agc)|*.agc";
                saveDialog.DefaultExt = "agc";
                saveDialog.AddExtension = true;
                saveDialog.Title = "Enregistrer un algorithme";
                saveDialog.FileName = nomFichier + "_" + numSequence;

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string cheminFichier = saveDialog.FileName;

                    // Vérifie si le fichier existe et incrémente si besoin
                    while (File.Exists(cheminFichier))
                    {
                        numSequence++;
                        string nouveauNom = nomFichier + "_" + numSequence + ".agc";
                        cheminFichier = Path.Combine(Path.GetDirectoryName(cheminFichier), nouveauNom);
                    }

                    string contenuCode = rtbCode.Text;
                    File.WriteAllText(cheminFichier, contenuCode);

                    MessageBox.Show("L'algorithme a été enregistré dans : " + cheminFichier,
                        "Fichier enregistré", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void btnProced_Click(object sender, EventArgs e)
        {

            string procedureCode =
            @"PROCEDURE NomProcedure()
            DEBUT
                // instructions
            FIN
            ";
            // Insère le code à la position du curseur
            int pos = rtbCode.SelectionStart;
            rtbCode.Text = rtbCode.Text.Insert(pos, procedureCode);
            rtbCode.SelectionStart = pos + procedureCode.Length; // Place le curseur après l'insertion
            rtbCode.Focus();
        }
        private void btnfnx_Click(object sender, EventArgs e)
        {
            string fonctionCode =
                @"FONCTION NomFonction() : TypeRetour 
                DEBUT  
                     // instructions 
                RETOURNER valeur 
                FIN";
            int pos = rtbCode.SelectionStart;
            rtbCode.Text = rtbCode.Text.Insert(pos, fonctionCode);
            rtbCode.SelectionStart = pos + fonctionCode.Length;
            rtbCode.Focus();
        }

        #region Panel des Boucles
        private void btnboucles_Click(object sender, EventArgs e)
        {
            
            panelFonction.Visible = false;
            panelProcedure.Visible = false;
            panelBoutons.Visible = true;
        }
        #endregion

        #region Panel des Procédures et fonctions
        private void btnProcedure_Click(object sender, EventArgs e)
        {
            
            panelFonction.Visible = false;
            panelBoutons.Visible = false;
            panelProcedure.Visible = true;
        }
        #endregion

        #region Panel des Fonctions Mathématiques
        private void btnFonction_Click(object sender, EventArgs e)
        {
            panelFonction.Visible = true;
            panelBoutons.Visible = false;
            panelProcedure.Visible = false;
        }
        #endregion

        #region Fonctionnalités des panel de la gestion du logiciel.

        #region panelStartPage (Ouverture du panel de la création du projet) 
        private void btnStartNouveauProjet_Click(object sender, EventArgs e)
            {
                panelCréationProjet.Visible = true;
            }
            #endregion

            #region panelCréationProjet (Création proprement dite(Nom du Projet, Emplacement, Description) et Annulation(Retour vers Start Page)
                private void btnCréationCréer_Click(object sender, EventArgs e)
                {
                    // 1. Récupérer le nom du projet saisi
                    string nomProjet = txtCréationNomProjet.Text.Trim();

                    // Vérifier que le champ n'est pas vide
                    if (string.IsNullOrEmpty(nomProjet))
                    {
                        MessageBox.Show("Veuillez entrer un nom de projet.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 2. Masquer le panel de création et afficher le panel principal
                    panelCréationProjet.Visible = true;
                    panelPrincipale.Visible = true;
                    panelStartPage.Visible = true;

                    // 3. Mettre à jour le TextBox dans le panel principal
                    txtNomProjetAffichage.Text = nomProjet;
                }
                private void btnCréationParcourir_Click(object sender, EventArgs e)
                {
                    using (FolderBrowserDialog dossierDialogue = new FolderBrowserDialog())
                    {
                        dossierDialogue.Description = "Choisissez l'emplacement du projet";
                        dossierDialogue.ShowNewFolderButton = true;

                        DialogResult resultat = dossierDialogue.ShowDialog();

                        if (resultat == DialogResult.OK && !string.IsNullOrWhiteSpace(dossierDialogue.SelectedPath))
                        {
                            txtCréationCheminProjet.Text = dossierDialogue.SelectedPath;
                        }
                    }
                }
                private void btnCréationAnnuler_Click(object sender, EventArgs e)
                {
                    //Efface les éléments des champs.
                    txtCréationCheminProjet.Text = string.Empty;
                    txtCréationNomProjet.Text = string.Empty;

                    //Bascule vers la page de Lancement (Start Page)
                    panelStartPage.Visible = true;
                    panelCréationProjet.Visible = false;
                }
        #endregion

        #endregion

        #region Thème du logiciel
            #region Activation
                private void activerToolStripMenuItem_Click(object sender, EventArgs e)
                {
                    this.BackColor = Color.FromArgb(30, 30, 30);
                    panelPremier.BackColor = Color.FromArgb(45, 45, 45);
                    panelPrincipale.BackColor = Color.FromArgb(30, 30, 30);
                    panelNomProjet.BackColor = Color.FromArgb(40, 40, 40);
                    panelEntete.BackColor = Color.FromArgb(25, 25, 25);
                    panelBoutons.BackColor = Color.FromArgb(50, 50, 50);
                    panelProcedure.BackColor = Color.FromArgb(50, 50, 50);
                    panelFonction.BackColor = Color.FromArgb(50, 50, 50);

                    rtbCode.BackColor = Color.FromArgb(40, 40, 40);
                    rtbCode.ForeColor = Color.White;
                    rtbErreur.BackColor = Color.FromArgb(40, 40, 40);
                    rtbErreur.ForeColor = Color.White;
                    txtApropos.BackColor = Color.FromArgb(40, 40, 40);
                    txtApropos.ForeColor = Color.White;
                    txtNomProjetAffichage.BackColor = Color.FromArgb(40, 40, 40);
                    txtNomProjetAffichage.ForeColor = Color.White;
                    label1.ForeColor = Color.White;
                    label2.ForeColor = Color.White;
                    label3.ForeColor = Color.White;
                    rtbCode.ForeColor = Color.Aqua;

                    menuStrip1.BackColor = Color.FromArgb(45, 45, 45);
                    menuStrip1.ForeColor = Color.White;

                    themeSombre = true;

                    AppliquerThemeSurLesBoutons(true);
                }
        #endregion
            #region Désactivation
                private void désactiverToolStripMenuItem_Click(object sender, EventArgs e)
                {
                    // Réinitialiser
                    this.BackColor = SystemColors.Control;
                    panelPremier.BackColor = SystemColors.ControlLight;
                    panelPrincipale.BackColor = Color.FromArgb(3, 9, 39);
                    panelNomProjet.BackColor = SystemColors.ControlLight;
                    panelEntete.BackColor = SystemColors.Control;
                    panelBoutons.BackColor = SystemColors.Info;
                    panelFonction.BackColor = SystemColors.Info;
                    panelProcedure.BackColor = SystemColors.Info;

                    rtbCode.BackColor = Color.White;
                    rtbCode.ForeColor = Color.Black;
                    rtbErreur.BackColor = SystemColors.Info;
                    rtbErreur.ForeColor = Color.Black;
                    txtApropos.BackColor = Color.White;
                    txtApropos.ForeColor = Color.Black;
                    txtNomProjetAffichage.BackColor = Color.White;
                    txtNomProjetAffichage.ForeColor = Color.Black;
                    label1.ForeColor = Color.Black;
                    label2.ForeColor = Color.Black;
                    label3.ForeColor = Color.Black;

                    menuStrip1.BackColor = SystemColors.Control;
                    menuStrip1.ForeColor = Color.Black;

                    themeSombre = false;

                    AppliquerThemeSurLesBoutons(false);
                }
        #endregion
            #region Application du thème
                private void AppliquerThemeSurLesBoutons(bool sombre)
                {
                    Color fond = sombre ? Color.FromArgb(40, 40, 40) : Color.FromArgb(3, 9, 39);
                    Color texte = Color.White;

                    foreach (var ctrl in panelBoutons.Controls)
                    {
                        Button btn = ctrl as Button;
                        if (btn != null)
                        {
                            btn.BackColor = fond;
                            btn.ForeColor = texte;
                        }
                    }
                }
        #endregion
        #endregion

        #region Basculer vers le site en cliquant sur Aide
            private void aideToolStripMenuItem1_Click(object sender, EventArgs e)
            {
                try
                {
                    // Méthode recommandée avec Windows 10 ou plus
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://blvcknesss.github.io/AlgoCraft.io/",
                        UseShellExecute = true // important pour ouvrir avec navigateur par défaut
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur : " + ex.Message);
                }
            }
        #endregion
    }
}
