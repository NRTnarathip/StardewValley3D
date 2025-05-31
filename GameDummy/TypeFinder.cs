using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GameDummy
{
    public static class TypeFinder
    {
        static Dictionary<string, Type> m_typeMap = new();
        public static Type? GetTypeFromFullName(string inputTypeFullName)
        {
            if (m_typeMap.TryGetValue(inputTypeFullName, out var existType))
            {
                return existType;
            }

            existType = Type.GetType(inputTypeFullName);
            if (existType is not null)
            {
                m_typeMap[inputTypeFullName] = existType;
                return existType;
            }

            // reload all type 
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                var types = asm.GetTypes();
                foreach (var type in types)
                {
                    m_typeMap[type.FullName] = type;
                }
            }

            m_typeMap.TryGetValue(inputTypeFullName, out existType);

            return existType;
        }
    }
}
