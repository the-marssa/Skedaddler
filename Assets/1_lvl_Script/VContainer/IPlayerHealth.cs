using System;

public interface IPlayerHealth
{
    int Current { get; }
    int Max { get; }

    
    bool Heal(int amount);

    
    bool TakeDamage(int amount);

   
    event Action<int, int> Changed;
}
