namespace CKAN.NetKAN.Sources.Spacedock
{
    /// <summary>
    /// Internal class to read errors from SD.
    /// </summary>
    internal class SpacedockError
    {
        // Currently only used via JsonConvert.DeserializeObject which the compiler
        // doesn't pick up on.
        public string? reason;
        public bool    error;
    }
}
