using System.Numerics;

namespace Messenger.Tests;

public class Tests {
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test1() {
        Assert.Pass();
    }

    [Test]
    public void TestKeyFile() {
        var file = new Program.PrivateKey(64, 128);
        file.Save();
        var retrieved = Program.PrivateKey.Read();
        Console.WriteLine(file.ToString());
        Assert.AreEqual(file.ToString(), retrieved.ToString());
    }

    [Test]
    public void TestKeyGen() {
        Program.KeyGen();
        Assert.AreEqual(BigInteger.One, Program.D * Program.E % Program.r);
    }
}