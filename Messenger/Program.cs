// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Numerics;
using System.Text;
using Messenger;

public class Program {
    public static readonly HttpClient Client = new ();
    private const string Email = "nap7292@rit.edu";
    public static void Main(string[] args) {
        Console.WriteLine("Hello, World!");
        Client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000");

        switch (args[0]) {
            case "keyGen":
                KeyGen();
                break;
            case "sendKey":
                SendKey(args[1]);
                break;
            case "getKey":
                GetKey(args[1]);
                break;
            case "sendMsg":
                SendMsg(args[1]);
                break;
            case "getMsg":
                // var msg = GetMsg();
                break;
            default:
                Console.WriteLine("Invalid option, must be: keyGen, sendKey, getKey, sendMsg, or getMsg");
                break;
        }
    }

    public static (BigInteger, BigInteger, BigInteger) KeyGen() {
        var pBits = 480;
        BigInteger N, r, E = 29, D;
        do {
            var p = PrimeGen.NextPrime(pBits);
            var q = PrimeGen.NextPrime(1024 - pBits);
            N = p * q;
            r = (p - 1) * (q - 1);
            D = ModInverse(E, r);
            // Sometimes has a remainder of 7, try again if so
        } while (D * E % r != BigInteger.One); 

        var publicKey = new Key.Public(E, N);
        publicKey.Save();
        var privateKey = new Key.Private(D, N);
        privateKey.Save();
        return (r, E, D);
    }
    
    public static void SendKey(string email) {
        var privateKey = Key.Private.Read();
        privateKey.AddEmail(email);
        privateKey.Save();
        
        var publicKey = Key.Public.Read();
        publicKey.SetEmail(email);
        // Force the put to complete before returning
        var done = Client.PutAsync($"/Key/{email}", new StringContent(publicKey.toJSON())).Result;
    }
    
    public static void GetKey(string email) {
        var response = Client.GetAsync($"/Key/{email}").Result;
        if (response.StatusCode != HttpStatusCode.OK) {
            throw new ArgumentException($"Email doesn't exist: {response.StatusCode}");
        }
        var key = Key.Public.fromJSON(response.Content.ReadAsStringAsync().Result);
        key.Save($"{email}.key");
    }
    
    public static void SendMsg(string msg) {
        
    }
    
    // public static string GetMsg() {
    //     
    // }
    
    private static BigInteger ModInverse(BigInteger a, BigInteger n) {
        BigInteger i = n, v = 0, d = 1;
        while (a>0) {
            BigInteger t = i/a, x = a;
            a = i % x;
            i = x;
            x = d;
            d = v - t*x;
            v = x;
        }
        v %= n;
        if (v<0) v = (v+n) % n;
        return v;
    }
}
