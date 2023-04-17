// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Messenger;

public class Program {
    private static readonly HttpClient Client = new ();
    public static void Main(string[] args) {
        Console.WriteLine("Hello, World!");
        Client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000");

        switch (args[0]) {
            case "keyGen":
                KeyGen();
                break;
            case "sendKey":
                SendKey(BigInteger.Parse(args[1]));
                break;
            case "getKey":
                // var key = GetKey();
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

    public static void KeyGen() {
        var pBits = 480;
        var p = PrimeGen.NextPrime(pBits);
        var q = PrimeGen.NextPrime(1024 - pBits);
        var N = p * q;
        var r = (p - 1) * (q - 1);
        BigInteger E = 7;
        var D = ModInverse(E, r);

        var publicKey = new KeyFile(E, N);
        publicKey.Save("public.key");
        var privateKey = new KeyFile(D, N);
        privateKey.Save("private.key");
    }
    
    public static void SendKey(BigInteger key) {
        
    }
    
    // public static BigInteger GetKey() {
    //     
    // }
    
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

    public class KeyFile {
        public List<string> email { get; set; } = new ();
        
        public string key { get; set; }

        
        [JsonConstructor]
        public KeyFile(List<string> email, string key) {
            this.email = email;
            this.key = key;
        }
        
        public KeyFile(BigInteger X, BigInteger N) {
            var XBytes = X.ToByteArray();
            var NBytes = N.ToByteArray();
            var key = new List<byte>();
            key.AddRange(BitConverter.GetBytes(XBytes.Length));
            key.AddRange(XBytes);
            key.AddRange(BitConverter.GetBytes(NBytes.Length));
            key.AddRange(NBytes);
            this.key = Convert.ToBase64String(key.ToArray());
        }

        public void AddEmail(string email) {
            this.email.Add(email);
        }

        public string toJSON() => JsonSerializer.Serialize(this);

        public void Save(string filename) {
            File.WriteAllText(filename, toJSON());
        }

        public static KeyFile? Read(string filename) {
            return JsonSerializer.Deserialize<KeyFile>(File.ReadAllText(filename));
        }

        public override string ToString() => toJSON();
    }
}
