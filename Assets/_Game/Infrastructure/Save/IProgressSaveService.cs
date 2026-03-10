namespace Game.Infrastructure.Save
{
    public interface IProgressSaveService
    {
        void Save(PlayerProgressData data);
        PlayerProgressData Load();
    }
}
