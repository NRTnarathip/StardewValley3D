using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuyNetwork
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class OnMessageAttribute : BaseAnnotationAttribute
    {
        public static readonly string TypeFullName = typeof(OnMessageAttribute).FullName;
        public string MessageName { get; }
        public OnMessageAttribute(string messageName) => MessageName = messageName;
    }
}
