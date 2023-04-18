using System.Numerics;

namespace Messenger.Tests;

public class Tests {
    [OneTimeSetUp]
    public void Setup() {
        Program.Client.BaseAddress = new Uri("http://kayrun.cs.rit.edu:5000");
    }

    [Test, Order(0)]
    public void TestPrivateKey() {
        var file = new Key.Private(PrimeGen.NextPrime(32), PrimeGen.NextPrime(32));
        file.Save();
        var retrieved = Key.Private.Read();
        Console.WriteLine(Convert.ToHexString(file.toBytes()));
        Console.WriteLine(file.ToString());
        Console.WriteLine(retrieved.ToString());
        Assert.AreEqual(file, retrieved);
    }
    
    [Test, Order(0)]
    public void TestPublicKey() {
        var file = new Key.Public(PrimeGen.NextPrime(32), PrimeGen.NextPrime(32));
        file.Save();
        var retrieved = Key.Public.Read();
        Console.WriteLine(Convert.ToHexString(file.toBytes()));
        Console.WriteLine(file.ToString());
        Console.WriteLine(retrieved.ToString());
        Assert.AreEqual(file, retrieved);
    }

    [Test, Order(1)]
    public void TestKeyGen() {
        var (r, E, D) = Program.KeyGen();
        Assert.Less(E, r);
        Assert.False(E % r == 0);
        Assert.AreEqual(BigInteger.One, E * D % r);
    }

    [Test, Order(2)]
    public void TestEncrypt() {
        var privateKey = Key.Private.Read();
        var publicKey = Key.Public.Read();
        Assert.AreEqual(privateKey.N(), publicKey.N());
        var message = PrimeGen.NextPrime(32);
        Assert.AreEqual(message, privateKey.Encrypt(publicKey.Encrypt(message)));
    }

    [Test, Order(3)]
    public void TestEncryptText() {
        var privateKey = Key.Private.Read();
        var publicKey = Key.Public.Read();
        var message = "Hello";
        var encrypted = publicKey.Encrypt(message);
        Console.WriteLine(message);
        Console.WriteLine(encrypted);
        Assert.AreEqual(message, privateKey.Decrypt(encrypted));
    }
    
    [Test, Order(2)]
    public void TestGetKey() => Program.GetKey("toivcs@rit.edu");


    [Test, Order(2)]
    public void TestSendKey() {
        var email = "nap7292@rit.edu";
        var code = Program.SendKey(email);
        Console.WriteLine(code);
        Program.GetKey(email);
    }

    [Test, Order(3)]
    public void TestSendMessage() {
        var email = "nap7292@rit.edu";
        var code = Program.SendMsg(email, "Hello");
        Console.WriteLine(code);
    }
    
    [Test, Order(4)]
    public void TestGetMessage() {
        var email = "nap7292@rit.edu";
        var code = Program.GetMsg(email);
        Console.WriteLine(code);
    }
}