namespace WizardsCode.Git
{
    internal struct GitLogEntry
    {
        public string hash;
        public string description;

        internal GitLogEntry(string hash, string description)
        {
            this.hash = hash;
            this.description = description;
        }
    }
}