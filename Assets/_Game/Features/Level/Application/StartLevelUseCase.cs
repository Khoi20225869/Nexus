namespace Game.Features.Level.Application
{
    public sealed class StartLevelUseCase
    {
        private readonly ILevelService _levelService;

        public StartLevelUseCase(ILevelService levelService)
        {
            _levelService = levelService;
        }

        public bool Execute(string levelId)
        {
            return _levelService.StartLevel(levelId);
        }
    }
}
