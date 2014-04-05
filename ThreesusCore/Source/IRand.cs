using System.Collections.Generic;

namespace Threesus
{
	/// <summary>
	/// Exposes methods that allow numbers and values to be pseudorandomly generated.
	/// </summary>
	public interface IRand
	{
		/// <summary>
		/// Returns a pseudorandom 8-bit integer.
		/// </summary>
		byte UInt8();

		/// <summary>
		/// Returns a pseudorandom 8-bit integer between low and high, inclusive.
		/// </summary>
		byte UInt8(byte low, byte high);

		/// <summary>
		/// Returns a pseudorandom 16-bit integer.
		/// </summary>
		short Int16();

		/// <summary>
		/// Returns a pseudorandom 16-bit integer between low and high, inclusive.
		/// </summary>
		short Int16(short low, short high);

		/// <summary>
		/// Returns a pseudorandom 32-bit integer.
		/// </summary>
		int Int32();

		/// <summary>
		/// Returns a pseudorandom 32-bit integer between low and high, inclusive.
		/// </summary>
		int Int32(int low, int high);

		/// <summary>
		/// Returns a pseudorandom 64-bit integer.
		/// </summary>
		long Int64();

		/// <summary>
		/// Returns a pseudorandom 64-bit integer between low and high, inclusive.
		/// </summary>
		long Int64(long low, long high);

		/// <summary>
		/// Returns a pseudorandom single-precision floating-point number between 0 and 1, inclusive.
		/// </summary>
		float Single();

		/// <summary>
		/// Returns a pseudorandom single-precision floating-point number between low and high, inclusive.
		/// </summary>
		float Single(float low, float high);

		/// <summary>
		/// Returns a pseudorandom double-precision floating-point number between 0 and 1, inclusive.
		/// </summary>
		double Double();

		/// <summary>
		/// Returns a pseudorandom double-precision floating-point number between low and high, inclusive.
		/// </summary>
		double Double(double low, double high);

		/// <summary>
		/// Returns a pseudorandom boolean, either true of false.
		/// </summary>
		bool Boolean();

		/// <summary>
		/// Fills a byte array with pseudorandom bytes.
		/// </summary>
		/// <param name="buf">The array to which pseudorandom bytes are written.</param>
		void FillBytes(byte[] buf);

		/// <summary>
		/// Fills a byte array with pseudorandom bytes.
		/// </summary>
		/// <param name="buf">The array to which pseudorandom bytes are written.</param>
		/// <param name="count">The index in the array at which to start writing pseudorandom bytes.</param>
		/// <param name="index">The number of pseudorandom bytes to write.</param>
		void FillBytes(byte[] buf, int index, int count);

		/// <summary>
		/// Randomizes the order of the specified array.
		/// </summary>
		void RandomizeOrder<T>(T[] array);

		/// <summary>
		/// Randomizes the order of the specified array.
		/// </summary>
		/// <param name="index">The starting index of the elements to randomize.</param>
		/// <param name="count">The number of elements from the starting index to randomize.</param>
		void RandomizeOrder<T>(T[] array, int index, int count);

		/// <summary>
		/// Randomizes the order of the specified list.
		/// </summary>
		void RandomizeOrder<T>(IList<T> list);

		/// <summary>
		/// Randomizes the order of the specified list.
		/// </summary>
		/// <param name="index">The starting index of the elements to randomize.</param>
		/// <param name="count">The number of elements from the starting index to randomize.</param>
		void RandomizeOrder<T>(IList<T> list, int index, int count);

		/// <summary>
		/// Returns an element selected at random from the specified array.
		/// </summary>
		T SelectElement<T>(T[] array);

		/// <summary>
		/// Returns an element selected at random from the specified array.
		/// </summary>
		/// <param name="index">The starting index of the subset of elements to select from.</param>
		/// <param name="count">The number of elements from the starting index to select from.</param>
		T SelectElement<T>(T[] array, int index, int count);

		/// <summary>
		/// Returns an element selected at random from the specified list.
		/// </summary>
		T SelectElement<T>(IList<T> list);

		/// <summary>
		/// Returns an element selected at random from the specified list.
		/// </summary>
		/// <param name="index">The starting index of the subset of elements to select from.</param>
		/// <param name="count">The number of elements from the starting index to select from.</param>
		T SelectElement<T>(IList<T> list, int index, int count);
	}
}