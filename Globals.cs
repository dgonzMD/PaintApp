﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PaintApp
{
    public static class Globals
    {
        public static SolidColorBrush scb;
        public static int[] di = { -1, -1, -1, 0, 1, 1, 1, 0 };
        public static int[] dj = { -1, 0, 1, 1, 1, 0, -1, -1 };
    }
}