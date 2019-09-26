using System;
using Foundation;

namespace XamarinEvolve.iOS
{
    [Preserve(AllMembers = true)]
    public class LinkerPleaseInclude
    {
        void Include(System.ComponentModel.ReferenceConverter converter)
        {
            var x = new System.ComponentModel.ReferenceConverter(typeof(void));
        }
    }
}
