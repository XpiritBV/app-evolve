﻿using System;
using Xamarin.Forms;
using XamarinEvolve.Droid;
using Xamarin.Forms.Platform.Android;
using Android.Widget;
using Android.Content;
using Android.Util;
using Android.Views;

[assembly:ExportCell(typeof(SwitchCell), typeof(SwitchCellEx))]
namespace XamarinEvolve.Droid
{
    public class SwitchCellEx: SwitchCellRenderer
    {

		protected override global::Android.Views.View GetCellCore(Cell item, global::Android.Views.View convertView, ViewGroup parent, Context context)
		{
		    var cell = (LinearLayout)base.GetCellCore(item, convertView, parent, context);

            var layout = cell.GetChildAt(1) as LinearLayout;
            if(layout == null)
                return cell;

            var label = layout.GetChildAt(0) as TextView;
            if (label == null)
                return cell;

            label.SetTextSize(ComplexUnitType.Dip, 16);

            var switchWidget = cell.GetChildAt(2) as global::Android.Widget.Switch;
            if (switchWidget == null)
                return cell;


            var layoutParams = (LinearLayout.LayoutParams)switchWidget.LayoutParameters;
            layoutParams.Height = ViewGroup.LayoutParams.WrapContent;
            layoutParams.Width = ViewGroup.LayoutParams.WrapContent;

            int px12 = (int)TypedValue.ApplyDimension(
                ComplexUnitType.Dip,
                12f, 
                context.Resources.DisplayMetrics
            );

            int px8 = (int)TypedValue.ApplyDimension(
                ComplexUnitType.Dip,
                8f, 
                context.Resources.DisplayMetrics
            );

            layoutParams.SetMargins(px8, px8, px12, px8);

            return cell;
        }

    }
}
