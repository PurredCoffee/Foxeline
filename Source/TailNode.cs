using Microsoft.Xna.Framework;

namespace Celeste.Mod.Foxeline;

/// <summary>
/// A singular tail node.
/// </summary>
public class TailNode
{
    #region Constants

    /// <summary>
    /// Tail node sizes, in pixels.
    /// These sizes are currently hardcoded and chosen to be pretty.
    /// </summary>
    public static readonly int[] NodeSizes = [3, 2, 1, 3, 1, 2, 2, 2];

    /// <summary>
    /// Tail node subtexture IDs.
    /// These subtexture IDs are currently hardcoded and chosen to be pretty.
    /// </summary>
    public static readonly int[] NodeTextureId = [0, 2, 3, 4, 4, 3, 1, 0];

    #endregion

    /// <summary>
    /// The tail to which this node belongs.
    /// </summary>
    public readonly Tail ParentTail;

    /// <summary>
    /// The index of this tail node.
    /// </summary>
    public readonly int TailNodeIndex;

    /// <summary>
    /// The tail node size, in pixels.
    /// </summary>
    public readonly int NodeSize;

    /// <summary>
    /// The tail node subtexture ID.
    /// </summary>
    public readonly int TextureId;

    /// <summary>
    /// The absolute node position.
    /// </summary>
    public Vector2 Position = Vector2.Zero;

    /// <summary>
    /// The node position relative to the <see cref="PlayerHair"/>'s bangs node.
    /// </summary>
    public Vector2 Offset = Vector2.Zero;

    /// <summary>
    /// The node velocity.
    /// </summary>
    public Vector2 Velocity = Vector2.Zero;

    /// <summary>
    /// A singular tail node.
    /// </summary>
    /// <param name="tail">
    ///   The tail to which this node should belong.
    /// </param>
    /// <param name="tailNodeIndex">
    ///   The index of the tail node.
    /// </param>
    internal TailNode(Tail tail, int tailNodeIndex)
    {
        ParentTail = tail;
        TailNodeIndex = tailNodeIndex;
        NodeSize = NodeSizes[tailNodeIndex];
        TextureId = NodeTextureId[tailNodeIndex];
    }
}
