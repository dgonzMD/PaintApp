using System;
using Microsoft.Phone.Controls;

namespace PaintApp
{
    public partial class Page1 : PhoneApplicationPage
    {
        public Page1()
        {
            InitializeComponent();
        }

        private void AppBarConfirm(object o, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}