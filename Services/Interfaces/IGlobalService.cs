namespace Services.Interfaces
{
    public interface IGlobalService
    {
        public string DatabaseServer { get; }

        public void SetDatabase(string database);
    }
}
