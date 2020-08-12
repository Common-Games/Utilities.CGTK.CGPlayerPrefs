namespace CommonGames.Utilities.CGTK.CGPlayerPrefs.Utils
{
    using System.Reflection;
    using JetBrains.Annotations;

    public static class PropertyInfoExtensions
    {
        [PublicAPI]
        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            MethodInfo __get = propertyInfo.GetGetMethod();
            MethodInfo __set = propertyInfo.GetSetMethod();
            
            if (__get != null)
            {
                return __get.IsStatic;
            }

            if (__set != null)
            {
                return __set.IsStatic;
            }
            
            return false;
        }
    }
}
