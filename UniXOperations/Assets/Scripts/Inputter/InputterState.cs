public readonly struct InputterState
{
    public InputterState(PlayerInputter playerInputter,
                         AIInputter aIInputter)
    {
        _playerInputter = playerInputter;
        _aIInputter = aIInputter;

        if (playerInputter == null)
        {
            Inputter = _aIInputter;
        }
        else
        {
            Inputter = _playerInputter;
        }
    }

    public InputterBase Inputter { get; }

    public InputterState EnterPlayer(PlayerInputter playerInputter)
    {
        return new InputterState(playerInputter, _aIInputter);
    }

    public InputterState LeavePlayer()
    {
        return new InputterState(null, _aIInputter);
    }



    private readonly PlayerInputter _playerInputter;
    private readonly AIInputter _aIInputter;
}
