namespace CommonGames.Utilities.CGTK.CGPlayerPrefs.Utils
{
    using System.Reflection;

    internal static class GetAttributeExtension
    {
        public static SaveAttribute GetSaveAttribute(this MemberInfo member)
        {
            try
            {
                object[] __attributes = member.GetCustomAttributes(typeof(SaveAttribute), false);

                foreach(object __attribute in __attributes)
                {
                    if(__attribute.GetType() != typeof(SaveAttribute)) continue;

                    return __attribute as SaveAttribute;
                }
            }
            catch
            {
                //this could happen due to a TypeLoadException in builds
            }

            return null;
        }
    }
}