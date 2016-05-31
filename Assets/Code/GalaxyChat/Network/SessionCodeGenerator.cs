using System;

public static class SessionCodeGenerator {
	private static readonly int[] BodyDeltaySmall =
	{
		28, 30, 28, 29, 28, 29, 26, 26, 28, 26, 26, 28, 27, 27, 29,
		30, 27, 27, 13, 28, 28, 17, 26, 28, 26, 24, 33, 29, 28, 28,
		28, 28, 27, 29, 27, 28, 29, 29, 29, 30, 29, 29, 28, 30, 28,
		29, 28, 28, 29, 30, 18
	};

	private static long authCounter1 = 8167260239830188032L;

	private static long authCounter2 = 1037;

	private static long authCounter3 = 94736404;

	private static long authCounter4 = -3106;

	private static string authPass = "5itndg36hj";

	private static string charToInt = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";


	public static string Generate(string sessionCode)
	{
		return fnGetAuthPass1(sessionCode, 0);
	}

	public static string RandomAbc(int count)
	{
		var rand = new Random();
		int size = charToInt.Length-1;
		string result = "";
		for (int k = 0; k < count; ++k){
			result += charToInt[rand.Next(size)];
		}
		return result;
	}
	
	#region Methods

	private static string fnGetAuthPass1(string c, int type)
	{
		var connectCode = c;
		var _connectCode = connectCode.ToCharArray();
		charToInt = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
		authCounter1 = 8167260239830188032L;
		authCounter2 = 1035;//1037;
		authCounter3 = 94736404;
		authCounter4 = -705;//-3106;
		authPass = "5itndg36hj";
		if (type != 0)
		{
			connectCode = c;
			++authCounter2;
		}

		/* big
			* 08-01 18:13:28.627: V/CGame(18182): c1 = 8167260239830188032
			* 08-01 18:13:28.627: V/CGame(18182): c2 = 1037
			* 08-01 18:13:28.627: V/CGame(18182): c3 = 94736404
			* 08-01 18:13:28.627: V/CGame(18182): c4 = -3106
			*/
		/* small
			* 08-03 11:03:51.381: V/CGame(5265): strtoint = 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz
			08-03 11:03:51.381: V/CGame(5265): c1 = 3766083024341434368
			08-03 11:03:51.381: V/CGame(5265): c2 = 1036
			08-03 11:03:51.381: V/CGame(5265): c3 = 94736404
			08-03 11:03:51.381: V/CGame(5265): c4 = -214
			*/
		string s = null;
		long a = 508;
		if (((authCounter4 & 0x80000000) != 0) && ((authCounter2 & 1024) > 0))
		{
			if (authCounter2 % 256 != 233)
			{
				a = a ^ authCounter1;
			}
		}
		else
		{
			a = a ^ authCounter2; // false
		}

		for (var k = 0; k < connectCode.Length; ++k)
		{
			a = a + _connectCode[k] * authCounter1;
		}

		var kkkStatic = authPass.Length;
		long power = 1;
		a = force1(a, kkkStatic, power);
		
		// force2
		var mas = new int[authPass.Length];
		var s_ch = authPass.ToCharArray();
		int n = 1, m = 123;
		var b = force2(mas, a);
		
		// force3
		n = force3(s_ch, mas, n);
		b *= n;

		// force4
		if (force4(kkkStatic, connectCode + authPass, s_ch, a, mas, n) != null)
		{
			s = (b * m).ToString();
		}
		else
		{
			s = new string(s_ch);
		}

		return s;
	}

	private static long force1(long a, int kkk, long power)
	{
		for (var k = kkk - 1; k >= 0; --k)
		{
			a += power * (charToInt.IndexOf(authPass[k]) + 1) + authCounter3;
			power *= 63;
		}

		return a;
	}

	private static int force2(int[] mas, long a)
	{
		mas[0] = (int) ((a + a + authCounter3) * a);
		for (var k = 1; k < mas.Length; ++k)
		{
			mas[k] = (int) ((a + k + mas[k - 1]) * a);
		}

		var rnd = new Random();
		var i = rnd.Next(990000);
		i += 10000;
		return i;
	}

	private static int force3(char[] s_ch, int[] mas, int n)
	{
		/**
			* Р­С‚Рѕ СЂСѓС‡РЅР°СЏ РѕР±С„СѓСЃРєР°С†РёСЏ, РЅРµ РѕР±СЂР°С‰Р°С‚СЊ РІРЅРёРјР°РЅРёСЏ.
			*/
		for (var j = 0; j < mas.Length * 4; ++j)
		{
			var i = j % mas.Length;
			var k = Math.Abs(j * mas[i] + mas[i]) % mas.Length;
			if (force5(s_ch[i], s_ch[k], mas.Length) == false)
			{
				++mas[(i + k) / 2];
				mas[i] += s_ch[k];
				mas[k] += s_ch[i];
				n++;
			}
		}

		return n;
	}

	private static char[] force4(int kkk, string s, char[] s_ch, long a, int[] mas, int n)
	{
		s = new string(s_ch);
		for (var k = 0; k < s.Length; ++k)
		{
			mas[k] += n | charToInt[charToInt.IndexOf(string.Empty + s_ch[k])];
			mas[k] += (int) (kkk + a);
			n += mas[k];
			mas[k] = mas[k] + (int) authCounter3;
			s_ch[k] = charToInt[Math.Abs(mas[k] % charToInt.Length)];
		}

		return null;
	}

	private static bool force5(char a, char b, int d)
	{
		if (Math.Abs(a - b) > d + force6(d))
		{
			return false;
		}

		return true;
	}

	private static int force6(int body)
	{
		return BodyDeltaySmall[Math.Abs(body % BodyDeltaySmall.Length)];
	}

	#endregion
}
