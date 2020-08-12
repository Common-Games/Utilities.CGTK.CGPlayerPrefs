using System;

namespace CommonGames.Utilities.CGTK.CGPlayerPrefs
{
    using Utils;

    // Explanation of SaveData structure.
    //
    //[Static Saves]
    //    <OwnerType> ownerTypes
    //        key(Transform)
    //        -> <object> staticSaves
    //
    // 
    //[Instanced Saves]
    //    Scenes
    //      key("main")
    //      -> <OwnerType> ownerTypes
    //          key(Transform)
    //          -> <OwnerInstance> ownerInstances
    //              key("1")
    //              -> <object> instanceSaves
    //                  key("position" = Vector3.zero)
    //                  key("rotation" = Quaternion.identity)
    //                  key("scale" = Vector3.one)
    //              ("2")
    //              -> <object> instanceSaves
    //                  key("position" = new Vector3(100, 20, -50))
    //                  ("rotation" = Quaternion.identity)
    //                  ("scale" = Vector3.one)
    //      -> <OwnerType> ownerTypes
    //          key(Health)
    //          -> <OwnerInstance> ownerInstances
    //              key("1")
    //              -> <object> instanceSaves
    //                  key("healthPercentage" = 90)

    #region Dictionaries

    [Serializable] public class StringSceneDict : SerializableDictionary<string, Scene> {}
    
    [Serializable] public class TypeOwnerTypeDict : SerializableDictionary<Type, OwnerType> {}
    
    [Serializable] public class StringOwnerInstanceDict : SerializableDictionary<string, OwnerInstance> {}
    
    [Serializable] public class StringObjectDict : SerializableDictionary<string, object> {}

    #endregion
    
    [Serializable]
    public class SaveData
    {
        #region Variables

        public StringSceneDict scenes = new StringSceneDict();

        public TypeOwnerTypeDict staticOwnerTypes = new TypeOwnerTypeDict();

        #endregion
        
        #region Methods

        public void Save(in Type ownerType, in string saveName, in object saveValue)
        {
            Save(
                ownerType: ownerType, 
                ownerScene: null, 
                ownerInstanceID: null, 
                saveName: saveName, 
                saveValue: saveValue);
        }
        //TODO -Walter- Maybe set OwnerType dictionaries based on if it's static or not.
        
        public void Save(Type ownerType, string ownerScene, string ownerInstanceID, string saveName, object saveValue)
        {
            if(string.IsNullOrEmpty(ownerScene) || string.IsNullOrEmpty(ownerInstanceID))
            {
                __SetStaticSave();
            }
            else
            {
                __SetInstancedSave();
            }

            void __SetStaticSave()
            {
                OwnerType __owner = __GetOwnerType(dictionary: ref staticOwnerTypes);

                //Set static saves
                __SetSave(dictionary: ref __owner.staticSaves);
            }
            void __SetInstancedSave()
            {
                OwnerType __ownerType = __GetOwnerType(dictionary: ref __GetScene().ownerTypes);

                OwnerInstance __ownerInstance = __GetOwnerInstance(dictionary: ref __ownerType.ownerInstances);

                //Set instance saves
                __SetSave(dictionary: ref __ownerInstance.instanceSaves);
            }
            
            Scene __GetScene()
            {
                if(!scenes.ContainsKey(ownerScene))
                {
                    scenes.Add(key: ownerScene, value: new Scene());
                }
                    
                return scenes[ownerScene];
            }
            
            OwnerType __GetOwnerType(ref TypeOwnerTypeDict dictionary)
            {
                if(!dictionary.ContainsKey(ownerType))
                {
                    dictionary.Add(key: ownerType, value: new OwnerType());
                }
                    
                return dictionary[ownerType];
            }

            OwnerInstance __GetOwnerInstance(ref StringOwnerInstanceDict dictionary)
            {
                if(!dictionary.ContainsKey(ownerInstanceID))
                {
                    dictionary.Add(key: ownerInstanceID, value: new OwnerInstance());
                }
                    
                return dictionary[ownerInstanceID];
            }

            void __SetSave(ref StringObjectDict dictionary)
            {
                if(!dictionary.ContainsKey(saveName))
                {
                    dictionary.Add(key: saveName, value: new OwnerType());
                }
                    
                //Debug.Log($"Set Variable = {saveValue}");
                
                dictionary[saveName] = saveValue;
            }
        }
        
        #endregion
        
    }

    [Serializable]
    public class Scene
    {
        public TypeOwnerTypeDict ownerTypes = new TypeOwnerTypeDict();
    }

    [Serializable]
    public class OwnerType
    {
        public StringOwnerInstanceDict ownerInstances = new StringOwnerInstanceDict();
        
        public StringObjectDict staticSaves = new StringObjectDict();
    }

    [Serializable]
    public class OwnerInstance
    {
        public StringObjectDict instanceSaves = new StringObjectDict();
    }
    
    //TODO -Walter- Maybe remove scenes from the equation? You've got Instance ID's anyways..
}