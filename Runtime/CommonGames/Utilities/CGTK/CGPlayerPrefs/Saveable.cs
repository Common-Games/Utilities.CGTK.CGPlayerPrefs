using System;
using System.Reflection;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace CommonGames.Utilities.CGTK.CGPlayerPrefs
{
    using UnityEngine;
    using Utils;
    
    [Serializable]
    public sealed partial class Saveable
    {
        #region Variables

        private MethodInfo _get, _set;
        
        /// <summary> Return name of the member, unless a custom name has been specified by the Attribute. </summary>
        [PublicAPI]
        public string Name 
            => string.IsNullOrEmpty(Attribute.name) ? MemberInfo.Name : Attribute.name;

        public SaveAttribute Attribute { get; private set; }

        #region Owner Info

        public Type OwnerType { get; private set; }

        #region Owner Instance

        public string OwnerInstanceScene { get; set; }
        public string OwnerInstanceID { get; set; }
        public object OwnerInstanceObject { get; set; }

        #endregion

        #endregion
        
        private List<object> _parameters = new List<object>();

        #endregion

        #region Accessors

        [PublicAPI]
        public bool IsStatic
        {
            get
            {
                if (Field != null)
                {
                    return Field.IsStatic;
                }

                if (Property != null)
                {
                    if (_get != null)
                    {
                        return _get.IsStatic;
                    }

                    if (_set != null)
                    {
                        return _set.IsStatic;
                    }
                }

                return false;
            }
        }

        [PublicAPI]
        public MemberInfo MemberInfo
        {
            get
            {
                if (Property != null)
                {
                    return Property;
                }

                if (Field != null)
                {
                    return Field;
                }

                return null;
            }
        }
        
        
        [PublicAPI]
        public PropertyInfo Property { get; private set; }

        [PublicAPI]
        public FieldInfo Field { get; private set; }

        #endregion

        #region Methods

        #region Constructors & Initialization
        
        public Saveable(in PropertyInfo property, in Type ownerType, in SaveAttribute attribute = null)
        {
            this.Property = property;
            this.Attribute = attribute ?? property.GetSaveAttribute();

            this.OwnerType = ownerType;
            
            Initialize();
        }

        public Saveable(in FieldInfo field, in Type ownerType, in SaveAttribute attribute = null)
        {
            this.Field = field;
            this.Attribute = attribute ?? field.GetSaveAttribute();
            
            this.OwnerType = ownerType;
            
            Initialize();
        }
        
        private void Initialize()
        {
            if(Property != null)
            {
                ParameterInfo __parameter = null;

                _get = Property.GetGetMethod();
                _set = Property.GetSetMethod();

                if(_get != null)
                {
                    __parameter = _get.ReturnParameter;
                }

                if(__parameter != null)
                {
                    _parameters.Add(__parameter);
                }
            }
            if (Field != null)
            {
                _parameters.Add(Field);
            }
        }

        #endregion

        #region Saving & Loading

        [PublicAPI]
        public void Save()
        {
            if(IsStatic)
            {
                if(Property != null)
                {
                    object __value = Property.GetValue(obj: null, index: null);
                    
                    SaveManager.SaveData.Save(
                        ownerType: OwnerType,
                        saveName: Name,
                        saveValue: __value);
                    
                    return;
                }

                if(Field != null)
                {
                    object __value =  Field.GetValue(obj: null);
                    
                    SaveManager.SaveData.Save(
                        ownerType: OwnerType,
                        saveName: Name,
                        saveValue: __value);
                    
                    return;
                }
            }
            //Not Static
            else if(OwnerInstanceObject != null)
            {
                if(Property != null)
                {
                    object __value =  Property.GetValue(obj: OwnerInstanceObject);

                    SaveManager.SaveData.Save(
                        ownerType: OwnerType, 
                        ownerScene: OwnerInstanceScene, 
                        ownerInstanceID: OwnerInstanceID, 
                        saveName: Name,
                        saveValue: __value);
                    
                    return;
                }
                
                if(Field != null)
                {
                    object __value =  Field.GetValue(obj: OwnerInstanceObject);

                    SaveManager.SaveData.Save(
                        ownerType: OwnerType, 
                        ownerScene: OwnerInstanceScene, 
                        ownerInstanceID: OwnerInstanceID, 
                        saveName: Name,
                        saveValue: __value);

                    return;
                }
            }
            
            Debug.LogWarning("What the fuck?");
        }

        [PublicAPI]
        public void Load(in object value)
        {
            if(IsStatic)
            {
                if(Property != null)
                {
                    Property.SetValue(obj: null, value);
                }

                if(Field != null)
                {
                    Field.SetValue(obj: null, value);
                }
            }
            else if(OwnerInstanceObject != null)
            {
                if(Property != null)
                {
                    Property.SetValue(obj: OwnerInstanceObject, value as Property.PropertyType);
                }
                
                if(Field != null)
                {
                    Field.SetValue(obj: OwnerInstanceObject, value as Field.FieldType);
                }
            }
        }

        #endregion

        #endregion
        
    }
   
}