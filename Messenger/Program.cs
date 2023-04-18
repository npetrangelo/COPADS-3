// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json;
using Messenger;

public class Program {
    public static readonly HttpClient Client = new ();
    public static void Main(string[] args) {
        Client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000");
        if (args.Length < 1) {
            Console.WriteLine("Must pick keyGen, sendKey, getKey, sendMsg, or getMsg");
            return;
        }

        switch (args[0]) {
            case "keyGen":
                if (args.Length < 2) {
                    Console.WriteLine("must include keysize");
                    return;
                }
                KeyGen(Int32.Parse(args[1]));
                break;
            case "sendKey":
                if (args.Length < 2) {
                    Console.WriteLine("must include email associated with key");
                    return;
                }

                if (SendKey(args[1]) == HttpStatusCode.NoContent) {
                    Console.WriteLine("Key saved");
                }
                break;
            case "getKey":
                if (args.Length < 2) {
                    Console.WriteLine("must include email associated with key");
                    return;
                }
                GetKey(args[1]);
                break;
            case "sendMsg":
                if (args.Length < 2) {
                    Console.WriteLine("must include email of recipient");
                    return;
                }
                if (args.Length < 3) {
                    Console.WriteLine("must include a message");
                    return;
                }

                if (SendMsg(args[1], args[2]) == HttpStatusCode.NoContent) {
                    Console.WriteLine("Message written");
                }
                break;
            case "getMsg":
                if (args.Length < 2) {
                    Console.WriteLine("must include email you are checking");
                    return;
                }
                GetMsg(args[1]);
                break;
            default:
                Console.WriteLine("Invalid option, must be: keyGen, sendKey, getKey, sendMsg, or getMsg");
                break;
        }
    }

    public static (BigInteger, BigInteger, BigInteger) KeyGen(int keysize) {
        var pBits = 480;
        BigInteger N, r, E = 29, D;
        do {
            var p = PrimeGen.NextPrime(pBits);
            var q = PrimeGen.NextPrime(keysize - pBits);
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
    
    public static HttpStatusCode SendKey(string email) {
        var privateKey = Key.Private.Read();
        privateKey.AddEmail(email);
        privateKey.Save();
        
        var publicKey = Key.Public.Read();
        publicKey.SetEmail(email);
        // Force the put to complete before returning
        var response = Client.PutAsync($"/Key/{email}", JsonContent.Create(publicKey)).Result;
        return response.StatusCode;
    }
    
    public static HttpStatusCode GetKey(string email) {
        var response = Client.GetAsync($"/Key/{email}").Result;
        if (response.StatusCode != HttpStatusCode.OK) {
            throw new ArgumentException($"key does not exist for {email}");
        }
        var key = Key.Public.fromJSON(response.Content.ReadAsStringAsync().Result);
        key.Save(email);
        return response.StatusCode;
    }
    
    public static HttpStatusCode SendMsg(string email, string msg) {
        var publicKey = Key.Public.Read(email);
        var encrypted = publicKey.Encrypt(msg);
        var message = new Message(email, encrypted);
        var response = Client.PutAsync($"/Message/{email}", JsonContent.Create(message)).Result;
        return response.StatusCode;
    }
    
    public static HttpStatusCode GetMsg(string email) {
        var privateKey = Key.Private.Read();
        if (!privateKey.email.Contains(email)) {
            throw new UnauthorizedAccessException("You are not authorized to read that email");
        }
        var response = Client.GetAsync($"/Message/{email}").Result;
        if (response.StatusCode != HttpStatusCode.OK) {
            throw new ArgumentException($"Email doesn't exist: {response.StatusCode}");
        }
        var msgString = response.Content.ReadAsStringAsync().Result;
        var message = JsonSerializer.Deserialize<Message>(msgString);
        Console.WriteLine(privateKey.Decrypt(message.content));
        return response.StatusCode;
    }
    
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

    private struct Message {
        public string email { get; set; }
        public string content { get; set; }

        public Message(string email, string content) {
            this.email = email;
            this.content = content;
        }
    }
}
