using System;

public sealed class GmodRandom : Component
{
	public static GmodRandom Instance; // singleton if you want to
	private ulong[] rs = new ulong[4];

	protected override void OnAwake()
	{
		// do not remove this or MSB will all be zero
		if(rs[0] == 0) {
			mathrandomseed(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
			//Log.Info("gmod random not seeded, seeding with unix epoch time");
		}
	}

	/* for the seed 1722039290 the states should be:

		10011100 00100111 00111101 01111110 11000011 11101110 01001100 00110111

		11101000 11011000 01000011 11100001 01010000 01011111 10111110 00111101

		10110000 10100100 01100010 10010111 01001101 00011010 10001100 01001111

		11111000 01101110 00111101 00110110 01011110 01011110 10111101 01001011 */

	// steps a lfsr
	private void TW223_GEN(ref ulong z, ref ulong r, int i, int k, int q, int s)
	{
		z = rs[i];

		z = (((z << q) ^ z) >> (k - s)) ^ ((z & (0xFFFFFFFFFFFFFFFF << (64 - k))) << s);
		r ^= z;

		rs[i] = z;
	}

	// steps the whole tausworthe generator
	// https://www.ams.org/journals/mcom/1996-65-213/S0025-5718-96-00696-5/S0025-5718-96-00696-5.pdf
	private void TW223_STEP(ref ulong z, ref ulong r)
	{
		TW223_GEN(ref z, ref r, 0, 63, 31, 18);
		TW223_GEN(ref z, ref r, 1, 58, 19, 28);
		TW223_GEN(ref z, ref r, 2, 55, 24, 7);
		TW223_GEN(ref z, ref r, 3, 47, 21, 8);
	}

	// steps the prng
	// supposed to return the full 64 bit unsigned int
	// but this is only used to step when seeding???
	// so I guess we're not returning anything
	private void lj_prng_u64()
	{
		ulong z = 0;
		ulong r = 0;
		TW223_STEP(ref z, ref r);
	}

	// step and extract the unsigned 64 bit integer as a double
	private ulong lj_prng_u64d()
	{
		ulong z = 0;
		ulong r = 0;
		TW223_STEP(ref z, ref r);

		// this bit mask somehow makes the value 1.0 <= d < 2.0
		return (r & 0x000fffffffffffff) | 0x3ff0000000000000;
	}

	// this prints the current state of each LFSR
	public void PrintStates()
	{
		Log.Info("=====================================================");
		Log.Info("STATES:");
		for(int i = 0; i < 4; i++) {
			Log.Info(UlongToBinaryString(rs[i]));
		}
		Log.Info("=====================================================");
	}

	private string UlongToBinaryString(ulong value)
	{
		string binary = Convert.ToString((long)value, 2).PadLeft(64, '0');

		for(int i = 8; i < binary.Length; i += 9) {
			binary = binary.Insert(i, " ");
		}

		return binary;
	}

	private string DoubleToBinaryString(double value)
	{
		ulong bits = BitConverter.DoubleToUInt64Bits(value);
		return UlongToBinaryString(bits);
	}

	// basically math.randomseed()
	public void mathrandomseed(double d)
	{
		uint r = 0x11090601;  // 64-k[i] as four 8 bit constants

		int i;

		for(i = 0; i < 4; i++) {
			uint m = 1u << (int)(r & 255);

			r >>= 8;

			d = d * 3.14159265358979323846 + 2.7182818284590452354;

			ulong u64 = BitConverter.DoubleToUInt64Bits(d);

			if(u64 < m) {
				u64 += m;  // ensure k[i] MSB of u[i] are non-zero
			}

			rs[i] = u64;
		}

		for(i = 0; i < 10; i++) {
			lj_prng_u64();
		}
	}

	// basically math.random()
	public double mathrandom(params double[] args)
	{
		double d = BitConverter.UInt64BitsToDouble(lj_prng_u64d()) - 1.0;

		int n = args.Length;
		if(n > 0) {
			double r1 = args[0];

			if(n == 1) {
				d = Math.Floor(d * r1) + 1.0;
			} else {
				double r2 = args[1];
				d = Math.Floor(d * (r2 - r1 + 1.0)) + r1;
			}
		}
		return d;
	}
}
