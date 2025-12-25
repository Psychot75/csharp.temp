using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using Arcraven.Avalonia.Viewers.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Arcraven.Avalonia.Viewers.Controls;

/// <summary>
/// A custom control that renders a line graph with zoom, pan, and auto-follow capabilities.
/// </summary>
public class GraphControl : Control
{
    private const double GraphPadding = 40.0;
    private const double ScrollbarHeight = 4.0;
    private const double ZoomStep = 1.1;
    private const double MaxZoomScale = 50.0;
    private const double MinZoomScale = 1.0;

    public static readonly StyledProperty<IEnumerable<GraphSeries>> SeriesProperty =
        AvaloniaProperty.Register<GraphControl, IEnumerable<GraphSeries>>(nameof(Series));

    public static readonly StyledProperty<Func<double, string>> XLabelFormatterProperty =
        AvaloniaProperty.Register<GraphControl, Func<double, string>>(nameof(XLabelFormatter));

    public static readonly StyledProperty<Func<double, string>> YLabelFormatterProperty =
        AvaloniaProperty.Register<GraphControl, Func<double, string>>(nameof(YLabelFormatter));

    public static readonly StyledProperty<bool> FollowModeProperty =
        AvaloniaProperty.Register<GraphControl, bool>(nameof(FollowMode), defaultValue: true);

    public static readonly StyledProperty<double?> MaxVisibleXRangeProperty =
        AvaloniaProperty.Register<GraphControl, double?>(nameof(MaxVisibleXRange));

    public static readonly StyledProperty<double?> MaxVisibleYRangeProperty =
        AvaloniaProperty.Register<GraphControl, double?>(nameof(MaxVisibleYRange));
    
    private Point? _mousePosition;
    private bool _isDragging;
    private Point _lastDragPoint;
    private double _viewScale = 1.0;
    private double _viewOffset = 0.0;

    /// <summary>
    /// Gets or sets the collection of data series to be rendered.
    /// </summary>
    public IEnumerable<GraphSeries> Series
    {
        get => GetValue(SeriesProperty);
        set => SetValue(SeriesProperty, value);
    }

    /// <summary>
    /// Gets or sets the formatter function for X-axis labels.
    /// </summary>
    public Func<double, string> XLabelFormatter
    {
        get => GetValue(XLabelFormatterProperty);
        set => SetValue(XLabelFormatterProperty, value);
    }

