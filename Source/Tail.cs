using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.Foxeline;

/// <summary>
/// A singular tail.
/// </summary>
public class Tail
{
    #region Constants

    /// <summary>
    /// The length of <see cref="TailNodes"/>.
    /// </summary>
    public const int TailNodeCount = 8;

    #endregion

    /// <summary>
    /// The tail collection to which this tail belongs.
    /// </summary>
    public readonly TailCollection ParentTailCollection;

    /// <summary>
    /// The index of this tail.
    /// </summary>
    public readonly int TailIndex;

    /// <summary>
    /// The list of tail nodes belonging to this tail.
    /// </summary>
    public readonly TailNode[] TailNodes;

    /// <summary>
    /// A singular tail.
    /// </summary>
    /// <param name="tailCollection">
    /// The collection of tails to which this tail should belong.
    /// </param>
    /// <param name="tailIndex">
    /// The index of the tail.
    /// </param>
    internal Tail(TailCollection tailCollection, int tailIndex)
    {
        ParentTailCollection = tailCollection;
        TailIndex = tailIndex;
        TailNodes = new TailNode[TailNodeCount];
        for (int i = 0; i < TailNodeCount; i++)
            TailNodes[i] = new TailNode(this, i);
    }

    /// <summary>
    /// Initializes the tail node positions.
    /// </summary>
    /// <param name="startPosition">
    /// The starting position of each tail node.
    /// </param>
    public void InitializePositions(Vector2 startPosition)
    {
        for (int i = 0; i < TailNodeCount; i++)
            TailNodes[i].Position = startPosition;
    }

    /// <summary>
    /// Moves the tail by the <paramref name="amount"/> specified.
    /// </summary>
    /// <param name="amount">
    /// How much to move each tail by.
    /// </param>
    public void MoveBy(Vector2 amount)
    {
        for (int i = 0; i < TailNodeCount; i++)
            TailNodes[i].Position += amount;
    }

    /// <summary>
    /// Clamps the tail node into reach of the previous tail node
    /// </summary>
    /// <param name="nodeIndex">Tail index</param>
    /// <param name="tailScale">Tail scale multiplier</param>
    public void ClampPosition(int nodeIndex, float tailScale)
    {
        TailNode previousTailNode = TailNodes[nodeIndex - 1];
        TailNode thisTailNode = TailNodes[nodeIndex];

        Vector2 positionDelta = previousTailNode.Position - thisTailNode.Position;
        float thisNodeRadius = thisTailNode.NodeSize * tailScale;

        //use lengthSquared to avoid the square root
        if (positionDelta.LengthSquared() <= thisNodeRadius * thisNodeRadius)
            //already in range
            return;

        thisTailNode.Position = previousTailNode.Position - positionDelta.SafeNormalize() * thisNodeRadius;
    }

    /// <summary>
    /// Draws the outline of the tail.
    /// </summary>
    public void DrawOutlineOnly()
    {
        PlayerHair hair = ParentTailCollection.Hair;

        bool isBigTail = FoxelineHelpers.isBigTail(hair);
        int tailVariant = (int)FoxelineHelpers.getTailVariant(hair) - 1;
        float tailScale = FoxelineHelpers.getTailScale(hair);

        if (isBigTail)
        {
            tailVariant += FoxelineConst.Variants;
            tailScale /= 2;
        }

        foreach (TailNode node in TailNodes.Reverse())
        {
            MTexture tex = FoxelineModule.Instance.TailNodeTextures[tailVariant][node.TextureId];
            Vector2 position = hair.Nodes[0].Floor() + node.Offset.Floor();

            tex.DrawCentered(position + Vector2.UnitX, Color.Black, tailScale);
            tex.DrawCentered(position + Vector2.UnitY, Color.Black, tailScale);
            tex.DrawCentered(position - Vector2.UnitX, Color.Black, tailScale);
            tex.DrawCentered(position - Vector2.UnitY, Color.Black, tailScale);
        }
    }

    public void DrawWithoutOutline(Color[] hairGradient)
    {
        PlayerHair hair = ParentTailCollection.Hair;

        bool isBigTail = FoxelineHelpers.isBigTail(hair);
        bool isPaintBrushTail = FoxelineHelpers.getPaintBrushTail(hair);
        int tailVariant = (int)FoxelineHelpers.getTailVariant(hair) - 1;
        float tailScale = FoxelineHelpers.getTailScale(hair);
        float tailBrushTint = FoxelineHelpers.getTailBrushTint(hair);
        Color tailBrushColor = FoxelineHelpers.getTailBrushColor(hair);

        float tailSoftness = (100 - FoxelineModule.Settings.FoxelineConstants.Softness) / 100f;

        if (isBigTail)
        {
            tailVariant += FoxelineConst.Variants;
            tailScale /= 2;
        }

        int hairCount = hair.Sprite.HairCount;

        foreach (TailNode node in TailNodes.Reverse())
        {
            bool isPaintBrushNode = node.TailNodeIndex < TailNodeCount * tailSoftness;

            //fill color is either a hair node color, or an interpolated hair color between the two nearest nodes
            float lerp = Math.Min(node.NormalizedTailNodeIndex, 1) * hairCount;
            int hairNodeIndex = (int)lerp;
            int nextHairNodeIndex = Math.Min(hairNodeIndex + 1, hairCount - 1);
            Color color = Color.Lerp(hairGradient[hairNodeIndex], hairGradient[nextHairNodeIndex], lerp % 1);

            if (isPaintBrushNode == isPaintBrushTail)
                //it's considered a paint brush node, tint it
                color = Color.Lerp(tailBrushColor, color, tailBrushTint);

            MTexture tex = FoxelineModule.Instance.TailNodeTextures[tailVariant][node.TextureId];
            Vector2 position = hair.Nodes[0].Floor() + node.Offset.Floor();

            tex.DrawCentered(position, color, tailScale);
        }
    }

    /// <summary>
    /// Draws the tail's base position.
    /// </summary>
    public void DrawBasePosition()
        => Draw.Point(TailNodes[0].Position, Color.Cyan);
}
