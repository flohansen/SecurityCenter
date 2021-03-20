﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace AquaMaintenancer.Theme.Components
{
    public class ChartData
    {
        public IEnumerable<float> Values { get; set; }
        public string Category { get; set; }
    }

    public class BarChart : Canvas
    {
        public Pen PenLabels { get; set; } = new Pen();
        public Pen PenAxis { get; set; } = new Pen();
        public Pen PenGrid { get; set; } = new Pen();
        public Pen PenLegendLabels { get; set; } = new Pen();

        public List<Brush> Colors { get; set; } = new List<Brush>();

        public double LegendEllipsesRadius { get; set; } = 2.5;
        public double LegendEllipsesSpacing { get; set; } = 8.0;
        public double LegendLabelSpacing { get; set; } = 12.0;
        public double LegendSpacing { get; set; } = 64.0;
        public double LegendLabelSize { get; set; } = 12.0;

        public double ValueLabelSize { get; set; } = 12.0;
        public double CategoryLabelSize { get; set; } = 12.0;

        #region LabelSpacing Property
        public static readonly DependencyProperty LabelSpacingProperty =
            DependencyProperty.Register(nameof(LabelSpacing), typeof(double),
                typeof(BarChart), new PropertyMetadata(16.0));

        public double LabelSpacing
        {
            get => (double)GetValue(LabelSpacingProperty);
            set => SetValue(LabelSpacingProperty, value);
        }
        #endregion

        #region BarSpacing Property
        public static readonly DependencyProperty BarSpacingProperty =
            DependencyProperty.Register(nameof(BarSpacing), typeof(double),
                typeof(BarChart), new PropertyMetadata(5.0));

        public double BarSpacing
        {
            get => (double)GetValue(BarSpacingProperty);
            set => SetValue(BarSpacingProperty, value);
        }
        #endregion

        #region InsideSpacing Property
        public static readonly DependencyProperty InsideSpacingProperty =
            DependencyProperty.Register(nameof(InsideSpacing), typeof(double),
                typeof(BarChart), new PropertyMetadata(15.0));

        public double InsideSpacing
        {
            get => (double)GetValue(InsideSpacingProperty);
            set => SetValue(InsideSpacingProperty, value);
        }
        #endregion

        #region BarWidth Property
        public static readonly DependencyProperty BarWidthProperty =
            DependencyProperty.Register(nameof(BarWidth), typeof(double),
                typeof(BarChart), new PropertyMetadata(20.0));

        public double BarWidth
        {
            get => (double)GetValue(BarWidthProperty);
            set => SetValue(BarWidthProperty, value);
        }
        #endregion

        public List<FormattedText> ValueLabels { get; private set; }
        public List<FormattedText> CategoryLabels { get; private set; }
        public List<FormattedText> SubCategoryLegendLabels { get; private set; }
        public List<string> SubCategories { get; set; } = new List<string>()
        {
            "Information",
            "Warnung",
            "Fehler"
        };

        private IEnumerable<ChartData> data = new List<ChartData>()
        {
            new ChartData { Values = new List<float>() { 57.0f, 25.0f, 13.0f }, Category = "07.03.2021" },
            new ChartData { Values = new List<float>() { 13.0f }, Category = "08.03.2021" },
            new ChartData { Values = new List<float>() { 30.0f }, Category = "09.03.2021" },
            new ChartData { Values = new List<float>() { 80.0f }, Category = "10.03.2021" },
            new ChartData { Values = new List<float>() { 29.0f }, Category = "11.03.2021" },
        };

        public BarChart()
        {
            BrushConverter bc = new BrushConverter();
            Colors.Add(bc.ConvertFrom("#5D4ADA") as SolidColorBrush);
            Colors.Add(bc.ConvertFrom("#66CA67") as SolidColorBrush);
            Colors.Add(bc.ConvertFrom("#2E9BFF") as SolidColorBrush);

            PenGrid.Brush = bc.ConvertFrom("#FFFFFF") as SolidColorBrush;
            PenGrid.Brush.Opacity = 0.05;

            PenAxis.Brush = bc.ConvertFrom("#FFFFFF") as SolidColorBrush;
            PenAxis.Brush.Opacity = 1.0;

            PenLabels.Brush = bc.ConvertFrom("#FFFFFF") as SolidColorBrush;
            PenLabels.Brush.Opacity = 0.3;

            PenLegendLabels.Brush = bc.ConvertFrom("#FFFFFF") as SolidColorBrush;
            PenLegendLabels.Brush.Opacity = 1.0;

            ValueLabels = GetValueLabels(data, 5);
            CategoryLabels = GetCategoryLabels(data);
            SubCategoryLegendLabels = GetSubCategoryLabels(SubCategories);
        }

        protected override void OnRender(DrawingContext ctx)
        {
            double categoryLabelsHeight = CalculateCategoryLabelsHeight(CategoryLabels);
            double valueLabelsWidth = CalculateValueLabelsWidth(ValueLabels);
            double legendWidth = CalculateLegendWidth();

            double categoryLabelsWidth = CalculateCategoryLabelsWidth(CategoryLabels, valueLabelsWidth, legendWidth);
            double valueLabelsHeight = CalculateValueLabelsHeight(ValueLabels, categoryLabelsHeight);

            // Draw the labels on y-axis
            DrawGridLines(ctx, valueLabelsWidth, legendWidth);
            DrawValueLabels(ctx, ValueLabels, valueLabelsHeight, categoryLabelsHeight);
            DrawCategoryLabels(ctx, CategoryLabels, valueLabelsWidth, categoryLabelsWidth);

            // Draw the x-axis
            ctx.DrawLine(PenAxis, new System.Windows.Point(valueLabelsWidth, ActualHeight - categoryLabelsHeight), new System.Windows.Point(ActualWidth - legendWidth - LegendSpacing, ActualHeight - categoryLabelsHeight));

            // Draw the y-axis
            ctx.DrawLine(PenAxis, new System.Windows.Point(valueLabelsWidth, 0), new System.Windows.Point(valueLabelsWidth, ActualHeight - categoryLabelsHeight));

            DrawLegend(ctx, legendWidth);
        }

        private double Remap(double value, double min1, double max1, double min2, double max2)
        {
            return (value - min1) * (max2 - min2) / (max1 - min1) + min2;
        }

        private double GetCanvasValue(double value)
        {
            double categoryLabelsHeight = CalculateCategoryLabelsHeight(CategoryLabels);

            double min1 = 0.0;
            double max1 = ValueLabels.Max(label => double.Parse(label.Text));
            double min2 = ActualHeight - categoryLabelsHeight;
            double max2 = ValueLabels.Last().Height / 2.0;

            return Remap(value, min1, max1, min2, max2);
        }

        private double CalculateLegendWidth()
        {
            double labelsWidth = SubCategoryLegendLabels.Max(x => x.Width);
            return labelsWidth + 2 * LegendEllipsesRadius + LegendEllipsesSpacing;
        }

        private double CalculateCategoryLabelsWidth(List<FormattedText> labels, double valueLabelsWidth, double legendWidth)
        {
            return ActualWidth - valueLabelsWidth - labels.Last().Width - 2 * InsideSpacing - legendWidth - LegendSpacing;
        }

        private double CalculateCategoryLabelsHeight(List<FormattedText> labels)
        {
            return labels.Max(label => label.Height) + LabelSpacing;
        }

        private double CalculateValueLabelsWidth(List<FormattedText> labels)
        {
            return labels.Max(label => label.Width) + LabelSpacing;
        }

        private double CalculateValueLabelsHeight(List<FormattedText> labels, double categoryLabelsHeight)
        {
            return ActualHeight - categoryLabelsHeight - labels.Last().Height / 2;
        }

        private List<FormattedText> GetValueLabels(IEnumerable<ChartData> data, int count)
        {
            float max = data.Max(d => d.Values.Max());
            List<FormattedText> labels = new List<FormattedText>();

            for (int i = 0; i < count; i++)
            {
                float yValue = max - i * max / (count - 1);
                FormattedText label = new FormattedText(yValue.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Roboto"), ValueLabelSize, PenLabels.Brush, 1.25);
                labels.Add(label);
            }

            return labels;
        }

        private List<FormattedText> GetCategoryLabels(IEnumerable<ChartData> data)
        {
            return data.Select(d => new FormattedText(d.Category, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Roboto"), CategoryLabelSize, PenLabels.Brush, 1.25)).ToList();
        }

        private List<FormattedText> GetSubCategoryLabels(IEnumerable<string> names)
        {
            return names.Select(name =>
                new FormattedText(name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Roboto"), LegendLabelSize, PenLegendLabels.Brush, 1.25)
            ).ToList();
        }

        private void DrawGridLines(DrawingContext ctx, double valueLabelsWidth, double legendWidth)
        {
            for (int i = 0; i < ValueLabels.Count - 1; i++)
            {
                FormattedText label = ValueLabels[i];
                double y = GetCanvasValue(double.Parse(label.Text));
                ctx.DrawLine(PenGrid, new System.Windows.Point(valueLabelsWidth, y), new System.Windows.Point(ActualWidth - legendWidth - LegendSpacing, y));
            }
        }

        private void DrawValueLabels(DrawingContext ctx, List<FormattedText> labels, double valueLabelsHeight, double categoryLabelsHeight)
        {
            double maxLabelWidth = labels.Max(l => l.Width);
            double steps = valueLabelsHeight / (labels.Count - 1);

            for (int i = 0; i < labels.Count; i++)
            {
                FormattedText label = labels[i];
                double x = maxLabelWidth - label.Width;
                double y = i * steps;

                ctx.DrawText(label, new System.Windows.Point(x, y));
            }
        }

        private void DrawCategoryLabels(DrawingContext ctx, List<FormattedText> labels, double valueLabelsWidth, double categoryLabelsWidth)
        {
            int numberLabels = labels.Count;
            double steps = categoryLabelsWidth / (numberLabels - 1);

            for (int i = 0; i < numberLabels; i++)
            {
                double x = i * steps + valueLabelsWidth + InsideSpacing;

                FormattedText label = labels[i];
                ctx.DrawText(label, new System.Windows.Point(x, ActualHeight - label.Height));

                double zero = GetCanvasValue(0);
                List<float> values = data.First(d => d.Category.Equals(label.Text)).Values.ToList();

                for (int j = 0; j < values.Count; j++)
                {
                    int n = values.Count;
                    double dx = x - ((n - 1) * BarSpacing + n * BarWidth) / 2 + (BarWidth + BarSpacing) * j + label.Width / 2;
                    double height = GetCanvasValue(values[j]);

                    Brush brush = GetColorForSubCategoryIndex(j);
                    Brush transparentBrush = brush.Clone();
                    transparentBrush.Opacity = 0.5;

                    ctx.DrawRectangle(transparentBrush, null, new Rect(dx, height, BarWidth, zero - height));
                    ctx.DrawRectangle(brush, null, new Rect(dx, height, BarWidth, 4));
                }
            }
        }

        private Brush GetColorForSubCategoryIndex(int index)
        {
            return Colors[index % Colors.Count];
        }

        private void DrawLegend(DrawingContext ctx, double legendWidth)
        {
            double x = ActualWidth - legendWidth + LegendEllipsesRadius;
            double y = 0;

            for (int i = 0; i < SubCategoryLegendLabels.Count; i++)
            {
                FormattedText subCategory = SubCategoryLegendLabels[i];
                double ellipseX = x;
                double ellipseY = y + LegendEllipsesRadius;

                if (2 * LegendEllipsesRadius < subCategory.Height)
                    ellipseY += subCategory.Height / 2 - LegendEllipsesRadius;

                Brush brush = GetColorForSubCategoryIndex(i);
                ctx.DrawEllipse(brush, null, new System.Windows.Point(ellipseX, ellipseY), LegendEllipsesRadius, LegendEllipsesRadius);

                ctx.DrawText(subCategory, new System.Windows.Point(x + LegendEllipsesRadius + LegendEllipsesSpacing, y));
                y += subCategory.Height + LegendLabelSpacing;
            }
        }

    }
}
