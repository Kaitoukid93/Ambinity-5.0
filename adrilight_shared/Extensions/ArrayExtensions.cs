using System.Collections.Generic;

namespace adrilight_shared.Extensions
{
    public static class ArrayExtensions
    {
        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            var temp = array[index2];
            array[index2] = array[index1];
            array[index1] = temp;
        }
        public static void RemoveRange(this List<string> target, List<string> input)
        {
            if (input == null || input.Count == 0) return;
            foreach (var p in input)
            {
                if (target.Contains(p))
                    target.Remove(p);
            }

        }
    }
}
