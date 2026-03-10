namespace Game.Features.Character.Domain
{
    public readonly struct CharacterSwitchedEvent : Game.Core.Events.IGameEvent
    {
        public CharacterSwitchedEvent(string characterId)
        {
            CharacterId = characterId;
        }

        public string CharacterId { get; }
    }
}
