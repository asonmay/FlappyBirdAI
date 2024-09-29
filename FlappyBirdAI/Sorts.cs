using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlappyBirdAI
{
    static class Sorts
    {
        public static Bird[] MergeSort(Bird[] values)
        {
            if (values.Length < 2)
            {
                return values;
            }

            int middle = values.Length / 2;
            Bird[] left = new Bird[values.Length - middle];
            Bird[] right = new Bird[middle];
            for (int i = 0; i < values.Length; i++)
            {
                if (i < left.Length) left[i] = values[i];
                else right[i - left.Length] = values[i];
            }

            return Merge(MergeSort(left), MergeSort(right));
        }

        private static Bird[] Merge(Bird[] left, Bird[] right)
        {
            Bird[] output = new Bird[left.Length + right.Length];
            int firstIndex = 0;
            int secondIndex = 0;
            int outputIndex = 0;

            while (firstIndex < left.Length && secondIndex < right.Length)
            {
                if (left[firstIndex].Fitness.CompareTo(right[secondIndex].Fitness) > 0)
                {
                    output[outputIndex] = left[firstIndex];
                    firstIndex++;
                }
                else
                {
                    output[outputIndex] = right[secondIndex];
                    secondIndex++;
                }

                outputIndex++;
            }

            while (firstIndex < left.Length)
            {
                output[outputIndex] = left[firstIndex];
                outputIndex++;
                firstIndex++;
            }

            while (secondIndex < right.Length)
            {
                output[outputIndex] = right[secondIndex];
                outputIndex++;
                secondIndex++;
            }

            return output;
        }
    }
}
