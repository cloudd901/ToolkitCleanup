using Xunit;

namespace ToolkitCleanup.Tests
{
    public class CryptoToolsTests
    {
        private const string Secret = "test-secret";

        [Fact]
        public void EncryptThenDecrypt_ReturnsOriginalString()
        {
            string original = "Hello, World!";
            string encrypted = Crypto.EncryptStringAES(original, Secret);
            string decrypted = Crypto.DecryptStringAES(encrypted, Secret);

            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void EncryptStringAES_NullInput_ReturnsNull()
        {
            string result = Crypto.EncryptStringAES(null, Secret);
            Assert.Null(result);
        }

        [Fact]
        public void EncryptStringAES_EmptyInput_ReturnsNull()
        {
            string result = Crypto.EncryptStringAES(string.Empty, Secret);
            Assert.Null(result);
        }

        [Fact]
        public void DecryptStringAES_NullInput_ReturnsNull()
        {
            string result = Crypto.DecryptStringAES(null, Secret);
            Assert.Null(result);
        }

        [Fact]
        public void DecryptStringAES_EmptyInput_ReturnsNull()
        {
            string result = Crypto.DecryptStringAES(string.Empty, Secret);
            Assert.Null(result);
        }

        [Fact]
        public void EncryptDecrypt_NullSecret_ReturnsNull()
        {
            string resultEncrypt = Crypto.EncryptStringAES("data", null);
            string resultDecrypt = Crypto.DecryptStringAES("cipher", null);

            Assert.Null(resultEncrypt);
            Assert.Null(resultDecrypt);
        }
    }
}
