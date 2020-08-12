using System;

namespace CommonGames.Utilities.CGTK.CGPlayerPrefs
{
    [Serializable]
    public class SaveAttribute : Attribute
    {
        public string name;

        public SaveAttribute(string name = "")
        {
            this.name = name; //Sanitize(name);
        }
    }
}