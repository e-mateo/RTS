using System;

public class Grid
{
	public int Width;
	public int Height;
	public int[] Values;

	public int Size => Width * Height;

	public Grid(int width, int height, int value)
	{
		Width = width;
		Height = height;

		Values = new int[Size];
		for (int i = 0; i < Size; ++i)
			Values[i] = value;
	}

	public bool Contains(int i, int j)
	{
		return i >= 0 && i < Width && j >= 0 && j < Height;
	}

    public void Set(int value, int i, int j)
    {
        Values[i + j * Width] = value;
    }

    public int Get(int i)
    {
        return Values[i];
    }

    public bool IsValue(int value, int i, int j)
    {
        int index = i + j * Width;
        return (index < Values.Length) && (Values[index] & value) > 0;
    }

    public void Clear()
    {
        Array.Clear(Values, 0, Values.Length);
    }
}
