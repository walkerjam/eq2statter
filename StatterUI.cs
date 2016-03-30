using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Statter
{
    public partial class StatterUI : UserControl
    {
        // Limited the number of tracked stats, hard limit from /do_file_commands 
        // (16 lines per macro, with the first being the marker)
        public const int MAX_SELECTABLE_STATS = 15; 

        public event Action SelectedStatsChanged;
        protected void OnSelectedStatsChanged()
        {
            if (SelectedStatsChanged != null)
                SelectedStatsChanged();

            btnAdd.Enabled = _selectedStats.Count < MAX_SELECTABLE_STATS;
        }

        private List<string> _logLines = new List<string>();
        private DlgAddStat _dlgAddStat = new DlgAddStat();
        private List<Stat> _selectedStats = new List<Stat>();

        // Keep a list of suggested colours to rotate through
        private Color[] _cStatColours =
        {
            Color.FromArgb(255, 104, 164, 98),
            Color.FromArgb(255, 242, 175, 88),
            Color.FromArgb(255, 53, 108, 154),
            Color.FromArgb(255, 209, 65, 63),
            Color.FromArgb(255, 134, 81, 137),
            Color.FromArgb(255, 143, 146, 145),
            Color.FromArgb(255, 76, 125, 168),
            Color.FromArgb(255, 121, 176, 114),
            Color.FromArgb(255, 244, 186, 110),
            Color.FromArgb(255, 219, 88, 87),
            Color.FromArgb(255, 105, 105, 105)
        };

        public StatterUI()
        {
            InitializeComponent();
        }

        private void StatterUI_Load(object sender, EventArgs e)
        {
            ShowInstructions();

            ShowLogContainer();

            chkParseOnImport.Checked = PluginSettings.ParseOnImport;
            chkSteppedLines.Checked = PluginSettings.StepLines;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            _dlgAddStat.SetUsedStats(GetUsedStatNames());
            _dlgAddStat.SetColour(_cStatColours[_selectedStats.Count % _cStatColours.Length]);

            _dlgAddStat.Location = btnAdd.PointToScreen(btnAdd.Location);
            if (_dlgAddStat.ShowDialog(this) == DialogResult.OK)
            {
                Stat newStat = _dlgAddStat.AddedStat;
                AddSelectedStat(newStat);
                OnSelectedStatsChanged();
            }
        }

        private void statDetails_StatDeleted(StatDetailPanel statDetailPanel)
        {
            RemoveSelectedStat(statDetailPanel);
            OnSelectedStatsChanged();
        }

        private void statDetails_StatModified(StatDetailPanel statDetailPanel)
        {
            OnSelectedStatsChanged();
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            _logLines.Clear();
            ShowLog();
        }

        [Conditional("DEBUG")]
        private void ShowLogContainer()
        {
            grpLog.Visible = true;
        }

        protected void ShowInstructions()
        {
            txtInstructions.Rtf = string.Format(
@"{{\rtf1\ansi\f0\pard
{{\b Instructions}}\par
\par
Create a macro that calls {{\i /do_file_commands {0}}} and bind this to a hotkey you naturally use often during combat.\par
\par
To view your stats, right-click an encounter listed in the encounter tree (note that this can also include the zone-wide {{\i ""All""}} encounter) and select {{\i View Encounter Stats}}. Doing so will open a window showing the minimum and maximum recorded values for each stat during the selected encounter. Clicking on one or more stat rows will display a graph of each selected stat over the course of the encounter. Hovering over the graph will show instantaneous values and times.\par
\par
Using this macro will spam up your chat window (using the chat category ""Command""), so you may want to redirect the output to a chat window that does not contain any other useful info.\par
\par
Finally, note that /do_file_commands currently limits the number of stats that can be tracked to {1}.\par
}}", StatterPlugin.MACRO_FILENAME, MAX_SELECTABLE_STATS);
        }

        public List<Stat> GetSelectedStats()
        {
            return _selectedStats;
        }

        public void SetSelectedStats(List<Stat> selectedStats)
        {
            _selectedStats.Clear();
            pnlStatDetails.Controls.Clear();

            foreach (Stat stat in selectedStats)
                AddSelectedStat(stat);

            btnAdd.Enabled = _selectedStats.Count < MAX_SELECTABLE_STATS;
        }

        private void AddSelectedStat(Stat stat)
        {
            _selectedStats.Add(stat);

            StatDetailPanel statDetails = new StatDetailPanel(stat);
            statDetails.StatDeleted += new Action<StatDetailPanel>(statDetails_StatDeleted);
            statDetails.StatModified += new Action<StatDetailPanel>(statDetails_StatModified);
            pnlStatDetails.Controls.Add(statDetails);
        }

        private void RemoveSelectedStat(StatDetailPanel statDetailPanel)
        {
            _selectedStats.Remove(statDetailPanel.Stat);
            pnlStatDetails.Controls.Remove(statDetailPanel);
        }

        private List<string> GetUsedStatNames()
        {
            List<string> usedStatNames = new List<string>();
            foreach (Stat stat in _selectedStats)
                usedStatNames.Add(stat.Name);
            return usedStatNames;
        }

        public void AddLogLine(string logLine)
        {
            _logLines.Add(logLine);
            ShowLog();
        }

        private delegate void ShowLogCallback();
        private void ShowLog()
        {
            if (this.InvokeRequired)
            {
                ShowLogCallback callback = new ShowLogCallback(ShowLog);
                this.Invoke(callback);
            }
            else
            {
                txtLog.Lines = _logLines.ToArray();
                txtLog.SelectionStart = txtLog.TextLength;
                txtLog.ScrollToCaret();
            }
        }

        private void btnTestGraph_Click(object sender, EventArgs e)
        {
            List<Stat> stats = new List<Stat>();
            stats.Add(new Stat("Fervor"));
            stats.Add(new Stat("Accuracy"));
            stats.Add(new Stat("Hate Mod"));

            stats[0].Colour = Color.Red;
            stats[1].Colour = Color.Green;
            stats[2].Colour = Color.Blue;

            int durationSeconds = 10;
            DateTime end = DateTime.Now;
            DateTime start = end.AddSeconds(-durationSeconds);
            for (int i = 0; i < durationSeconds; i++)
            {
                stats[0].ParseReading((i * 1).ToString(), start.AddSeconds(i));
                stats[1].ParseReading((i * 1.5).ToString(), start.AddSeconds(i));
                stats[2].ParseReading((i * 2).ToString(), start.AddSeconds(i));
            }

            DlgViewStats dlgViewStats = new DlgViewStats();
            dlgViewStats.ShowStats(stats, start, end, "Test");
        }

        private void chkParseOnImport_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettings.ParseOnImport = chkParseOnImport.Checked;
        }

        private void chkSteppedLines_CheckedChanged(object sender, EventArgs e)
        {
            PluginSettings.StepLines = chkSteppedLines.Checked;
        }
    }
}
