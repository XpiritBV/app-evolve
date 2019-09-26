using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XamarinEvolve.Clients.UI.Effects;
using XamarinEvolve.Droid.Effects;

[assembly: ExportEffect(typeof(ImageShadowEffect), "ImageShadowEffect")]

namespace XamarinEvolve.Droid.Effects
{
    public class ImageShadowEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                if (global::Android.OS.Build.VERSION.SdkInt >= global::Android.OS.BuildVersionCodes.Lollipop)
                {
                    var control = Control as global::Android.Widget.ImageView;
                    var effect = (ShadowEffect)Element.Effects.FirstOrDefault(e => e is ShadowEffect);

                    control.Elevation = Math.Max(effect.DistanceX, effect.DistanceY);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot set property on attached control. Error: ", ex.Message);
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
