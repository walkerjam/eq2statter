﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using Advanced_Combat_Tracker;

namespace Statter
{
    public partial class DlgViewStats : Form
    {
        protected List<EncounterStat> _stats = new List<EncounterStat>();
        private DateTime _start = DateTime.MinValue;
        private DateTime _end = DateTime.MinValue;
        private string _title = "";

        public DlgViewStats()
        {
            InitializeComponent();

            statGraph.ShowSteppedStatLines = PluginSettings.StepLines;
        }

        private void ViewStats_Load(object sender, EventArgs e)
        {
        }

        // When the user selects one or more stats in the datagrid, render a graph
        // for the selected stats
        private void dgStats_SelectionChanged(object sender, EventArgs e)
        {
            List<EncounterStat> selectedStats = new List<EncounterStat>();

            foreach (DataGridViewRow dgvr in dgStats.SelectedRows)
            {
                DataRowView drv = dgvr.DataBoundItem as DataRowView;
                if (drv != null && drv.Row != null && drv.Row.ItemArray.Length > 0)
                {
                    string statName = drv.Row.ItemArray[0].ToString();
                    EncounterStat encStat = _stats.Find((es) => { return es.Stat.Name.Equals(statName); });
                    if (encStat != null)
                        selectedStats.Add(encStat);
                }
            }

            if (selectedStats.Count > 0)
                statGraph.DrawStats(selectedStats, _start, _end);
        }

        public void ShowStats(List<Stat> stats, EncounterData encounterData)
        {
            ShowStats(stats, encounterData.StartTime, encounterData.EndTime, string.Format("{0} - {1}", encounterData.ZoneName, encounterData.Title));
        }

        public void ShowStats(List<Stat> stats, DateTime startTime, DateTime endTime, string title)
        {
            _start = startTime;
            _end = endTime;
            _title = title;

            // Iterate over the supplied stats and do some high-level calculations.
            // Add the stat into the local collection to be rendered later.
            _stats.Clear();
            foreach (Stat stat in stats)
            {
                EncounterStat encStat = new EncounterStat()
                {
                    Stat = stat,
                    Readings = stat.GetReadings(_start, _end)
                };

                foreach (StatReading reading in encStat.Readings)
                {
                    if (encStat.MinReading == null || reading.Value < encStat.MinReading.Value)
                        encStat.MinReading = reading;

                    if (encStat.MaxReading == null || reading.Value > encStat.MaxReading.Value)
                        encStat.MaxReading = reading;
                }

                _stats.Add(encStat);
            }

            ShowStatsTable();
        }

        private void ShowStatsTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Stat", typeof(string));
            dt.Columns.Add("Min", typeof(string));
            dt.Columns.Add("Max", typeof(string));

            if ((_end - _start).TotalSeconds > 1)
            {
                foreach (EncounterStat stat in _stats)
                {
                    DataRow dr = dt.Rows.Add(new object[] { 
                        stat.Stat.Name, 
                        stat.MinReading == null ? "" : stat.MinReading.Value.ToString("0"), 
                        stat.MaxReading == null ? "" : stat.MaxReading.Value.ToString("0"), 
                    });
                }

                Text = string.Format("Stats for {0:h':'mm':'ss tt} - {1:h':'mm':'ss tt} ({2})", _start, _end, _title);
                dgStats.DataSource = dt;
            }

            this.Show();
        }
    }
}
