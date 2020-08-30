using UnityEngine;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using JetBrains.Annotations;

#if ODIN_INSPECTOR
using Sirenix.Serialization;
#endif

using Newtonsoft.Json;

namespace CommonGames.Utilities.CGTK.CGPlayerPrefs
{
    using Utils;

    public static class SaveManager
    {
        #region Properties

        #region Assemblies
        
        private static List<(Assembly assembly, Type[] types)> _assemblies = null;
        
        /// <summary> List of Tuples of all Assemblies and the types within them. </summary>
        [PublicAPI]
        public static List<(Assembly assembly, Type[] types)> Assemblies
        {
            get
            {
                if(_assemblies != null) return _assemblies;
                
                Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                _assemblies = new List<(Assembly assembly, Type[] types)>();
                
                foreach(Assembly __assembly in allAssemblies)
                {
                    Type[] __types = __assembly.GetTypes();
                    _assemblies.Add((__assembly, __types));
                }

                return _assemblies;
            }
        }

        #endregion

        #region Saveables
        
        private static List<Saveable> _saveables = null;
        
        [PublicAPI]
        public static List<Saveable> Saveables
        {
            get
            {
                if (_saveables == null)
                {
                    FindSaveables();
                }

                return _saveables;
            }
        }

        #endregion

        #region Save Data
        
        public static SaveData SaveData { get; } = new SaveData();

        #if ODIN_INSPECTOR
        [OdinSerialize]
        #endif
        [NonSerialized]
        public static SaveData serializedData;

        #endregion

        #region Save File

        private const string _DEFAULT_SAVE_NAME = "save";
        
        private static string SaveFolder => Application.persistentDataPath + "/saves/";

        private static string SaveFile(in int saveIndex) => $"{SaveFolder}{_DEFAULT_SAVE_NAME}{saveIndex}.json";

        #endregion

        /*
        /// <summary> Override from <see cref="PersistentEnsuredSingleton{T}"/> (makes it so it shows up in the hierarchy) </summary>
        protected override bool IsVisibleInHierarchy => true;
        */

        #endregion

        #region Methods

        #region Instance Registration

        /// <summary> Registers an Owner/<see cref="SaveableMonoBehaviour"/>. </summary>
        [PublicAPI]
        public static void RegisterInstance<T>(in T owner, in string id)
            where T : Behaviour
        {
            Type __type = owner.GetType();
            
            //Debug.Log($"UUID = {id}");

            PropertyInfo[] __properties = __type.GetProperties();
            foreach(PropertyInfo __property in __properties)
            {
                //Skip if the Property doesn't have the Attribute attached.
                if(!Attribute.IsDefined(__property, typeof(SaveAttribute))) continue;
                
                //Skip if the Property is Static.
                if(__property.IsStatic()) continue;

                Saveable __saveable = new Saveable(property: __property, ownerType: __type)
                {
                    OwnerInstanceID = id,
                    OwnerInstanceObject = owner,
                    OwnerInstanceScene = owner.gameObject.scene.name
                };
                
                Saveables.Add(item: __saveable);
            }
            
            FieldInfo[] __fields = __type.GetFields();
            foreach(FieldInfo __field in __fields)
            {
                //Skip if the Field doesn't have the Attribute attached.
                if(!Attribute.IsDefined(__field, typeof(SaveAttribute))) continue;
                
                //Skip if the Field is Static.
                if(__field.IsStatic) continue;
                        
                Saveable __saveable = new Saveable(field: __field, ownerType: __type)
                {
                    OwnerInstanceID = id,
                    OwnerInstanceObject = owner,
                    OwnerInstanceScene = owner.gameObject.scene.name
                };
                
                Saveables.Add(item: __saveable);
            }
        }
        
        /// <summary> Unregisters an Owner/<see cref="SaveableMonoBehaviour"/>. </summary>
        [PublicAPI]
        public static void UnregisterInstance(in string id)
        {
            for(int __index = 0; __index < Saveables.Count; __index++)
            {
                Saveable __saveable = Saveables[index: __index];
                
                if(__saveable.OwnerInstanceID == id)
                {
                    Saveables.Remove(__saveable);
                    __index--;
                }
            }
        }

        #endregion

