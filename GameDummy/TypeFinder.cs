using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameDummy
{
    public static class TypeFinder
    {
        static Dictionary<string, Type> m_typeMap = new();
        public static Type? GetTypeFromFullName(string targetTypeFullName)
        {
            if (m_typeMap.TryGetValue(targetTypeFullName, out var existType))
            {
                return existType;
            }

            existType = Type.GetType(targetTypeFullName);
            if (existType is not null)
            {
                m_typeMap[targetTypeFullName] = existType;
                return existType;
            }

            // reload all type 
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                // error here 
                //                try get type from fullName: System.Drawing.Rectangle
                //[CLIENT] System.Reflection.ReflectionTypeLoadException: Unable to load one or more of the requested types.
                //Could not load type 'OptionValue' from assembly 'Steamworks.NET, Version=20.0.0.0, Culture=neutral, PublicKeyToken=null' because it contains an object field at offset 0 that is incorrectly aligned or overlapped by a non-object field.

                //var types = asm.GetTypes();
                //foreach (var type in types)
                //{
                //    m_typeMap[type.FullName] = type;
                //}


                // Fixed
                var type = asm.GetType(targetTypeFullName);
                if (type is not null)
                {
                    m_typeMap[targetTypeFullName] = type;
                    return type;
                }
            }

            return null;
        }
    }
}
