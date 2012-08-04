using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BlurayAutoPlay
{
    public partial class MainForm : Form
    {
        private AboutBox aboutBox = new AboutBox();

        private AutoplayController autoplayController = new AutoplayController();
        private List<IMediaPlayer> installedMediaPlayers;

        private string savedMediaPlayerName = "";
        private string curSelectedMediaPlayerName
        {
            get { return mediaPlayersComboBox.SelectedItem == null ? "" : mediaPlayersComboBox.SelectedItem.ToString(); }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            CancelButton = cancelButton;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ReloadMediaPlayers();
            ResetButtons();
        }

        private void ReloadMediaPlayers()
        {
            installedMediaPlayers = autoplayController.GetInstalledMediaPlayers();

            mediaPlayersComboBox.Items.Clear();
            mediaPlayersComboBox.Items.Add("");

            foreach (var mediaPlayer in installedMediaPlayers)
            {
                mediaPlayersComboBox.Items.Add(mediaPlayer.GetName());
            }

            savedMediaPlayerName = autoplayController.GetSavedMediaPlayerName();
            savedMediaPlayerName = savedMediaPlayerName == null ? "" : savedMediaPlayerName;

            int savedMediaPlayerIndex = mediaPlayersComboBox.FindStringExact(savedMediaPlayerName);

            if (savedMediaPlayerIndex > -1)
            {
                mediaPlayersComboBox.SelectedIndex = savedMediaPlayerIndex;
            }
        }

        private void ResetButtons()
        {
            if (HasChanged())
            {
                saveButton.Enabled = false;
                saveToolStripMenuItem.Enabled = false;
                cancelButton.Text = "Close";
            }
            else
            {
                saveButton.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                cancelButton.Text = "Cancel";
            }
        }

        private bool HasChanged()
        {
            return curSelectedMediaPlayerName.Equals(savedMediaPlayerName);
        }

        private void Save()
        {
            if (saveButton.Enabled)
            {
                autoplayController.SetAutoplay(curSelectedMediaPlayerName);
                savedMediaPlayerName = curSelectedMediaPlayerName;
                ResetButtons();
            }
        }

        private void mediaPlayersComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResetButtons();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MainForm_Load(null, null);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            aboutBox.ShowDialog();
        }
    }
}
