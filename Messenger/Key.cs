using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Messenger; 

public abstract class Key {
    public string key { get; set; }
    
    private BigInteger _X, _N;

    public Key(string key) {
        this.key = key;
        var bytes = Convert.FromBase64String(key);
        var x = BitConverter.ToInt32(bytes, 0);
        _X = new BigInteger(bytes.Take(new Range(4, x + 4)).ToArray());
        var nBytes = BitConverter.ToInt32(bytes, 4 + x);
        _N = new BigInteger(bytes.Take(new Range(4 + x + 4, 4 + x + 4 + nBytes)).ToArray());
    }

    public Key(BigInteger X, BigInteger N) {
        _X = X;
        _N = N;
        var XBytes = X.ToByteArray();
        var NBytes = N.ToByteArray();
        var key = new List<byte>();
        key.AddRange(BitConverter.GetBytes(XBytes.Length));
        key.AddRange(XBytes);
        key.AddRange(BitConverter.GetBytes(NBytes.Length));
        key.AddRange(NBytes);
        this.key = Convert.ToBase64String(key.ToArray());
    }

    public BigInteger N() => _N;

    public byte[] toBytes() => Convert.FromBase64String(key);

    public BigInteger Encrypt(BigInteger msg) {
        return BigInteger.ModPow(msg, _X, _N);
    }

    public abstract string toJSON();

    protected void Save(string filename) => File.WriteAllText(filename, toJSON());

    protected static TValue? Read<TValue>(string filename) {
        return JsonSerializer.Deserialize<TValue>(File.ReadAllText(filename));
    }

    public override bool Equals(object? obj) {
        if (obj == null) return false;
        if (GetType() != obj.GetType()) return false;
        obj = (Key) obj;
        return Equals((Key) obj);
    }
        
    public bool Equals(Key that) {
        return toJSON() == that.toJSON() && key == that.key && _X == that._X && _N == that._N;
    }

    public override string ToString() => $"_X=0x{Convert.ToHexString(_X.ToByteArray())} _N=0x{Convert.ToHexString(_N.ToByteArray())} {toJSON()}";
    
    public class Public : Key {
        public string email { get; set; }
        
        public Public(BigInteger X, BigInteger N) : base(X, N) {}

        [JsonConstructor]
        public Public(string email, string key) : base(key) {
            this.email = email;
        }
        
        public string Encrypt(string message) {
            var msg = new BigInteger(Encoding.UTF8.GetBytes(message));
            var encrypted = Encrypt(msg);
            return Convert.ToBase64String(encrypted.ToByteArray());
        }
        
        public override string toJSON() => JsonSerializer.Serialize(this);

        public void Save() => base.Save("public.key");
        
        public static Public? Read() => Key.Read<Public>("public.key");
    }
    
    public class Private : Key {
        public List<string> email { get; set; } = new ();
        
        public Private(BigInteger X, BigInteger N) : base(X, N) {}

        [JsonConstructor]
        public Private(List<string> email, string key) : base(key) {
            this.email = email;
        }
        
        public void AddEmail(string email) {
            this.email.Add(email);
        }
        
        public string Decrypt(string message) {
            var msg = new BigInteger(Convert.FromBase64String(message));
            var encrypted = Encrypt(msg);
            return Encoding.UTF8.GetString(encrypted.ToByteArray());
        }
        
        public override string toJSON() => JsonSerializer.Serialize(this);

        public void Save() => base.Save("private.key");
        
        public static Private? Read() => Key.Read<Private>("private.key");
    }
}