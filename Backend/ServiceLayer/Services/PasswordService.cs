using System;
using System.Net.Http;
using System.Security.Cryptography;

namespace ServiceLayer.Services
{
    public class PasswordService : IPasswordService
    {
        public byte[] GenerateSalt() {
            byte[] salt = new byte[128];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public string HashPassword(string password, byte[] salt)
        {
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(password, salt);
            rfc.IterationCount = 10000;
            byte[] hash = rfc.GetBytes(256);
            return Convert.ToBase64String(hash);
        }

        public string HashPasswordSHA1(string password, byte[] salt)
        {
            SHA1 sh = SHA1.Create();
            byte[] byte_arr = System.Text.Encoding.ASCII.GetBytes(password);
            byte[] hashed_bytes = sh.ComputeHash(byte_arr);
            return BitConverter.ToString(hashed_bytes).Replace("-", "");
        }

       public int CheckPasswordPwned(string password) {
            string PwnedPasswordAPIURL = "https://api.pwnedpasswords.com/range/";
            //Take user password and hash using SHA-1
            string hashed_Password = HashPasswordSHA1(password, null);
            //Refromat the hashed password into a prefix and suffix
            string prefix = hashed_Password.Substring(0, 5);
            string suffix = hashed_Password.Substring(5);

            //Iterate through each line of the Api Response and compare with our hashed password suffix
            foreach (string key in QueryPwnedApi(prefix, PwnedPasswordAPIURL))
            {
                string[] pwned_count = key.Split(':');
                //If the strings match, return the # of times password was compromised
                if (suffix == pwned_count[0])
                {
                    return Int32.Parse(pwned_count[1]);
                }

            }
            return 0;
        }

        public bool CheckPasswordLength(string password)
        {
            return password.Length >= 12 && password.Length <= 2000;
        }

        public string[] QueryPwnedApi(string prefix, string url)
        {
            HttpClient client = new HttpClient();
            return (client.GetStringAsync(url + prefix).Result).Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        }
    }
}