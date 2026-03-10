namespace Game.Features.Level.Application
{
    public sealed class CompleteLevelUseCase
    {
        private readonly ILevelService _levelService;

        public CompleteLevelUseCase(ILevelService levelService)
        {
            _levelService = levelService;
        }

        public void Execute()
        {
            _levelService.CompleteCurrentLevel();
        }
    }
}
