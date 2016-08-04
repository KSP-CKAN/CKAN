namespace CKAN.NetKAN.Sources.Spacedock
{
    /// <summary>
    /// Internal class to read errors from SD.
    /// </summary>
    internal class SpacedockError
    {
        // Currently only used via JsonConvert.DeserializeObject which the compiler
        // doesn't pick up on.
#pragma warning disable 0649
        public string reason;
        public bool error;
#pragma warning restore 0649
    }
}