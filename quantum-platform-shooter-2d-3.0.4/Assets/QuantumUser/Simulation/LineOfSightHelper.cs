namespace Quantum
{
  using Photon.Deterministic;

  /// <summary>
  ///   The <c>LineOfSightHelper</c> provides ways to check the clear path between two positions
  /// </summary>
  public static unsafe class LineOfSightHelper
  {
    // Returns true if there's no static collider between source and target
    /// <summary>
    /// Returns true if there's no static collider between source and target.
    /// </summary>
    /// <param name="frame">The game frame.</param>
    /// <param name="source">The initial position to check.</param>
    /// <param name="target">The final position to check.</param>
    public static bool HasLineOfSight(Frame frame, FPVector2 source, FPVector2 target)
    {
      Physics2D.HitCollection hits = frame.Physics2D.LinecastAll(source, target, -1, QueryOptions.HitStatics);
      for (int i = 0; i < hits.Count; i++)
      {
        EntityRef entity = hits[i].Entity;
        if (entity == EntityRef.None)
        {
          return false;
        }
      }
      return true;
    }
  }
}