using System;

namespace Streamdeck_collection.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class StreamdeckActionAttribute : Attribute
    {
        public string Action { get; }

        public StreamdeckActionAttribute(string action)
        {
            Action = action;
        }
    }
}
