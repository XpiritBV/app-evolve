using System;

namespace XamarinEvolve.DataObjects
{
    public class Room : BaseDataObject
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the image URL if there is one
        /// </summary>
        /// <value>The image URL.</value>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the latitude if there is one
        /// </summary>
        /// <value>The latitude.</value>
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude if there is one
        /// </summary>
        /// <value>The longitude.</value>
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the floor number or level of the room
        /// </summary>
        /// <value>The floor level.</value>
        public int? FloorLevel { get; set; }

        /// <summary>
        /// Gets or sets the X coordinate on the floor map if there is one
        /// </summary>
        /// <value>The X Coordinate.</value>
        public int? XCoordinate { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate on the floor map if there is one
        /// </summary>
        /// <value>The Y Coordinate.</value>
        public int? YCoordinate { get; set; }

#if MOBILE
        [Newtonsoft.Json.JsonIgnore]
        public Uri ImageUri 
        { 
            get 
            { 
                try
                {
                    return new Uri(ImageUrl);
                }
                catch
                {

                }
                return null;
            } 
        }
#endif
    }
}