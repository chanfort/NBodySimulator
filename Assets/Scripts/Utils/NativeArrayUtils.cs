using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace NBodySimulator
{
    public class NativeArrayUtils
    {
        public static NativeArray<T> Append<T>(NativeArray<T> input, NativeArray<T> elements) where T : struct
        {
            int inputLength = input.Length;
            int elementsLength = elements.Length;

            NativeArray<T> copy = new NativeArray<T>(inputLength + elementsLength, Allocator.Persistent);

            new CopyAppend<T>
            {
                elements = elements,
                input = input,
                copy = copy
            }.Schedule(inputLength + elementsLength, 4).Complete();

            input.Dispose();
            elements.Dispose();

            input = copy;

            return input;
        }

        public static NativeArray<T> RemoveRange<T>(NativeArray<T> input, int startIndex, int nToRemove) where T : struct
        {
            if (startIndex < 0 || startIndex + nToRemove > input.Length)
            {
                return input;
            }

            int inputLength = input.Length;
            int nToAllocate = inputLength - nToRemove;

            NativeArray<T> results = default;
            if (nToAllocate > 0)
            {
                results = new NativeArray<T>(nToAllocate, Allocator.Persistent);
            }

            int j = 0;

            for (int i = 0; i < inputLength; i++)
            {
                if (i < startIndex || i >= startIndex + nToRemove)
                {
                    results[j] = input[i];
                    j++;
                }
            }

            input.Dispose();
            return results;
        }

        public static T[] Append<T>(T[] input, T[] elements)
        {
            int inputLength = input.Length;
            int elementsLength = elements.Length;

            T[] copy = new T[inputLength + elementsLength];

            for (int i = 0; i < inputLength; i++)
            {
                copy[i] = input[i];
            }

            for (int i = 0; i < elementsLength; i++)
            {
                copy[i + inputLength] = elements[i];
            }

            return copy;
        }

        public static T[] RemoveRange<T>(T[] input, int startIndex, int nToRemove) where T : struct
        {
            if (startIndex < 0 || startIndex + nToRemove > input.Length)
            {
                return input;
            }

            int inputLength = input.Length;

            T[] results = new T[inputLength - nToRemove];
            int j = 0;

            for (int i = 0; i < inputLength; i++)
            {
                if (i < startIndex || i >= startIndex + nToRemove)
                {
                    results[j] = input[i];
                    j++;
                }
            }

            return results;
        }

        [BurstCompile]
        public struct CopyAppend<T> : IJobParallelFor where T : struct
        {
            [ReadOnly] public NativeArray<T> elements;
            [ReadOnly] public NativeArray<T> input;
            [WriteOnly] public NativeArray<T> copy;

            public void Execute(int i)
            {
                if (i < input.Length)
                {
                    copy[i] = input[i];
                }
                else
                {
                    copy[i] = elements[i - input.Length];
                }
            }
        }
    }
}
