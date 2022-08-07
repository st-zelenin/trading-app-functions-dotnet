using System;
namespace DataAccess.Models
{
    public class MissingItemResponse : Exception
    {
        public MissingItemResponse(string id, string containerName)
            : base($"Item {id} is missing in {containerName}") { }

    }
}

