namespace KirisameY.Numeric.Test.NumericTests;

public class ModifierTests
{
    [Fact]
    public void SingleModifierTransformsValue()
    {
        var numeric = NumericTestUtils.Create(10.0);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 5));

        Assert.Equal(15.0, numeric.Value);
    }

    [Fact]
    public void BaseValueIsUnaffectedByModifiers()
    {
        var numeric = NumericTestUtils.Create(10.0);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 5));

        Assert.Equal(10.0, numeric.BaseValue);
    }

    [Fact]
    public void ModifiersAreAppliedInAscendingOrderNotInsertionOrder()
    {
        var numeric = NumericTestUtils.Create(0.0);
        numeric.AddModifier(new TestModifier(TestOrder.Late, v => v * 10));
        numeric.AddModifier(new TestModifier(TestOrder.Early, v => v + 1));
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        // Early: 0 + 1 = 1, Middle: 1 * 2 = 2, Late: 2 * 10 = 20
        Assert.Equal(20.0, numeric.Value);
    }

    [Fact]
    public void ModifiersWithSameOrderAreAppliedInInsertionOrder()
    {
        var numeric = NumericTestUtils.Create(0.0);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 1));
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v * 3));

        // (0 + 1) * 3 = 3, reversed order would give 1
        Assert.Equal(3.0, numeric.Value);
    }

    [Fact]
    public void ModifiersAcrossOrdersInterleaveWithSameOrderGroups()
    {
        var numeric = NumericTestUtils.Create(1.0);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 1));
        numeric.AddModifier(new TestModifier(TestOrder.Early, v => v * 2));
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 10));

        // Early: 1 * 2 = 2, Middle: (2 + 1) + 10 = 13
        Assert.Equal(13.0, numeric.Value);
    }

    [Fact]
    public void AddModifierExposesModifierInCollection()
    {
        var numeric = NumericTestUtils.Create(0.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v);

        numeric.AddModifier(modifier);

        Assert.Single(numeric.Modifiers);
        Assert.Contains(modifier, numeric.Modifiers);
    }

    [Fact]
    public void ModifiersCollectionCreatedBeforeAddStillTracksNewModifiers()
    {
        var numeric = NumericTestUtils.Create(0.0);
        Assert.Empty(numeric.Modifiers);

        numeric.AddModifier(new TestModifier(TestOrder.Early, v => v));
        numeric.AddModifier(new TestModifier(TestOrder.Late, v => v));

        Assert.Equal(2, numeric.Modifiers.Count);
    }

    [Fact]
    public void RemoveModifierReturnsTrueAndRemovesIt()
    {
        var numeric = NumericTestUtils.Create(0.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 5);
        numeric.AddModifier(modifier);

        Assert.True(numeric.RemoveModifier(modifier));
        Assert.Empty(numeric.Modifiers);
    }

    [Fact]
    public void RemovedModifierNoLongerAffectsValue()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 5);
        numeric.AddModifier(modifier);
        Assert.Equal(15.0, numeric.Value);

        numeric.RemoveModifier(modifier);

        Assert.Equal(10.0, numeric.Value);
    }

    [Fact]
    public void RemoveModifierReturnsFalseWhenModifierWasNeverAdded()
    {
        var numeric = NumericTestUtils.Create(0.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v);

        Assert.False(numeric.RemoveModifier(modifier));
    }

    [Fact]
    public void RemoveModifierReturnsFalseWhenOrderHasNoModifiers()
    {
        var numeric = NumericTestUtils.Create(0.0);
        var added = new TestModifier(TestOrder.Early, v => v);
        var other = new TestModifier(TestOrder.Late, v => v);
        numeric.AddModifier(added);

        Assert.False(numeric.RemoveModifier(other));
        Assert.Single(numeric.Modifiers);
    }

    [Fact]
    public void RemoveModifierReturnsFalseWhenRemovedTwice()
    {
        var numeric = NumericTestUtils.Create(0.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v);
        numeric.AddModifier(modifier);

        Assert.True(numeric.RemoveModifier(modifier));
        Assert.False(numeric.RemoveModifier(modifier));
    }

    [Fact]
    public void OnlyMatchingModifierIsRemoved()
    {
        var numeric = NumericTestUtils.Create(0.0);
        var removed = new TestModifier(TestOrder.Middle, v => v + 100);
        var kept = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(removed);
        numeric.AddModifier(kept);

        numeric.RemoveModifier(removed);

        Assert.Equal(1.0, numeric.Value);
        Assert.Contains(kept, numeric.Modifiers);
    }

    [Fact]
    public void ValueIsRecomputedWhenModifierRaisesUpdated()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        Assert.Equal(11.0, numeric.Value);

        modifier.Transform = v => v + 5;
        modifier.RaiseUpdated();

        Assert.Equal(15.0, numeric.Value);
    }

    [Fact]
    public void ValueIsCachedUntilModifierRaisesUpdated()
    {
        var numeric = NumericTestUtils.Create(10.0);
        var modifier = new TestModifier(TestOrder.Middle, v => v + 1);
        numeric.AddModifier(modifier);
        Assert.Equal(11.0, numeric.Value);

        modifier.Transform = v => v + 5;
        Assert.Equal(11.0, numeric.Value);

        Assert.Equal(1, modifier.ModifyCallCount);
    }

    [Fact]
    public void ValueIsRecomputedAfterModifierIsAdded()
    {
        var numeric = NumericTestUtils.Create(10.0);
        Assert.Equal(10.0, numeric.Value);

        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v * 2));

        Assert.Equal(20.0, numeric.Value);
    }

    [Fact]
    public void ValueReflectsNewBaseValue()
    {
        var numeric = NumericTestUtils.Create(10.0);
        Assert.Equal(10.0, numeric.Value);

        numeric.BaseValue = 20.0;

        Assert.Equal(20.0, numeric.Value);
    }

    [Fact]
    public void ValueWithModifierReflectsNewBaseValue()
    {
        var numeric = NumericTestUtils.Create(10.0);
        numeric.AddModifier(new TestModifier(TestOrder.Middle, v => v + 5));
        Assert.Equal(15.0, numeric.Value);

        numeric.BaseValue = 20.0;

        Assert.Equal(25.0, numeric.Value);
    }
}
