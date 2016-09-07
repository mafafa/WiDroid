/*
 * Code taken and modified from http://stackoverflow.com/questions/833943/watermark-hint-text-placeholder-textbox-in-wpf
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;


namespace Helper.CommonAdorners
{
    public class WatermarkAdorner : Adorner
    {
        private String _watermarkText;
        private TextBlock _watermarkTextBlock;
        private ContentPresenter _contentPresenter;
        
        public WatermarkAdorner(UIElement adornedElement, String watermarkText)
            : base(adornedElement)
        {
            WatermarkText = watermarkText;
            _watermarkTextBlock = new TextBlock();

            IsHitTestVisible = false;

            // Watermark Style
            _contentPresenter = new ContentPresenter();
            _watermarkTextBlock.Text = WatermarkText;
            _watermarkTextBlock.FontStyle = FontStyles.Italic;
            _contentPresenter.Content = _watermarkTextBlock;
            _contentPresenter.Opacity = 0.6;
            _contentPresenter.Margin = new Thickness(5, 0, 0, 0);

            // Hide the control adorner when the adorned element is hidden
            Binding binding = new Binding("IsVisible");
            binding.Source = AdornedElement;
            binding.Converter = new BooleanToVisibilityConverter();
            SetBinding(VisibilityProperty, binding);
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _contentPresenter;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Here's the secret to getting the adorner to cover the whole control
            _contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public String WatermarkText
        {
            get { return _watermarkText; }
            set { _watermarkText = value; }
        }

        public Control Control
        {
            get { return (Control)AdornedElement; }
        }
    }
}
