using System;
using System.Collections.Generic;

// ReSharper disable once RedundantUsingDirective
using UnityEngine;

using JetBrains.Annotations;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

using Sirenix.Serialization;

using MonoBehaviour = Sirenix.OdinInspector.SerializedMonoBehaviour;
#endif

//Written by Wybren
//Modified by Walter
public class UUID : MonoBehaviour 
    #if !ODIN_INSPECTOR
    , ISerializationCallbackReceiver
    #endif
{
    private static readonly Dictionary<UUID, string> StringToUUID = new Dictionary<UUID, string>();
    private static readonly Dictionary<string, UUID> UUIDToString = new Dictionary<string, UUID>();

    private void RegisterUUID()
    {
        string UID;
        if (StringToUUID.TryGetValue(this, out UID))
        {
            // found object instance, update ID
            ID = UID;
            _backupID = ID;
            if (!UUIDToString.ContainsKey(UID))
            {
                UUIDToString.Add(UID, this);
            }

            return;
        }

        if (string.IsNullOrEmpty(ID))
        {
            // No ID yet, generate a new one.
            ID = Guid.NewGuid().ToString();
            _backupID = ID;
            UUIDToString.Add(ID, this);
            StringToUUID.Add(this, ID);

            return;
        }

        UUID tmp;
        if (!UUIDToString.TryGetValue(ID, out tmp))
        {
            // ID not known to the Database, so just register it
            UUIDToString.Add(ID, this);
            StringToUUID.Add(this, ID);

            return;
        }
        if (tmp == this)
        {
            // Database inconsistency
            StringToUUID.Add(this, ID);

            return;
        }
        if (tmp == null)
        {
            // object in Database got destroyed, replace with new
            UUIDToString[ID] = this;
            StringToUUID.Add(this, ID);

            return;
        }

        // we got a duplicate, generate new ID
        ID = Guid.NewGuid().ToString();
        _backupID = ID;
        UUIDToString.Add(ID, this);
        StringToUUID.Add(this, ID);
    }
    private void UnregisterUUID()
    {
        UUIDToString.Remove(ID);
        StringToUUID.Remove(key: this);
    }
    
    #if ODIN_INSPECTOR
    [PropertyOrder(-10)]
    [OdinSerialize] [ReadOnly]
    #else
    //[field: SerializeField]
    #endif
    [PublicAPI]
    public string ID { get; protected set; }

    private string _backupID;

    #if ODIN_INSPECTOR
    protected override void OnAfterDeserialize()
    {
        base.OnAfterDeserialize();
    #else
    public void OnAfterDeserialize()
    {
    #endif
        
        if(ID == null || ID != _backupID)
        {
            RegisterUUID();
        }
    }
    #if ODIN_INSPECTOR
    protected override void OnBeforeSerialize()
    {
        base.OnBeforeSerialize();
    #else
    public void OnBeforeSerialize()
    {
    #endif
    
        if(ID == null || ID != _backupID)
        {
            RegisterUUID();
        }
    }

    protected void OnDestroy()
    {
        UnregisterUUID();
        ID = null;
    }
}
