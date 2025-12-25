using System.Collections.ObjectModel;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.Viewers.Models;
using Avalonia.Media;

namespace Arcraven.Avalonia.Viewers.ViewModels;

public class GraphViewModel : ObservableObject
    {
        private ObservableCollection<GraphSeries> _series = new();
        public ObservableCollection<GraphSeries> Series 
        { 
            get => _series; 
            set => Set(ref _series, value); 
        }

        private Func<double, string> _xLabelFormatter;
        public Func<double, string> XLabelFormatter 
        { 
            get => _xLabelFormatter; 
            set => Set(ref _xLabelFormatter, value); 
        }

        private Func<double, string> _yLabelFormatter;
        public Func<double, string> YLabelFormatter
        {
            get => _yLabelFormatter; 
            set => Set(ref _yLabelFormatter, value); 
        }
        
        private double? _xAxisMaxRange;
        public double? XAxisMaxRange
        {
            get => _xAxisMaxRange;
            set => Set(ref _xAxisMaxRange, value);
        }

        public GraphViewModel()
        {
            // Default Formatter
            YLabelFormatter = val => $"${val:N0}";
            XLabelFormatter = val => val.ToString();
        }

        /// <summary>
        /// GENERIC METHOD: Converts any data type into renderable points
        /// </summary>
        public void SetData<TX, TY>(IEnumerable<(TX x, TY y)> data, Color color, string title)
        {
            var newSeries = new GraphSeries
            {
                Title = title,
                Color = color,
                Points = new List<GraphPoint>()
            };

            foreach (var item in data)
            {
                newSeries.Points.Add(new GraphPoint
                {
                    X = ConvertX(item.x),
                    Y = Convert.ToDouble(item.y),
                    OriginalValue = item
                });
            }

            Series.Clear();
            Series.Add(newSeries);
        }

        // Helper to handle X-Axis Types
        private double ConvertX(object value)
        {
            return value switch
            {
                DateTime dt => dt.Ticks,
                int i => i,
                double d => d,
                string s => 0, // Strings require a Category/Index map (omitted for brevity, usually you pass index)
                _ => 0
            };
        }

        public void SetupTimeAxis()
        {
            XLabelFormatter = val => new DateTime((long)val).ToString("MMM dd");
        }
    }