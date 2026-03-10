namespace Game.Features.Character.Application
{
    public sealed class SwitchCharacterUseCase
    {
        private readonly ICharacterService _characterService;

        public SwitchCharacterUseCase(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        public bool Execute(string characterId)
        {
            return _characterService.Switch(characterId);
        }
    }
}
