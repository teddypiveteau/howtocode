using System;
using System.Collections.Generic;
using System.Linq;

namespace HowToUseReflexion
{
    public static class ReflexionHelper
    {
        #region constructor and instances

        public static T GetNewInstance<T>() where T : new()
        {
            var type = typeof(T);
            var constructorInfo = type.GetConstructor(Type.EmptyTypes);

            if (constructorInfo == null)
                throw new Exception($"Sorry, but there is no default constructor in {type.Name}");

            return (T)constructorInfo.Invoke(new object[] { });
        }

        public static T GetNewInstance<T>(object[] parameters) where T : new()
        {
            if (parameters.Any(p => p == null))
                throw new ArgumentException("Parameters cannot have a null parameter, because we have to get the type of each parameter to match the wanted constructor");

            var type = typeof(T);
            var paramTypes = parameters.Select(p => p.GetType()).ToArray();
            var constructorInfo = type.GetConstructor(paramTypes);

            if (constructorInfo == null)
                throw new Exception($"Sorry. But we do not have found your ({string.Join(", ", paramTypes.Select(t => t.Name))}) constructor");

            return (T)constructorInfo.Invoke(parameters);
        }

        #endregion

        #region Properties

        public static List<string> GetPropertyNames<T>()
        {
            var type = typeof(T);
            var propertyInfos = type.GetProperties();

            return propertyInfos.Select(p => p.Name).ToList();
        }

        public static bool HasProperty<T>(string propertyName)
        {
            var type = typeof(T);

            return type.GetProperty(propertyName) != null;
        }

        public static bool HasProperty<T>(string propertyName, Type propertyType)
        {
            var type = typeof(T);

            return type.GetProperty(propertyName, propertyType) != null;
        }

        #endregion

        #region Property values

        public static object GetPropertyValue<T>(T instance, string propertyName)
        {
            if (instance == null)
                return null;

            var type = typeof(T);
            var propertyInfo = type.GetProperty(propertyName);

            if (propertyInfo == null)
                throw new Exception($"Sorry. But we do not have found the ({type.Name}.{propertyName}) property");
            
            return propertyInfo.GetValue(instance);
        }

        public static object GetPropertyValue<T>(T instance, string propertyName, Type propertyType)
        {
            if (instance == null)
                return null;

            var type = typeof(T);
            var propertyInfo = type.GetProperty(propertyName, propertyType);

            if (propertyInfo == null)
                throw new Exception($"Sorry. But we do not have found the ({type.Name}.{propertyName}) property");

            return propertyInfo.GetValue(instance);
        }

        public static bool SetPropertyValue<T,P>(T instance, string propertyName, P propertyValue)
        {
            if (instance == null)
                return false;

            var type = instance.GetType();
            var propertyInfo = type.GetProperty(propertyName, typeof(P));

            if (propertyInfo == null)
                throw new Exception($"Sorry. But we do not have found the ({type.Name}.{propertyName}) property");

            propertyInfo.SetValue(instance, propertyValue);

            return true;
        }

        #endregion
    }
}
