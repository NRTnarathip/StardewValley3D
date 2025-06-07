using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GuyNetwork
{
    internal sealed class ObjectEnableAnnotation
    {
        public readonly object obj;
        public readonly List<MethodInfo> methods;
        public readonly Type type;
        public readonly List<BaseAnnotationAttribute> baseAnnotationAttributes;
        public readonly Dictionary<string, List<MethodInfo>> methodsLookupWithAnnotationType = new();
        public readonly Dictionary<MethodInfo, List<BaseAnnotationAttribute>> annotationsLookupByMethodInfo = new();

        public ObjectEnableAnnotation(object obj)
        {
            this.obj = obj;
            this.type = obj.GetType();
            methods = type.GetMethods(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
            ).ToList();

            foreach (var method in methods)
            {
                var attributes = (BaseAnnotationAttribute[])method.GetCustomAttributes(typeof(BaseAnnotationAttribute), true);
                if (attributes.Length > 0)
                {
                    annotationsLookupByMethodInfo.Add(method, new(attributes));

                    foreach (var att in attributes)
                    {
                        var attType = att.GetType();
                        if (methodsLookupWithAnnotationType.TryGetValue(
                            attType.FullName, out var methodsLookupWithAnnotationType_value) is false)
                        {
                            methodsLookupWithAnnotationType_value = new();
                            methodsLookupWithAnnotationType[attType.FullName] = methodsLookupWithAnnotationType_value;
                        }

                        methodsLookupWithAnnotationType_value.Add(method);
                    }
                }
            }
        }

        public List<MethodInfo>? GetMethodsFromAnnotationFullName(string fullName)
        {
            methodsLookupWithAnnotationType.TryGetValue(fullName, out var result);
            return result;
        }

        public List<BaseAnnotationAttribute>? GetBaseAnnotationAttributesFromMethodInfo(MethodInfo method)
        {
            annotationsLookupByMethodInfo.TryGetValue(method, out var result);
            return result;
        }
    }
}
