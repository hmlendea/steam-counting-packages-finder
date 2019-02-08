namespace SteamAccountCreator.Processors
{
    public interface IProcessor
    {
        string Name { get; }

        void Close();
    }
}