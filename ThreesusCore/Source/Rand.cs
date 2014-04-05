using System;

namespace Threesus
{
	/// <summary>
	/// Implements IRand to generate pseudorandom numbers and values based on a fast, and low-memory whose state is serializable.
	/// </summary>
	/// <remarks>Ported from Curran Muhlberger's Java implementation.</remarks>
	public class Rand : BaseRand
	{
		#region Fields


		private ulong _a;
		private ulong _b;

		#endregion
		#region Constructors

		/// <summary>
		/// Creates a new pseudorandom number generator seeded based on the current time and other unpredictable factors.
		/// </summary>
		public Rand()
		{
			unchecked
			{
				Reseed((ulong)DateTime.Now.Ticks);
			}
		}

		/// <summary>
		/// Creates a new pseudorandom number generator seeded to the specified value.
		/// </summary>
		public Rand(ulong seed)
		{
			Reseed(seed);
		}

		/// <summary>
		/// Creates a new pseudorandom number generator seeded to the specified internal state values.
		/// </summary>
		public Rand(ulong a, ulong b)
		{
			_a = a;
			_b = b;
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Sets the specified out parameters with the current internal state of this pseudorandom number generator.
		/// </summary>
		public void GetState(out ulong a, out ulong b)
		{
			a = _a;
			b = _b;
		}

		/// <summary>
		/// Sets the internal state of this pseudorandom number generate to the specified values.
		/// </summary>
		public void SetState(ulong a, ulong b)
		{
			_a = a;
			_b = b;
		}

		/// <summary>
		/// Reseeds this pseudorandom number generator to the specified seed value.
		/// </summary>
		public void Reseed(ulong seed)
		{
			_b = 1L;
			_a = 0x38ecac5fb3251641L ^ seed;
			_a ^= _a >> 17;
			_b = 0xffffda61L * (_b & 0xffffffffL) + (_b >> 32);
			_a ^= _a << 31;
			_a ^= _a >> 8;
			_b = _a ^ _b;
			_a ^= _a >> 17;
			_b = 0xffffda61L * (_b & 0xffffffffL) + (_b >> 32);
			_a ^= _a << 31;
			_a ^= _a >> 8;
			_a = _a ^ _b;
		}

		/// <summary>
		/// Returns a pseudorandom 64-bit integer.
		/// </summary>
		public override long Int64()
		{
			_a ^= _a >> 17;
			_b = 0xffffda61L * (_b & 0xffffffffL) + (_b >> 32);
			_a ^= _a << 31;
			_a ^= _a >> 8;
			ulong ret = _a ^ _b;
			unchecked
			{
				return (long)ret;
			}
		}

		#endregion
	}
}