    /// <summary>
    /// Gets or sets the formatter function for Y-axis labels.
    /// </summary>
    public Func<double, string> YLabelFormatter
    {
        get => GetValue(YLabelFormatterProperty);
        set => SetValue(YLabelFormatterProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the view should automatically scroll to the newest data.
    /// </summary>
    public bool FollowMode
    {
        get => GetValue(FollowModeProperty);
        set => SetValue(FollowModeProperty, value);
    }
    
    public double? MaxVisibleXRange
    {
        get => GetValue(MaxVisibleXRangeProperty);
        set => SetValue(MaxVisibleXRangeProperty, value);
    }

    public double? MaxVisibleYRange
    {
        get => GetValue(MaxVisibleYRangeProperty);
        set => SetValue(MaxVisibleYRangeProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Series == null || !Series.Any()) return;

        var allPoints = Series.SelectMany(s => s.Points).ToList();
        if (allPoints.Count == 0) return;

        var bounds = Bounds;
        double graphWidth = bounds.Width - (GraphPadding * 2);
        double graphHeight = bounds.Height - (GraphPadding * 2);

        double globalMinX = allPoints.Min(p => p.X);
        double globalMaxX = allPoints.Max(p => p.X);
        double minY = allPoints.Min(p => p.Y);
        double maxY = allPoints.Max(p => p.Y);
        
        if (MaxVisibleYRange.HasValue)
        {
            if (maxY - minY < MaxVisibleYRange.Value)
            {
                double center = (maxY + minY) / 2;
                maxY = center + (MaxVisibleYRange.Value / 2);
                minY = center - (MaxVisibleYRange.Value / 2);
            }
        }
        else
        {
            double yRange = maxY - minY;
            if (yRange == 0) yRange = 1;
            maxY += yRange * 0.1;
            minY -= yRange * 0.1;
        }

        double totalXRange = globalMaxX - globalMinX;
        if (totalXRange == 0) totalXRange = 1;

        double visibleRange;

        if (MaxVisibleXRange.HasValue && MaxVisibleXRange.Value > 0)
        {
            visibleRange = MaxVisibleXRange.Value;

            if (totalXRange > visibleRange)
                _viewScale = totalXRange / visibleRange;
            else
                _viewScale = 1.0;
        }
        else
        {
            visibleRange = totalXRange / _viewScale;
        }

        double currentMinX, currentMaxX;

        if (FollowMode)
        {
            currentMaxX = globalMaxX;
            currentMinX = currentMaxX - visibleRange;

            double maxOffset = 1.0 - (1.0 / _viewScale);
            _viewOffset = Math.Max(0, maxOffset);
        }
        else
        {
            currentMinX = globalMinX + (_viewOffset * totalXRange);
            currentMaxX = currentMinX + visibleRange;
        }

        Point ToPoint(GraphPoint p)
        {
            double xRatio = (p.X - currentMinX) / visibleRange;
            double x = GraphPadding + (xRatio * graphWidth);
            double y = (bounds.Height - GraphPadding) - ((p.Y - minY) / (maxY - minY)) * graphHeight;
            return new Point(x, y);
        }

        using (context.PushClip(new Rect(GraphPadding, 0, graphWidth, bounds.Height)))
        {
            DrawSeries(context, bounds, ToPoint);
        }

        DrawGrid(context, bounds, currentMinX, currentMaxX, minY, maxY);
        DrawScrollbar(context, bounds);

        if (_mousePosition.HasValue)
        {
            DrawTooltip(context, bounds, ToPoint, allPoints, currentMinX, currentMaxX);
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SeriesProperty)
        {
            var oldList = change.OldValue as IEnumerable<GraphSeries>;
            var newList = change.NewValue as IEnumerable<GraphSeries>;

            if (oldList is INotifyCollectionChanged oldIncc)
                oldIncc.CollectionChanged -= OnSeriesCollectionChanged;

            if (oldList != null)
            {
                foreach (var item in oldList)
                {
                    if (item is INotifyPropertyChanged inpc)
                        inpc.PropertyChanged -= OnSeriesItemChanged;
                }
            }

            if (newList is INotifyCollectionChanged newIncc)
                newIncc.CollectionChanged += OnSeriesCollectionChanged;

            if (newList != null)
            {
                foreach (var item in newList)
                {
                    if (item is INotifyPropertyChanged inpc)
                        inpc.PropertyChanged += OnSeriesItemChanged;
                }
            }

            InvalidateVisual();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if(MaxVisibleXRange.HasValue) 
        {
            SetCurrentValue(MaxVisibleXRangeProperty, null);
        }
        
        base.OnPointerWheelChanged(e);

        var bounds = Bounds;
        double graphWidth = bounds.Width - (GraphPadding * 2);
        double mouseX = e.GetPosition(this).X - GraphPadding;
        double relMouseX = Math.Clamp(mouseX / graphWidth, 0.0, 1.0);

        double currentWindowWidth = 1.0 / _viewScale;
        double dataPosUnderMouse = _viewOffset + (relMouseX * currentWindowWidth);

        double zoomFactor = ZoomStep;
        if (e.Delta.Y < 0) zoomFactor = 1 / zoomFactor;

        double newScale = Math.Clamp(_viewScale * zoomFactor, MinZoomScale, MaxZoomScale);

        double newWindowWidth = 1.0 / newScale;
        double newOffset = dataPosUnderMouse - (relMouseX * newWindowWidth);

        _viewScale = newScale;
        _viewOffset = newOffset;
        FollowMode = false;

        ClampOffset();
        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var props = e.GetCurrentPoint(this).Properties;

        if (props.IsLeftButtonPressed)
        {
            _isDragging = true;
            _lastDragPoint = e.GetPosition(this);
            e.Pointer.Capture(this);
            FollowMode = false;
        }

        if (e.ClickCount == 2)
        {
            _viewScale = 1.0;
            _viewOffset = 0.0;
            FollowMode = true;
            InvalidateVisual();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _isDragging = false;
        e.Pointer.Capture(null);
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        _mousePosition = e.GetPosition(this);

        if (_isDragging && _viewScale > 1.0)
        {
            var currentPos = e.GetPosition(this);
            double deltaPixels = _lastDragPoint.X - currentPos.X;

            double graphWidth = Bounds.Width - (GraphPadding * 2);
            if (graphWidth > 0)
            {
                double deltaOffset = (deltaPixels / graphWidth) / _viewScale;
                _viewOffset += deltaOffset;
                ClampOffset();
            }

            _lastDragPoint = currentPos;
        }

        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        _mousePosition = null;
        InvalidateVisual();
    }

    private void OnSeriesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => RequestRender();

    private void OnSeriesItemChanged(object? sender, PropertyChangedEventArgs e) => RequestRender();

    private void RequestRender()
    {
        if (FollowMode)
        {
            double maxOffset = 1.0 - (1.0 / _viewScale);
            _viewOffset = Math.Max(0, maxOffset);
        }
        InvalidateVisual();
    }

    private void ClampOffset()
    {
        double maxOffset = 1.0 - (1.0 / _viewScale);
        _viewOffset = Math.Clamp(_viewOffset, 0.0, maxOffset);
    }

    private void DrawSeries(DrawingContext context, Rect bounds, Func<GraphPoint, Point> toPoint)
    {
        foreach (var series in Series)
        {
            if (!series.Points.Any()) continue;

            var pen = new Pen(new SolidColorBrush(series.Color), 3, lineCap: PenLineCap.Round);
            var strokeGeo = new StreamGeometry();
            var fillGeo = new StreamGeometry();

            using (var ctx = strokeGeo.Open())
            {
                ctx.BeginFigure(toPoint(series.Points.First()), true);
                foreach (var pt in series.Points) ctx.LineTo(toPoint(pt));
                ctx.EndFigure(false);
            }

            using (var ctx = fillGeo.Open())
            {
                var first = toPoint(series.Points.First());
                var last = toPoint(series.Points.Last());
                double bottomY = bounds.Height - GraphPadding;

                ctx.BeginFigure(new Point(first.X, bottomY), true);
                ctx.LineTo(first);
                foreach (var pt in series.Points) ctx.LineTo(toPoint(pt));
                ctx.LineTo(new Point(last.X, bottomY));
                ctx.EndFigure(true);
            }

            var fillGradient = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
                GradientStops = new GradientStops
                {
                    new GradientStop(Color.FromArgb(100, series.Color.R, series.Color.G, series.Color.B), 0),
                    new GradientStop(Color.FromArgb(0, series.Color.R, series.Color.G, series.Color.B), 1)
                }
            };

            context.DrawGeometry(fillGradient, null, fillGeo);
            context.DrawGeometry(null, pen, strokeGeo);
        }
    }

    private void DrawGrid(DrawingContext context, Rect bounds, double minX, double maxX, double minY, double maxY)
    {
        var gridPen = new Pen(new SolidColorBrush(Color.Parse("#333333")), 1) { DashStyle = DashStyle.Dash };
        var labelBrush = new SolidColorBrush(Colors.Gray);
        var typeface = new Typeface(FontFamily.Default);

        int ySteps = 5;
        for (int i = 0; i <= ySteps; i++)
        {
            double normalized = i / (double)ySteps;
            double yVal = minY + (normalized * (maxY - minY));
            double yPos = (bounds.Height - GraphPadding) - (normalized * (bounds.Height - 2 * GraphPadding));

            context.DrawLine(gridPen, new Point(GraphPadding, yPos), new Point(bounds.Width - GraphPadding, yPos));

            string text = YLabelFormatter?.Invoke(yVal) ?? yVal.ToString("F0");
            var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, labelBrush);
            context.DrawText(formattedText, new Point(5, yPos - formattedText.Height / 2));
        }

        int xSteps = 6;
        for (int i = 0; i <= xSteps; i++)
        {
            double normalized = i / (double)xSteps;
            double xVal = minX + (normalized * (maxX - minX));
            double xPos = GraphPadding + (normalized * (bounds.Width - 2 * GraphPadding));

            string text = XLabelFormatter?.Invoke(xVal) ?? xVal.ToString("F1");
            var formattedText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 10, labelBrush);

            if (xPos > GraphPadding)
            {
                context.DrawText(formattedText, new Point(xPos - (formattedText.Width / 2), bounds.Height - GraphPadding + 5));
            }
        }
    }

    private void DrawScrollbar(DrawingContext context, Rect bounds)
    {
        if (_viewScale <= 1.0) return;

        double barWidth = bounds.Width - (GraphPadding * 2);
        double barX = GraphPadding;
        double barY = bounds.Height - 5;

        context.FillRectangle(Brushes.DarkGray, new Rect(barX, barY, barWidth, ScrollbarHeight), 2);

        double thumbWidth = barWidth * (1.0 / _viewScale);
        double thumbX = barX + (_viewOffset * barWidth);

        if (thumbX + thumbWidth > barX + barWidth)
            thumbX = barX + barWidth - thumbWidth;

        context.FillRectangle(Brushes.Cyan, new Rect(thumbX, barY, thumbWidth, ScrollbarHeight), 2);
    }

    private void DrawTooltip(DrawingContext context, Rect bounds, Func<GraphPoint, Point> toPoint, List<GraphPoint> allPoints, double minX, double maxX)
    {
        var mouseX = _mousePosition!.Value.X;

        var visiblePoints = allPoints.Where(p => p.X >= minX && p.X <= maxX);
        if (!visiblePoints.Any()) return;

        var closestPoint = visiblePoints.OrderBy(p => Math.Abs(toPoint(p).X - mouseX)).First();
        var screenPos = toPoint(closestPoint);

        if (screenPos.X < 0 || screenPos.X > bounds.Width) return;

        context.DrawLine(new Pen(Brushes.White, 1) { DashStyle = DashStyle.Dash },
                         new Point(screenPos.X, GraphPadding),
                         new Point(screenPos.X, bounds.Height - GraphPadding));

        context.DrawEllipse(Brushes.White, null, screenPos, 5, 5);
        context.DrawEllipse(Brushes.Black, null, screenPos, 3, 3);

        string text = $"{XLabelFormatter?.Invoke(closestPoint.X) ?? closestPoint.X.ToString()}\n{YLabelFormatter?.Invoke(closestPoint.Y) ?? closestPoint.Y.ToString()}";
        var formattedText = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Medium),
            12,
            Brushes.White
        );

        double cardPadding = 8;
        double cardW = formattedText.Width + cardPadding * 2;
        double cardH = formattedText.Height + cardPadding * 2;
        double cardX = screenPos.X + 15;
        double cardY = screenPos.Y - 15 - cardH;

        if (cardX + cardW > bounds.Width) cardX = screenPos.X - 15 - cardW;
        if (cardY < 0) cardY = screenPos.Y + 15;

        var cardRect = new Rect(cardX, cardY, cardW, cardH);
        context.FillRectangle(new SolidColorBrush(Color.Parse("#CC222222")), cardRect, 5);
        context.DrawRectangle(new Pen(Brushes.Gray, 1), cardRect, 5);
        context.DrawText(formattedText, new Point(cardX + cardPadding, cardY + cardPadding));
    }
}