using Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace EnergyMeter.Tests
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database _database;
        private readonly DispatcherTimer _timer;
        private double _counter = 0;
        public MainWindow()
        {
            InitializeComponent();
            _database = new Database(@"C:\ProgramData\Innovators\ServerData");
            var tableNames = _database.GetTableNames();
            _timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += onTimerTick;
        }

        private void onTimerTick(object sender, EventArgs e)
        {
            if(_counter >= 10) { _timer.Stop(); }
            var current = new Current(_counter, _counter * 2, _counter * 3);
            _database.CreateEmptyTable("Current", current.GetValues());
            _database.AddRecord("Current", current.GetValues());

            var power = new Power(_counter, _counter * 2, _counter * 3);
            _database.CreateEmptyTable("Power", power.GetValues());
            _database.AddRecord("Power", power.GetValues());

            _counter++;

            Debug.WriteLine($"Counter : {_counter}");
        }

        private void Serialise_Click(object sender, RoutedEventArgs e)
        {
            _counter = 0;
            _timer.Start();
        }

        private void DeSerialise_Click(object sender, RoutedEventArgs e)
        {
            var currentData = _database.ReadRecord("Current");
            var currentReadings = new List<Current>();
            foreach(var current in currentData)
            {
                var currentCharacteristic = new Current(current);
                currentReadings.Add(currentCharacteristic);
            }
            var currentChartPrimaryData = new ObservableCollection<DataPoint>();
            var currentChartSecondaryData = new ObservableCollection<DataPoint>();
            var currentChartTertiaryData = new ObservableCollection<DataPoint>();
            for (int i = 1; i <= currentReadings.Count; i++)
            {
                currentChartPrimaryData.Add(new DataPoint
                {
                    X = i,
                    Y = currentReadings[i - 1].Primary
                });
                currentChartSecondaryData.Add(new DataPoint
                {
                    X = i,
                    Y = currentReadings[i - 1].Secondary
                });
                currentChartTertiaryData.Add(new DataPoint
                {
                    X = i,
                    Y = currentReadings[i - 1].Tertiary
                });
            }


            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)currentChart.Series[0]).Title = "Primary";
            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)currentChart.Series[0]).ItemsSource = currentChartPrimaryData;

            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)currentChart.Series[1]).Title = "Secondary";
            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)currentChart.Series[1]).ItemsSource = currentChartSecondaryData;

            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)currentChart.Series[2]).Title = "Tertiary";
            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)currentChart.Series[2]).ItemsSource = currentChartTertiaryData;

            ((System.Windows.Controls.DataVisualization.Charting.LinearAxis)currentChart.ActualAxes[0]).Interval = 1;
            var powerData = _database.ReadRecord("Power");
            var powerReadings = new List<Power>();
            foreach (var power in powerData)
            {
                var powerCharacteristic = new Power(power);
                powerReadings.Add(powerCharacteristic);
            }
            var powerChartPrimaryData = new ObservableCollection<DataPoint>();
            var powerChartSecondaryData = new ObservableCollection<DataPoint>();
            var powerChartTertiaryData = new ObservableCollection<DataPoint>();
            for (int i = 1; i <= powerReadings.Count; i++)
            {
                powerChartPrimaryData.Add(new DataPoint
                {
                    X = i,
                    Y = powerReadings[i - 1].Primary
                });
                powerChartSecondaryData.Add(new DataPoint
                {
                    X = i,
                    Y = powerReadings[i - 1].Secondary
                });
                powerChartTertiaryData.Add(new DataPoint
                {
                    X = i,
                    Y = powerReadings[i - 1].Tertiary
                });
            }


            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)powerChart.Series[0]).Title = "Primary";
            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)powerChart.Series[0]).ItemsSource = powerChartPrimaryData;

            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)powerChart.Series[1]).Title = "Secondary";
            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)powerChart.Series[1]).ItemsSource = powerChartSecondaryData;

            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)powerChart.Series[2]).Title = "Tertiary";
            ((System.Windows.Controls.DataVisualization.Charting.LineSeries)powerChart.Series[2]).ItemsSource = powerChartTertiaryData;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            currentChart.Width = Width / 2;
            powerChart.Width = Width / 2;
        }
    }
    public class DataPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
