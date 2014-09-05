using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SkypeSlacker.Extensions
{
    public static class ButtonExtensions
    {
        public static void Toggle(this Button button)
        {
            button.IsEnabled = !button.IsEnabled;
        }
    }
}
