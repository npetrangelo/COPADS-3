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
        file.Save("test.txt");
        var retrieved = Program.PrivateKey.Read("test.txt");
        Console.WriteLine(file.ToString());
        Assert.AreEqual(file.ToString(), retrieved.ToString());
    }
}