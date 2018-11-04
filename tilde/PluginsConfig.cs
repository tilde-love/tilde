// Copyright (c) Tilde Love Project. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.
//
namespace tilde
{
//    public class PluginsConfig : Dictionary<string, string>
//    {        
//        public static IEnumerable<IScriptApi> FindScriptApis(Assembly assembly)
//        {
//            List<IScriptApi> apis = new List<IScriptApi>(); 
//            
//            foreach (Type type in assembly.GetExportedTypes())
//            {
//                if (type.IsAbstract == true)
//                {
//                    continue;
//                }
//
//                if (type.IsInterface == true)
//                {
//                    continue; 
//                }
//
//                if (type.IsGenericType == true)
//                {
//                    continue; 
//                }
//
//                if (typeof(IScriptApi).IsAssignableFrom(type) == false)
//                {
//                    continue;
//                }
//
//
//                ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
//
//                if (constructorInfo == null)
//                {
//                    continue;
//                }
//
//                apis.Add((IScriptApi) System.Activator.CreateInstance(type));
//            }
//
//            return apis; 
//        }
//    }
}