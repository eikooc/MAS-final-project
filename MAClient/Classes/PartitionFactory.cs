using System;

namespace MAClient.Classes
{
    public class PartitionFactory
    {
        private static string partitionType;

        public static void Initialize(string arg)
        {
            partitionType = arg;
        }

        public static MapPartition Create(int maxCol, int maxRow)
        {
            switch (partitionType.ToLower().Trim())
            {
                case "-withorder": return new MapPartitionWithOrder(maxCol, maxRow);
                default:
                case "-noorder": return new MapPartitionNoOrder(maxCol, maxRow);
            }
            throw new Exception("Unknown partitioner");
        }
    }
}
