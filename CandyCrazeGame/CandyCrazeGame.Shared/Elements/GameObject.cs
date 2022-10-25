using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Foundation;

namespace CandyCrazeGame
{
    public class GameObject : Border
    {
        #region Fields

        private readonly Image _content = new() { Stretch = Stretch.Uniform, Visibility = Microsoft.UI.Xaml.Visibility.Collapsed };

        private readonly CompositeTransform _compositeTransform = new()
        {
            CenterX = 0.5,
            CenterY = 0.5,
            Rotation = 0,
            ScaleX = 1,
            ScaleY = 1,
        };


        #region HitBox Debug

        //private Border _hitBoxborder;

        #endregion

        #endregion

        #region Properties

        public double X { get; set; }

        public double Y { get; set; }

        public double Speed { get; set; }

        #endregion

        #region Ctor

        public GameObject()
        {
            RenderTransformOrigin = new Point(0.5, 0.5);

            RenderTransform = _compositeTransform;
            CanDrag = false;

            Child = _content;

            #region HitBox Debug

            //BorderThickness = new Microsoft.UI.Xaml.Thickness(1);
            //BorderBrush = new SolidColorBrush(Colors.Black);

            //_hitBoxborder = new Border()
            //{
            //    BorderThickness = new Microsoft.UI.Xaml.Thickness(1),
            //    BorderBrush = new SolidColorBrush(Colors.Black)
            //};

            //var grid = new Grid()
            //{
            //    HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
            //    VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center
            //};

            //grid.Children.Add(_content);
            //grid.Children.Add(_hitBoxborder);

            //Child = grid;

            #endregion
        }

        #endregion        

        #region Methods

        public void SetSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double GetTop()
        {
            return Canvas.GetTop(this);
        }

        public double GetLeft()
        {
            return Canvas.GetLeft(this);
        }

        public void SetTop(double top)
        {
            Canvas.SetTop(this, top);
        }

        public void SetLeft(double left)
        {
            Canvas.SetLeft(this, left);
        }

        public void SetZ(int z)
        {
            Canvas.SetZIndex(this, z);
        }

        public void SetPosition(double left, double top)
        {
            Canvas.SetTop(this, top);
            Canvas.SetLeft(this, left);
        }

        public void SetContent(Uri uri)
        {
            _content.Source = new BitmapImage(uri);
            _content.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
        }

        public void HideContent()
        {
            _content.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
        }

        public void SetHitBoxBorder(Rect rect)
        {
            //_hitBoxborder.Height = rect.Height;
            //_hitBoxborder.Width = rect.Width;
        }

        public void SetScaleTransform(double scaleTransform)
        {
            _compositeTransform.ScaleX = scaleTransform;
            _compositeTransform.ScaleY = scaleTransform;
        }

        public void SetScaleX(double scaleX)
        {
            _compositeTransform.ScaleX = scaleX;
        }

        #endregion
    }

    public enum ElementType
    {
        NONE,
        PLAYER,
        PLAYER_JUMP,
        PLAYER_FALL,
        PLAYER_FLY,
        POWERUP,
        HEALTH,
        COLLECTIBLE,
        CLOUD,
    }
}
