using JetBrains.Annotations;
using UnityEngine;

public static class GenericExtensions
{
        #region EnsuredGetIfNull

        #region GameObject Context

        [PublicAPI]
        public static T EnsuredGetIfNull<T>(this T obj, in GameObject context) where T : Component
        {
            if(obj != null) return obj;

            return context.TryGetComponent(component: out T __returnedObject) 
                ? __returnedObject 
                : context.AddComponent<T>() as T;
        }

        #endregion

        #region Component Context

        [PublicAPI]
        public static T EnsuredGetIfNull<T>(this T obj, in Component context) where T : Component
            => EnsuredGetIfNull(obj: obj, context: context.gameObject);

        #endregion

        #endregion
}
