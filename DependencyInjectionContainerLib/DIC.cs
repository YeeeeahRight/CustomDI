using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DependencyInjectionContainerLib
{
    public class DIC
    {
        private readonly DICConfig _configuration;
        private readonly Stack<Type> _dependenciesStack;
        private static readonly object _ob = new object();

        public DIC(DICConfig config)
        {
            _configuration = config;
            _dependenciesStack = new Stack<Type>();
        }

        public T Resolve<T>() where T : class
        {
            var typeToResolve = typeof(T);
            var _currentType = typeToResolve;
            RegisteredTypeInfo registeredType = _configuration.GetImplementation(typeToResolve);
            if (typeToResolve.IsGenericType && registeredType == null)            
                registeredType = _configuration.GetImplementation(typeToResolve.GetGenericTypeDefinition());            
            else            
                registeredType = _configuration.GetImplementation(typeToResolve);
            return (T)GetInstance(registeredType, _currentType);
        }

        public IEnumerable<T> ResolveAll<T>() where T : class
        {
            return (IEnumerable<T>)InstantiateEnumerable(typeof(T), typeof(T));
        }

        private object InstantiateEnumerable(Type type, Type currType)
        {
            RegisteredTypeInfo registeredType = _configuration.GetImplementation(type);
            if (registeredType != null)
            {
                IList collection = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
                IEnumerable<RegisteredTypeInfo> registeredTypes = _configuration.GetAllImplementations(type);
                foreach (RegisteredTypeInfo item in registeredTypes)                
                    collection.Add(GetInstance(item, currType));                
                return collection;
            }
            return null;
        }

        private object GetInstance(RegisteredTypeInfo registeredType, Type currType)
        {
            if (registeredType.Lifecycle == LifecycleType.Singleton)
            {
                if (registeredType.Instance == null)
                {
                    lock (_ob)
                    {
                        if (registeredType.Instance == null)                        
                            registeredType.Instance = Instantiate(registeredType.InterfaceType, currType);                        
                    }
                }
                return registeredType.Instance;
            }
            else
            {
                object createdInst = Instantiate(registeredType.InterfaceType, currType);
                return createdInst;
            }
        }

        private object Instantiate(Type type, Type currType)
        {
            Console.WriteLine("1: " + type + " and 2: " + currType);
            RegisteredTypeInfo registeredType = _configuration.GetImplementation(type);
            if (!(registeredType == null))
            {
                if (!_dependenciesStack.Contains(registeredType.InterfaceType))
                {
                    _dependenciesStack.Push(registeredType.InterfaceType);
                    Type typeToInstantiate = registeredType.ImplementationType;

                    if (typeToInstantiate.IsGenericTypeDefinition)                    
                        typeToInstantiate = typeToInstantiate.MakeGenericType(currType.GenericTypeArguments);
                    
                    ConstructorInfo[] constructors = typeToInstantiate.GetConstructors().OrderByDescending(x => x.GetParameters().Length).ToArray();

                    int currentConstructor = 1;
                    bool createdSuccessfully = false;
                    object result = null;

                    while (!createdSuccessfully && currentConstructor <= constructors.Length)
                    {
                        try
                        {
                            ConstructorInfo constructorInfo = constructors[currentConstructor - 1];
                            object[] constructorParam = GetConstructorParameters(constructorInfo, currType);
                            result = Activator.CreateInstance(typeToInstantiate, constructorParam);
                            createdSuccessfully = true;
                        }
                        catch
                        {
                            createdSuccessfully = false;
                            currentConstructor++;
                        }
                    }

                    _dependenciesStack.Pop();
                    if (createdSuccessfully)                    
                        return result;                                      
                }                
            }
            return null;
        }

        private object[] GetConstructorParameters(ConstructorInfo constructorInfo, Type currType)
        {
            ParameterInfo[] parametersInfo = constructorInfo.GetParameters();
            object[] parameters = new object[parametersInfo.Length];
            for (int i = 0; i < parametersInfo.Length; i++)            
                parameters[i] = GetInstance(_configuration.GetImplementation(parametersInfo[i].ParameterType), currType);
            return parameters;
        }
    }
}
