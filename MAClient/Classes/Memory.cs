using System;

namespace MAClient.Classes
{
	public static class Memory
	{
		private static readonly int MB = 1024 * 1024;


		public static double used()
		{

			return 0.5; //(maxMemory - System.GC.GetTotalMemory) / MB;
		}

		public static double free()
		{
			return 0.5; //RUNTIME.freeMemory() / MB;
		}

		public static double total()
		{
			return 1; //maxMemory / MB;
		}

		public static double max()
		{
			return 2; //RUNTIME.maxMemory() / MB;
		}

		public static string stringRep()
		{
			return string.Format("[Used: {0} MB, Free: {1} MB, Alloc: {2} MB, MaxAlloc: {3} MB]", used(), free(), total(), max());
		}
	}
}