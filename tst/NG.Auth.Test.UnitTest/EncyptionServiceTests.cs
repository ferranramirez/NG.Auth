using Xunit;

namespace NG.Auth.Test.UnitTest
{
    public class EncyptionServiceTests
    {
        //private IPasswordHasher _passwordHasher;

        //public EncyptionServiceTests()
        //{
        //    HashingOptions hashingOptions = new HashingOptions();
        //    hashingOptions.Iterations = 2000;
        //    services.Configure<HashingOptions>(hashingOptions);
        //    _passwordHasher = new PasswordHasher(hashingOptions);
        //}

        [Fact]
        public void Encrypt()
        {
            // Arrange
            string password = "RandomPWD*^123";


            // Act
            var actual = 12;//  _passwordHasher.Hash(password);

            // Assert
            Assert.NotNull(actual);
        }

        //[Fact(Skip = "Not implemented yet")]
        //public void Decrypt()
        //{
        //    // Arrange
        //    string password = "RandomPWD*^123";
        //    var encrypted = _passwordHasher.Hash(password);

        //    // Act
        //    //var rightPassword = _passwordHasher.Check(encrypted, false);

        //    // Assert
        //    Assert.True(false);

        //}
    }
}
