using System;
using System.Windows.Forms;

namespace Statter
{
    public partial class StatDetailPanel : UserControl
    {
        public event Action<StatDetailPanel> StatDeleted;
        private void OnStatDeleted() { if (StatDeleted != null) StatDeleted(this); }

        public event Action<StatDetailPanel> StatModified;
        private void OnStatModified() { if (StatModified != null) StatModified(this); }

        private Stat _stat = null;
        public Stat Stat { get { return _stat; } }

        public StatDetailPanel(Stat stat)
        {
            InitializeComponent();

            _stat = stat;
        }

        private void StatDetails_Load(object sender, EventArgs e)
        {
            lblName.Text = _stat.Name;
            pnlColour.BackColor = _stat.Colour;
        }

        private void pnlColour_Click(object sender, EventArgs e)
        {
            dlgColour.Color = pnlColour.BackColor;
            if (dlgColour.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pnlColour.BackColor = dlgColour.Color;
                if (_stat.Colour != pnlColour.BackColor)
                {
                    _stat.Colour = pnlColour.BackColor;
                    OnStatModified();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            OnStatDeleted();
        }
    }
}
