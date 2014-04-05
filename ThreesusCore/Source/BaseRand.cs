using System;
using System.Collections.Generic;

namespace Threesus
{
	/// <summary>
	/// A base class for classes that implements the IRand interface.
	/// </summary>
	public abstract class BaseRand : IRand
	{
		#region Public Methods

		/// <summary>
		/// Returns a pseudorandom 8-bit integer.
		/// </summary>
		public virtual byte UInt8()
		{
			unchecked
			{
				return (byte)Int64(byte.MinValue, byte.MaxValue);
			}
		}

		/// <summary>
		/// Returns a pseudorandom 8-bit integer between low and high, inclusive.
		/// </summary>
		public virtual byte UInt8(byte low, byte high)
		{
			unchecked
			{
				return (byte)Int64(low, high);
			}
		}

		/// <summary>
		/// Returns a pseudorandom 16-bit integer.
		/// </summary>
		public virtual short Int16()
		{
			unchecked
			{
				return (short)Int64(short.MinValue, short.MaxValue);
			}
		}

		/// <summary>
		/// Returns a pseudorandom 16-bit integer between low and high, inclusive.
		/// </summary>
		public virtual short Int16(short low, short high)
		{
			unchecked
			{
				return (short)Int64(low, high);
			}
		}

		/// <summary>
		/// Returns a pseudorandom 32-bit integer.
		/// </summary>
		public virtual int Int32()
		{
			unchecked
			{
				return (int)Int64(int.MinValue, int.MaxValue);
			}
		}

		/// <summary>
		/// Returns a pseudorandom 32-bit integer between low and high, inclusive.
		/// </summary>
		public virtual int Int32(int low, int high)
		{
			unchecked
			{
				return (int)Int64(low, high);
			}
		}

		/// <summary>
		/// Returns a pseudorandom 64-bit integer.
		/// </summary>
		public abstract long Int64();

		/// <summary>
		/// Returns a pseudorandom 64-bit integer between low and high, inclusive.
		/// </summary>
		public virtual long Int64(long low, long high)
		{
			if(high < low)
				throw new ArgumentOutOfRangeException("high", "The high parameter must be greater than or equal to the low parameter.");

			// Special case if asking for entire range. Otherwise we'll have an overflow below.
			if(low == long.MinValue && high == long.MaxValue)
				return Int64();

			unchecked
			{
				ulong range = (ulong)(high - low + 1);
				ulong numBuckets = ulong.MaxValue / range;
				ulong max = numBuckets * range;
				ulong r;
				do
				{
					r = (ulong)Int64();
				}
				while(r >= max);
				return (long)(r / numBuckets) + low;
			}
		}

		/// <summary>
		/// Returns a pseudorandom single-precision floating-point number between 0 and 1, inclusive.
		/// </summary>
		public virtual float Single()
		{
			unchecked
			{
				ulong val = (ulong)Int64();
				return (float)val / ulong.MaxValue;
			}
		}

		/// <summary>
		/// Returns a pseudorandom single-precision floating-point number between low and high, inclusive.
		/// </summary>
		public virtual float Single(float low, float high)
		{
			high = Math.Max(low, high);
			return Single() * (high - low) + low;
		}

		/// <summary>
		/// Returns a pseudorandom double-precision floating-point number between 0 and 1, inclusive.
		/// </summary>
		public virtual double Double()
		{
			unchecked
			{
				ulong val = (ulong)Int64();
				return (double)val / ulong.MaxValue;
			}
		}

		/// <summary>
		/// Returns a pseudorandom double-precision floating-point number between low and high, inclusive.
		/// </summary>
		public virtual double Double(double low, double high)
		{
			high = Math.Max(low, high);
			return Double() * (high - low) + low;
		}

		/// <summary>
		/// Returns a pseudorandom boolean, either true of false.
		/// </summary>
		public virtual bool Boolean()
		{
			return Int64() < 0L;
		}

		/// <summary>
		/// Fills a byte array with pseudorandom bytes.
		/// </summary>
		/// <param name="buf">The array to which pseudorandom bytes are written.</param>
		public virtual void FillBytes(byte[] buf)
		{
			if(buf == null)
				throw new ArgumentNullException("buf");

			FillBytes(buf, 0, buf.Length);
		}

		/// <summary>
		/// Fills a byte array with pseudorandom bytes.
		/// </summary>
		/// <param name="buf">The array to which pseudorandom bytes are written.</param>
		/// <param name="count">The index in the array at which to start writing pseudorandom bytes.</param>
		/// <param name="index">The number of pseudorandom bytes to write.</param>
		public virtual void FillBytes(byte[] buf, int index, int count)
		{
			if(buf == null)
				throw new ArgumentNullException("buf");

			for(int i = 0; i < count; i++)
			{
				buf[index + i] = UInt8();
			}
		}

		/// <summary>
		/// Randomizes the order of the specified array.
		/// </summary>
		public virtual void RandomizeOrder<T>(T[] array)
		{
			RandomizeOrder(array, 0, array.Length);
		}

		/// <summary>
		/// Randomizes the order of the specified array.
		/// </summary>
		/// <param name="index">The starting index of the elements to randomize.</param>
		/// <param name="count">The number of elements from the starting index to randomize.</param>
		public virtual void RandomizeOrder<T>(T[] array, int index, int count)
		{
			if(array == null)
				throw new ArgumentNullException("array");

			for(int i = index; i < index + count; i++)
			{
				int i2 = Int32(i, index + count - 1);
				T temp = array[i];
				array[i] = array[i2];
				array[i2] = temp;
			}
		}

		/// <summary>
		/// Randomizes the order of the specified list.
		/// </summary>
		public virtual void RandomizeOrder<T>(IList<T> list)
		{
			RandomizeOrder(list, 0, list.Count);
		}

		/// <summary>
		/// Randomizes the order of the specified list.
		/// </summary>
		/// <param name="index">The starting index of the elements to randomize.</param>
		/// <param name="count">The number of elements from the starting index to randomize.</param>
		public virtual void RandomizeOrder<T>(IList<T> list, int index, int count)
		{
			if(list == null)
				throw new ArgumentNullException("list");

			for(int i = index; i < index + count; i++)
			{
				int i2 = Int32(i, index + count - 1);
				T temp = list[i];
				list[i] = list[i2];
				list[i2] = temp;
			}
		}

		/// <summary>
		/// Returns an element selected at random from the specified array.
		/// </summary>
		public virtual T SelectElement<T>(T[] array)
		{
			return SelectElement(array, 0, array.Length);
		}

		/// <summary>
		/// Returns an element selected at random from the specified array.
		/// </summary>
		/// <param name="index">The starting index of the subset of elements to select from.</param>
		/// <param name="count">The number of elements from the starting index to select from.</param>
		public virtual T SelectElement<T>(T[] array, int index, int count)
		{
			return array[Int32(index, index + count - 1)];
		}

		/// <summary>
		/// Returns an element selected at random from the specified list.
		/// </summary>
		public virtual T SelectElement<T>(IList<T> list)
		{
			return SelectElement(list, 0, list.Count);
		}

		/// <summary>
		/// Returns an element selected at random from the specified list.
		/// </summary>
		/// <param name="index">The starting index of the subset of elements to select from.</param>
		/// <param name="count">The number of elements from the starting index to select from.</param>
		public virtual T SelectElement<T>(IList<T> list, int index, int count)
		{
			return list[Int32(index, index + count - 1)];
		}

		#endregion
	}
}