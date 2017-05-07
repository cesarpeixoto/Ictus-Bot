

public enum EntityClanType { AncientClan, ModernClan }
public enum SelectorStateType { Valid, Invalid, Opponent }
public enum CommandType { NoOrder, GoTo, Attack, Follow, CancelOrder }

public struct SelectorHitState
{
    public DinamicEnity[] opponent;
    public DinamicEnity[] allies;
    public int selectorState;

    //public override bool Equals(object Obj)
    //{
    //    SelectorHitState other = (SelectorHitState)Obj;
    //    if (!CompareArrays(this.opponent, other.opponent)) return false;
    //    if (!CompareArrays(this.allies, other.allies)) return false;

    //    return (this.selectorState == other.selectorState);
    //}

    //public static bool operator ==(SelectorHitState that, SelectorHitState other)
    //{
    //    return that.Equals(other);
    //}

    //public static bool operator !=(SelectorHitState that, SelectorHitState other)
    //{
    //    return !that.Equals(other);
    //}

    //private static bool CompareArrays(Entity[] that, Entity[] other)
    //{
    //    if (that == null && other == null)
    //        return true;
    //    if (that != null && other == null)
    //        return false;
    //    if (that == null && other != null)
    //        return false;

    //    if (that.Length != other.Length)
    //        return false;

    //    for (int i = 0; i < that.Length; i++)
    //        if (that[i] != other[i]) { return false; }

    //    return true;
    //}
}

public struct NetworkSelectorHitState
{
    public NetworkDinamicEnity[] opponent;
    public NetworkDinamicEnity[] allies;
    public int selectorState;
}





