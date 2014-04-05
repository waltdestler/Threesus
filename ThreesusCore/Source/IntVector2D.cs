using System;
using System.Runtime.CompilerServices;

namespace Threesus
{
	/// <summary>
	/// Stores a 2-dimensional point or displacement in space as two integer values.
	/// </summary>
	public struct IntVector2D : IEquatable<IntVector2D>, IComparable, IComparable<IntVector2D>
	{
		#region Public Static Constants

		public static readonly IntVector2D Zero = new IntVector2D();
		public static readonly IntVector2D One = new IntVector2D(1, 1);

		#endregion
		#region Public Fields

		public int X;
		public int Y;

		#endregion
		#region Constructors

		/// <summary>
		/// Creates a new IntVector2D from the specified x and y values.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntVector2D(int x, int y)
		{
			X = x;
			Y = y;
		}

		#endregion
		#region Public Methods

		/// <summary>
		/// Returns whether this IntVector2D is equal to the specified object.
		/// </summary>
		public override bool Equals(object obj)
		{
			if(obj is IntVector2D)
				return Equals((IntVector2D)obj);
			else
				return false;
		}

		/// <summary>
		/// Returns whether this IntVector2D is equal to the specified IntVector2D.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(IntVector2D v)
		{
			return X == v.X && Y == v.Y;
		}

		/// <summary>
		/// Compares this IntVector2D to the specified object.
		/// </summary>
		int IComparable.CompareTo(object obj)
		{
			return CompareTo((IntVector2D)obj);
		}

		/// <summary>
		/// Compares this vector to the specified IntVector2D.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int CompareTo(IntVector2D v)
		{
			int result = X.CompareTo(v.X);
			if(result != 0)
				return result;
			return Y.CompareTo(v.Y);
		}

		/// <summary>
		/// Returns the hash code for this IntVector2D.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + X.GetHashCode();
				hash = hash * 23 + Y.GetHashCode();
				return hash;
			}
		}

		/// <summary>
		/// Returns an IntVector2D perpendicular to this IntVector2D.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public IntVector2D Perp()
		{
			return new IntVector2D(-Y, X);
		}

		/// <summary>
		/// Returns the string representation of this IntVector2D.
		/// </summary>
		public override string ToString()
		{
			return "{X=" + X + ",Y=" + Y + "}";
		}

		#endregion
		#region Public Static Methods

		/// <summary>
		/// Computes the dot product of v1 and v2 and returns the resulting value.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DotProduct(IntVector2D v1, IntVector2D v2)
		{
			return v1.X * v2.X + v1.Y * v2.Y;
		}

		/// <summary>
		/// Computes the cross product of v1 and v2 and returns the resulting value.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CrossProduct(IntVector2D v1, IntVector2D v2)
		{
			return v1.X * v2.Y - v2.X * v1.Y;
		}

		/// <summary>
		/// Performs a component-wise multiplication of the specified Vectors.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D ComponentMultiply(IntVector2D v1, IntVector2D v2)
		{
			return new IntVector2D(v1.X * v2.X, v1.Y * v2.Y);
		}

		#endregion
		#region Operators

		/// <summary>
		/// Returns whether the specified Vectors are equal.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(IntVector2D v1, IntVector2D v2)
		{
			return v1.X == v2.X && v1.Y == v2.Y;
		}

		/// <summary>
		/// Returns whether the specified Vectors are not equal.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(IntVector2D v1, IntVector2D v2)
		{
			return v1.X != v2.X || v1.Y != v2.Y;
		}

		/// <summary>
		/// Returns the summation of the specified vectors.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D operator +(IntVector2D v1, IntVector2D v2)
		{
			v1.X += v2.X;
			v1.Y += v2.Y;
			return v1;
		}

		/// <summary>
		/// Returns the subtraction of v2 from v1.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D operator -(IntVector2D v1, IntVector2D v2)
		{
			v1.X -= v2.X;
			v1.Y -= v2.Y;
			return v1;
		}

		/// <summary>
		/// Returns the negation of the specified IntVector2D.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D operator -(IntVector2D v)
		{
			v.X = -v.X;
			v.Y = -v.Y;
			return v;
		}

		/// <summary>
		/// Returns the component-wise multiplication of the specified IntVector2D and factor.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D operator *(IntVector2D v, int factor)
		{
			v.X *= factor;
			v.Y *= factor;
			return v;
		}

		/// <summary>
		/// Returns the component-wise multiplication of the specified factor and IntVector2D.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D operator *(int factor, IntVector2D v)
		{
			v.X *= factor;
			v.Y *= factor;
			return v;
		}

		/// <summary>
		/// Returns the component-wise division of the specified IntVector2D numerator by the specified denominator.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D operator /(IntVector2D v, int denominator)
		{
			v.X /= denominator;
			v.Y /= denominator;
			return v;
		}

		/// <summary>
		/// Returns the component-wise division of the specified numerator by the specified IntVector2D denominator.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IntVector2D operator /(int numerator, IntVector2D v)
		{
			v.X = numerator / v.X;
			v.Y = numerator / v.Y;
			return v;
		}

		#endregion
	}
}