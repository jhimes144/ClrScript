using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Clank
{
    static class Util
    {
        public static T[] CreateInstancesOfInterface<T>() where T : class
        {
            var interfaceType = typeof(T);
            var assembly = Assembly.GetExecutingAssembly();

            var implementingTypes = assembly.GetTypes()
                .Where(type => interfaceType.IsAssignableFrom(type) &&
                       !type.IsInterface &&
                       !type.IsAbstract &&
                       type.GetConstructor(Type.EmptyTypes) != null)
                .ToArray();

            var instances = implementingTypes
                .Select(type => (T)Activator.CreateInstance(type))
                .ToArray();

            return instances;
        }
    }
}