        /// <summary> Finds all occurrences of <see cref="SaveAttribute"/>, and creates <see cref="Saveable"/>s for them. </summary>
        [PublicAPI]
        public static void FindSaveables()
        {
            if(_saveables != null) return;
            
            _saveables = new List<Saveable>();

            foreach((Assembly _, Type[] __types) in Assemblies)
            {
                foreach(Type __type in __types)
                {
                    PropertyInfo[] __properties = __type.GetProperties();
                    foreach(PropertyInfo __property in __properties)
                    {
                        //Skip if the Property doesn't have the Attribute attached.
                        if(!Attribute.IsDefined(__property, typeof(SaveAttribute))) continue;
                        
                        //Skip if the Property is not Static.
                        if(!__property.IsStatic()) continue;
                        
                        Saveable __saveable = new Saveable(property: __property, ownerType: __type);
                        _saveables.Add(item: __saveable);
                    }

                    FieldInfo[] __fields = __type.GetFields();
                    foreach(FieldInfo __field in __fields)
                    {
                        //Skip if the Field doesn't have the Attribute attached.
                        if(!Attribute.IsDefined(__field, typeof(SaveAttribute))) continue;
                        
                        //Skip if the Field is not Static.
                        if(!__field.IsStatic) continue;
                        
                        Saveable __saveable = new Saveable(field: __field, ownerType: __type);
                        _saveables.Add(item: __saveable);
                    }
                }
            }
        }

        #region Saving & Loading
        
        /// <summary> Saves all <see cref="Saveables"/>'s Data. </summary>
        /// <param name="saveIndex"> The number of the save. </param>
        [PublicAPI]
        public static void SaveAll(in ushort saveIndex = 0)
        {
            //For static Saveables
            foreach(Saveable __saveable in Saveables)
            {
                __saveable.Save();
            }

            string __saveFile = SaveFile(saveIndex);
            
            //Makes sure the save file exists.
            if(!File.Exists(__saveFile))
            {
                if(!Directory.Exists(SaveFolder))
                {
                    Directory.CreateDirectory(path: SaveFolder);
                }

                File.Create(path: __saveFile);
            }

            using(StreamWriter __writer = new StreamWriter(__saveFile))
            {
                string __jsonData = JsonConvert.SerializeObject(SaveData, Formatting.Indented);
            
                __writer.Write(__jsonData);
                __writer.Close();
            }
        }
        
        /// <summary> Loads all <see cref="Saveables"/>'s Data. </summary>
        /// <param name="saveIndex"> The number of the save. </param>
        [PublicAPI]
        public static void LoadAll(in ushort saveIndex = 0)
        {
            string __saveFile = SaveFile(saveIndex);

            if(File.Exists(__saveFile))
            {
                using(StreamReader __reader = new StreamReader(__saveFile))
                {
                    string __jsonData = __reader.ReadToEnd();

                    serializedData = JsonConvert.DeserializeObject<SaveData>(__jsonData);
                    __reader.Close();
                }
            }
            else
            {
                Debug.LogError(message: $"SaveFile {__saveFile} Does NOT exist");
                return;
            }

            //Yuck. Let's come back to this method in a month and overhaul it, see how readable it is *then*.
            
            //Only checks the active Saveables this way.
            foreach(Saveable __saveable in Saveables)
            {
                
                if(__saveable.IsStatic)
                {
                    foreach(KeyValuePair<Type, OwnerType> __staticOwner in serializedData.staticOwnerTypes)
                    {
                        //Owner has the same Type?
                        if(__staticOwner.Key != __saveable.OwnerType) continue;

                        foreach(KeyValuePair<string, object> __save in __staticOwner.Value.staticSaves)
                        {
                            if(__save.Key == __saveable.Name) //Is the same Saveable!
                            {
                                __saveable.Load(__save.Value);
                            }
                        }
                    }
                }
                else
                {
                    //If the saveable isn't static we'll have to search for the correct instance, and set it - if found.
                    
                    foreach(KeyValuePair<string, Scene> __scene in serializedData.scenes)
                    {
                        //Is in the same Scene?
                        if(__scene.Key != __saveable.OwnerInstanceScene) continue;
                        
                        foreach(KeyValuePair<Type, OwnerType> __instanceType in __scene.Value.ownerTypes)
                        {
                            //Owner has the same Type?
                            if(__instanceType.Key != __saveable.OwnerType) continue;
                            
                            foreach(KeyValuePair<string, OwnerInstance> __instance in __instanceType.Value.ownerInstances)
                            {
                                //Is the same Owner Instance?
                                if(__instance.Key != __saveable.OwnerInstanceID) continue;
                                
                                foreach(KeyValuePair<string, object> __save in __instance.Value.instanceSaves)
                                {
                                    if(__save.Key == __saveable.Name) //Is the same Saveable!
                                    {
                                        __saveable.Load(__save.Value);
                                    }
                                }
                            }
                        }
                    }
                }
                
            }
            
        }

        #endregion

        #endregion
    }
    
    //TODO -Walter- Make this class a (non-MonoBehaviour) Singleton.
}
