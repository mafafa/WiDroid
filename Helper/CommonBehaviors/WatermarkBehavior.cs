/*
 * Code taken and modified from http://stackoverflow.com/questions/833943/watermark-hint-text-placeholder-textbox-in-wpf
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

using Helper.CommonAdorners;


namespace Helper.CommonBehaviors
{
    public static class WatermarkBehavior
    {
        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.RegisterAttached(
                    "WatermarkText",
                    typeof(String),
                    typeof(WatermarkBehavior),
                    new UIPropertyMetadata(String.Empty, OnWatermarkChanged));

        public static String GetWatermarkText(DependencyObject obj)
        {
            return (String)obj.GetValue(WatermarkTextProperty);
        }

        public static void SetWatermarkText(DependencyObject obj, String value)
        {
            obj.SetValue(WatermarkTextProperty, value);
        }

        private static void OnWatermarkChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            Control control = (Control)obj;
            if ((control != null) && (obj is TextBox))
            {
                if ((String)args.NewValue != String.Empty)
                {
                    control.Loaded += ControlLostKeyboardFocus;
                    control.LostKeyboardFocus += ControlLostKeyboardFocus;
                    control.GotKeyboardFocus += ControlGotKeyboardFocus;
                }
                else
                {
                    control.Loaded -= ControlLostKeyboardFocus;
                    control.LostKeyboardFocus -= ControlLostKeyboardFocus;
                    control.GotKeyboardFocus -= ControlGotKeyboardFocus;
                }
            }
        }

        private static void ControlLostKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (ShouldShowWatermark(textBox))
            {
                AddWatermark(textBox);
            }
        }

        private static void ControlGotKeyboardFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (ShouldShowWatermark(textBox))
            {
                RemoveWatermark(textBox);
            }
        }

        private static bool ShouldShowWatermark(TextBox textBox)
        {
            return textBox.Text == String.Empty;
        }

        private static void AddWatermark(TextBox textBox)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(textBox);

            // layer could be null if control is no longer in the visual tree
            if (adornerLayer != null)
            {
                adornerLayer.Add(new WatermarkAdorner(textBox, GetWatermarkText(textBox)));
            }
        }

        private static void RemoveWatermark(TextBox textBox)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(textBox);

            // layer could be null if control is no longer in the visual tree
            if (adornerLayer != null)
            {
                Adorner[] adorners = adornerLayer.GetAdorners(textBox);
                if (adorners == null)
                {
                    return;
                }

                foreach (Adorner adorner in adorners)
                {
                    if (adorner is WatermarkAdorner)
                    {
                        adorner.Visibility = Visibility.Hidden;
                        adornerLayer.Remove(adorner);
                    }
                }
            }
        }
    }
}
