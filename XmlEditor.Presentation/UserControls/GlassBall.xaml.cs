using System.Windows;
using System.Windows.Media;

namespace XmlEditor.Presentation.UserControls
{
    /// <summary>
    /// Interaction logic for GlassBall.xaml
    /// </summary>
    /// <seealso cref="http://www.zimbio.com/Silverlight/articles/5/Glass+ball"/>
    public partial class GlassBall 
    {
        public GlassBall()
        {
            InitializeComponent();
        }

        #region Color

        /// <summary>
        /// Color Dependency Property
        /// </summary>
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(GlassBall),
                new FrameworkPropertyMetadata(Colors.DarkRed,
                    new PropertyChangedCallback(OnColorChanged)));

        /// <summary>
        /// Gets or sets the Color property. This dependency property 
        /// indicates ....
        /// </summary>
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Color property.
        /// </summary>
        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (GlassBall)d;
            var oldColor = (Color)e.OldValue;
            var newColor = target.Color;
            var opaqueColor = newColor;
            opaqueColor.A = 0; // set transparency to 0
            target.ballBack1.Fill = new SolidColorBrush(newColor);
            target.ballShadow2.Color = opaqueColor;
            target.OnColorChanged(oldColor, newColor);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Color property.
        /// </summary>
        protected virtual void OnColorChanged(Color oldColor, Color newColor)
        {
        }

        #endregion

        #region Diameter

        /// <summary>
        /// Diameter Dependency Property
        /// </summary>
        public static readonly DependencyProperty DiameterProperty =
            DependencyProperty.Register("Diameter", typeof(uint), typeof(GlassBall),
                new FrameworkPropertyMetadata((uint)32,
                    new PropertyChangedCallback(OnDiameterChanged)));

        /// <summary>
        /// Gets or sets the Diameter property. 
        /// </summary>
        public uint Diameter
        {
            get { return (uint)GetValue(DiameterProperty); }
            set { SetValue(DiameterProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Diameter property.
        /// </summary>
        private static void OnDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var target = (GlassBall)d;
            var oldDiameter = (uint)e.OldValue;
            var newDiameter = target.Diameter;
            target.scale.ScaleX = target.scale.ScaleY = newDiameter/218F;
            target.OnDiameterChanged(oldDiameter, newDiameter);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Diameter property.
        /// </summary>
        protected virtual void OnDiameterChanged(uint oldDiameter, uint newDiameter)
        {
        }

        #endregion

        
    }
}
