namespace Troublecat.Core.Assets.Fonts;

public readonly struct Kerning
{
    public Kerning()
    {
    }

    public int First { get; init; }
    public int Second { get; init; }
    public int Amount { get; init; }

}
