using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace EscapeRoom
{
    public class Animation
    {
        public async Task FadeInAsync(UIElement element, double seconds = .3, bool handleVisibility = true)
        {
            if (handleVisibility)
                element.Visibility = Visibility.Visible;

            DoubleAnimation animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds));

            element.BeginAnimation(UIElement.OpacityProperty, animation);

            await Task.Delay(TimeSpan.FromSeconds(seconds));
        }
        public async Task FadeOutAsync(UIElement element, double seconds = .3, Visibility handleVisiblity = Visibility.Collapsed)
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(seconds));

            element.BeginAnimation(UIElement.OpacityProperty, animation);

            await Task.Delay(TimeSpan.FromSeconds(seconds));

            if (handleVisiblity != Visibility.Visible)
                element.Visibility = handleVisiblity;
        }
        public void FadeIn(UIElement element, double seconds = .3)
        {
            DoubleAnimation animation = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds));
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        public void FadeOut(UIElement element, double seconds = .3)
        {
            DoubleAnimation animation = new DoubleAnimation(0, TimeSpan.FromSeconds(seconds));
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}
