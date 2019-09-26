using System;
#if !MOBILE
using System.ComponentModel.DataAnnotations;
#endif

namespace XamarinEvolve.DataObjects
{
    public class MobileToWebSync : BaseDataObject
    {
        public string UserId { get; set; }

#if !MOBILE
        [StringLength(5)]
#endif
        public string TempCode { get; set; }

        public DateTime Expires { get; set; }
    }
}
