using System;
namespace MiniHacks.Model
{
    public class MiniHack : BaseDataObject
    {
        public string Name { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string GitHubUrl { get; set; }
        public string BadgeUrl { get; set; }
        public string UnlockCode { get; set; }


        [Newtonsoft.Json.JsonIgnore]
        public Uri BadgeUri 
        { 
            get 
            { 
                try
                {
                    return new Uri(BadgeUrl);
                }
                catch
                {

                }
                return null;
            } 
        }

        bool isCompleted;
        [Newtonsoft.Json.JsonIgnore]
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { SetProperty(ref isCompleted, value); }
        }
    }
}

