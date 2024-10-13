public class PlayerMovementStateMachine : StateMachine
{
    public Player Player { get; }
    public PlayerIdlingState IdlingState { get; }
    
    public PlayerRunningState RunningState { get; }

    public PlayerSprintingState SprintingState { get; }

    //cache the states
    public PlayerMovementStateMachine(Player player)
    {
        Player = player;
        
        IdlingState = new PlayerIdlingState(this);
        RunningState = new PlayerRunningState(this);
        SprintingState = new PlayerSprintingState(this);
    }
}
