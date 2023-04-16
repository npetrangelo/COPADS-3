using System.Numerics;
using System.Security.Cryptography;

namespace Messenger; 

public static class PrimeGen {
    public static BigInteger NextPrime(int bits) {
        if (bits % 8 != 0) {
            throw new ArgumentException("Bits must be a multiple of 8.");
        }
        
        if (bits < 32) {
            throw new ArgumentOutOfRangeException(nameof(bits), "Bits must be at least 32.");
        }
        
        BigInteger prime;
        do {
            prime = new BigInteger(RandomNumberGenerator.GetBytes(bits / 8), true);
        } while (!prime.IsProbablyPrime(bits));

        return prime;
    }

    private static bool IsProbablyPrime(this BigInteger value, int bits, int k = 10) {
        if (value > 0 && value < 3) { // Handle base case
            return true;
        }
        
        int[] lowPrimes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 };
        foreach (var n in lowPrimes) {
            if (BigInteger.Remainder(value, n) == 0) {
                return false;
            }
        }

        var d = BigInteger.Subtract(value, 1);
        var r = 0;
        while (d.IsEven) {
            d = BigInteger.Divide(d, 2);
            r++;
        }
        
        for (var i = 0; i < k; i++) {
            BigInteger a;
            do {
                a = new BigInteger(RandomNumberGenerator.GetBytes(bits/8), true);
            } while (a < 2 || BigInteger.Compare(a, BigInteger.Subtract(value, 2)) > 0);
            var x = BigInteger.ModPow(a, d, value);
            if (x == 1 || BigInteger.Compare(x, BigInteger.Subtract(value, 1)) == 0) {
                continue;
            }

            var y = BigInteger.ModPow(x, 2, value);
            for (var j = 0; j < r; j++) {
                y = BigInteger.ModPow(x, 2, value);
                if (y == 1 && x != 1 && BigInteger.Compare(x, BigInteger.Subtract(value, 1)) != 0) {
                    return false;
                }
                x = y;
            }
            if (y != 1) {
                return false;
            }
        }
        return true;
    }
}