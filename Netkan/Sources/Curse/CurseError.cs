namespace CKAN.NetKAN.Sources.Curse
{
    /// <summary>
    /// Internal class to read errors from SD.
    /// </summary>
    internal class CurseError
    {
        // Currently only used via JsonConvert.DeserializeObject which the compiler
        // doesn't pick up on.
#pragma warning disable 0649
        public int code;
        public string error;
        public string message;
        public string game;
#pragma warning restore 0649
    }
}