using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvVariableInect
{
    public static class Extensions
    {
        public static bool ContainsAttribute(this IEnumerable<CustomAttribute> attributes, Type attributeType)
        {
            return attributes.Any(attribute => attribute.Constructor.DeclaringType.Name == attributeType.Name);
        }

        public static CustomAttribute GetAttribute(this IEnumerable<CustomAttribute> attributes, Type attributeType)
        {
            return attributes.FirstOrDefault(x => x.Constructor.DeclaringType.Name == attributeType.Name);
        }

        public static bool AsBool(this string value)
        {
            return value == "true" || value == "1";
        }
    }
}
