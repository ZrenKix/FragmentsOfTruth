using System;

public enum State { On, Off }

[Serializable]
public class GameConfig
{
    // Player
    public float moveSpeed = 5f;
    public float rotationSpeed = 90f;

    // Sonar
    public State sonarState = State.Off;
}