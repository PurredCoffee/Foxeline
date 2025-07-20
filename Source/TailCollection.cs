using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoMod.Utils;

namespace Celeste.Mod.Foxeline;

/// <summary>
/// A collection of tails.
/// </summary>
public class TailCollection : IReadOnlyList<Tail>
{
    #region Constants

    /// <summary>
    /// The maximum supported number of tails.
    /// </summary>
    public const int MaxTailCount = 9;

    /// <summary>
    /// The <see cref="MonoMod.Utils.DynamicData"/> key in the <see cref="PlayerHair"/> object which contains the
    /// <see cref="TailCollection"/>.
    /// </summary>
    public const string DynamicDataKey = "foxeline_tailcollection";

    //instantiate the arrays once and keep them around to cut down on GC pressure
    /// <summary>
    /// The order in which tails should be drawn, indexed by tail count.
    /// This list is currently hardcoded and chosen to be pretty.
    /// </summary>
    private static readonly int[][] TailDrawOrders = [
        [], //no tails or invalid tail count
        [0],
        [1, 0],
        [2, 0, 1],
        [2, 1, 3, 0],
        [4, 0, 3, 1, 2],
        [5, 0, 3, 2, 4, 1],
        [6, 0, 5, 1, 4, 2, 3],
        [7, 0, 6, 1, 4, 3, 5, 2],
        [8, 0, 7, 1, 6, 5, 2, 3, 4],
    ];

    #endregion

    /// <summary>
    /// Gets or creates the <see cref="TailCollection"/> for a given <see cref="PlayerHair"/>.
    /// </summary>
    /// <param name="hair">
    /// The <see cref="PlayerHair"/> which should own this tail collection.
    /// </param>
    /// <param name="hairData">
    /// The <see cref="DynamicData"/> for a <see cref="PlayerHair"/> object.
    /// </param>
    public static TailCollection GetOrCreate(PlayerHair hair, DynamicData hairData = null)
    {
        hairData ??= DynamicData.For(hair);

        // we need to instance check ourselves, because MonoMod will cast and subsequently crash when hot reloading
        if (!(hairData.TryGet(DynamicDataKey, out object value) && value is TailCollection tails))
            hairData.Set(DynamicDataKey, tails = new TailCollection(hair));

        return tails;
    }

    /// <summary>
    /// The <see cref="PlayerHair"/> which owns this tail collection.
    /// </summary>
    public readonly PlayerHair Hair;

    /// <summary>
    /// The list of tails in the collection.
    /// </summary>
    private readonly List<Tail> Tails;

    /// <summary>
    /// An array determining the order in which tails should be drawn.
    /// </summary>
    public int[] TailDrawOrder
        => Count is >= 0 and <= MaxTailCount
            ? TailDrawOrders[Count]
            : TailDrawOrders[0];

    /// <summary>
    /// A collection of tails.
    /// </summary>
    /// <param name="hair">
    /// The <see cref="PlayerHair"/> which owns this tail collection.
    /// </param>
    public TailCollection(PlayerHair hair)
    {
        Hair = hair;
        Tails = [];
        EnsureTailsInitialized();
    }

    /// <summary>
    /// Ensures that the number of tails in the collection is at least equal to the number of tails from the settings.
    /// </summary>
    public void EnsureTailsInitialized()
    {
        for (int iTail = Count; iTail < FoxelineHelpers.getTailCount(Hair); iTail++)
            Tails.Add(new Tail(this, iTail));
    }

    /// <summary>
    /// Initializes all tail node positions.
    /// Called by <see cref="FoxelineHooks.PlayerHair_Start"/>.
    /// </summary>
    /// <param name="startPosition">
    /// The starting position of each tail node.
    /// </param>
    public void InitializeTailPositions(Vector2 startPosition)
    {
        foreach (Tail tail in this)
            tail.InitializePositions(startPosition);
    }

    /// <summary>
    /// Moves each tail by the <paramref name="amount"/> specified.
    /// Called by <see cref="FoxelineHooks.PlayerHair_MoveHairBy"/>.
    /// </summary>
    /// <param name="amount">
    /// How much to move each tail by.
    /// </param>
    public void MoveTailsBy(Vector2 amount)
    {
        foreach (Tail tail in this)
            tail.MoveBy(amount);
    }

    /// <summary>
    /// Draws all tails with their outlines.
    /// </summary>
    public void DrawAllTails()
    {
        int[] drawOrder = TailDrawOrder;
        Color[] hairGradient = FoxelineHelpers.getHairGradient(Hair, DynamicData.For(Hair));

        if (!FoxelineHelpers.getSeparateTails(Hair))
        {
            foreach (int tailIndex in drawOrder)
                Tails[tailIndex].DrawOutlineOnly();
            foreach (int tailIndex in drawOrder)
                Tails[tailIndex].DrawWithoutOutline(hairGradient);
            return;
        }

        foreach (int tailIndex in drawOrder)
        {
            Tails[tailIndex].DrawOutlineOnly();
            Tails[tailIndex].DrawWithoutOutline(hairGradient);
        }
    }

    /// <summary>
    /// Draws all tails' base positions.
    /// </summary>
    public void DrawTailBasePositions()
    {
        foreach (int tailIndex in TailDrawOrder)
            Tails[tailIndex].DrawBasePosition();
    }

    #region Implemented members

    /// <summary>
    /// Gets or sets the <see cref="Tail"/> at the specified index.
    /// </summary>
    public Tail this[int index]
    {
        get => Tails[index];
        private set => Tails[index] = value;
    }

    /// <summary>
    /// Gets the number of tails from settings. May be less than the number of tails in the collection.
    /// </summary>
    public int Count => Math.Min(FoxelineHelpers.getTailCount(Hair), Tails.Count);

    /// <inheritdoc />
    public IEnumerator<Tail> GetEnumerator()
        => new TailEnumerator(this);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private sealed class TailEnumerator(TailCollection tails) : IEnumerator<Tail>
    {
        private TailCollection Tails = tails;
        private int Count = tails.Count;
        private int TailIndex = -1;

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (TailIndex == Count - 1)
                return false;

            Current = Tails[++TailIndex];
            return true;
        }

        /// <inheritdoc />
        public void Reset()
        {
            TailIndex = -1;
            Count = Tails.Count;
            Current = null;
        }

        /// <inheritdoc />
        public Tail Current { get; private set; }

        /// <inheritdoc />
        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose()
        {
            ObjectDisposedException.ThrowIf(Tails is null, this);

            Tails = null;
            Count = -1;
            TailIndex = -1;
            Current = null;
        }
    }

    #endregion
}
