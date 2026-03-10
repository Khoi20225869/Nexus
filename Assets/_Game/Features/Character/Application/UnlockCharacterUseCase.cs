namespace Game.Features.Character.Application
{
    public sealed class UnlockCharacterUseCase
    {
        private readonly ICharacterService _characterService;

        public UnlockCharacterUseCase(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        public bool Execute(string characterId)
        {
            return _characterService.Unlock(characterId);
        }
    }
}
