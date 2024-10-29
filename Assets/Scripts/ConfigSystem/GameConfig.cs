using System;

public enum State { On, Off }

[Serializable]
public class GameConfig
{
    // Player
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 50;

    // Sonar
    public State sonarState = State.Off;
}