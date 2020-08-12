namespace CommonGames.Utilities.CGTK.CGPlayerPrefs
{

    public abstract class SaveableMonoBehaviour : UUID
    {
        //when the object gets created, register it
        protected virtual void OnEnable()
        {
            SaveManager.RegisterInstance(owner: this, id: ID);
        }
        
        //when the object gets destroyed, unregister it
        protected virtual void OnDisable()
        {
            SaveManager.UnregisterInstance(id: ID);
        }
    }

}