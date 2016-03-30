using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace Statter
{
    public partial class DlgAddStat : Form
    {
        protected Stat _addedStat = null;
        public Stat AddedStat { get { return _addedStat; } }

        public DlgAddStat()
        {
            InitializeComponent();
        }

        public void SetUsedStats(List<string> usedStats)
        {
            cmbStat.BeginUpdate();
            cmbStat.Items.Clear();
            foreach (string stat in Stat.GetAvailableStats(usedStats))
                cmbStat.Items.Add(stat);
            cmbStat.EndUpdate();

            if (cmbStat.Items.Count > 0)
                cmbStat.SelectedIndex = 0;
        }

        public void SetColour(Color colour)
        {
            dlgColour.Color = colour;
            pnlColour.BackColor = colour;
        }

        private void pnlColour_Click(object sender, EventArgs e)
        {
            dlgColour.Color = pnlColour.BackColor;
            if (dlgColour.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                pnlColour.BackColor = dlgColour.Color;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            if (cmbStat.SelectedItem == null) return;

            _addedStat = new Stat(cmbStat.SelectedItem.ToString()) { Colour = pnlColour.BackColor };
            DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
