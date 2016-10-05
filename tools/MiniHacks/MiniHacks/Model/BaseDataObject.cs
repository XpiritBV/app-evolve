using System;
using MvvmHelpers;

namespace MiniHacks.Model
{
    public class BaseDataObject : ObservableObject
    {
        public BaseDataObject()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string RemoteId { get; set; }

        [Newtonsoft.Json.JsonProperty("Id")]
        public string Id { get; set; }

    }
}

