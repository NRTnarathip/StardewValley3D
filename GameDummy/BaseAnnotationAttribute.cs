using System;

namespace GameDummy
{
    public abstract class BaseAnnotationAttribute : Attribute
    {
        public BaseAnnotationAttribute()
        {
            Console.WriteLine("on ctor base network annotation");
        }
    }
}
