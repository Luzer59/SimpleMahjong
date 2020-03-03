public class Id
{
    private static int nextId = 0;

    public static int Get()
    {
        if (nextId == int.MaxValue)
        {
            nextId = 0;
        }
        nextId++;
        return nextId - 1;
    }
}
