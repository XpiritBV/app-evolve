using System;
using System.Collections.Generic;

namespace XamarinEvolve.DataObjects
{
    public class ScavengerHunt : BaseDataObject
    {
        public ScavengerHunt()
        {
            ObjectsToFind = new List<ObjectToFind>();
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? OpenFrom { get; set; }
        public DateTime? OpenUntil { get; set; }

        public virtual ICollection<ObjectToFind> ObjectsToFind { get; set; }
	}
}

