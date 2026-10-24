using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Middleware.Services.Encryption
{
    public class PasswordHandler
    {
        public (string Hash, string Salt) HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            // This generates 16 cryptographically random bytes.
            //A salt is a random value added to the password - hashing process.
            //It helps ensure that two users with the same password don't automatically get the same stored hash.
            // The salt isn't a secret like the password. It must be stored so that the application can verify the
            // password during login.

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                600_000,
                HashAlgorithmName.SHA256,
                32
            );

            // You're using PBKDF2, a password-based key derivation function. It transforms the password
            // and salt into a derived byte sequence
            // password The plain - text password received by HashPassword()
            //salt The random 16 - byte salt generated in the previous line
            //600_000 Number of PBKDF2 iterations
            //HashAlgorithmName.SHA256 The underlying hash algorithm
            //32  Desired output length in bytes

            return (
                Convert.ToBase64String(hash),
                Convert.ToBase64String(salt)
            );
            //Both the hash and salt are byte arrays, but your database entity expects strings.
            //Convert.ToBase64String() converts each byte array into a Base64 - encoded string,
            //which is convenient for storing the values in text columns.
        }

        //Important: This isn't encryption. Encryption is designed to be reversible with a key.
        //Password hashing is designed so you can verify a password without needing to recover the original password from the stored hash.

        public bool VerifyPassword(
            string password,
            string storedHash,
            string storedSalt)
        {
            byte[] salt = Convert.FromBase64String(storedSalt);
            byte[] expectedHash = Convert.FromBase64String(storedHash);

            byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                600_000,
                HashAlgorithmName.SHA256,
                32
            );

            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash
            );
        }
    }
}

// HASHING

// A password should not be stored in your database in a form that lets your application or a database attacker
// simply read the original password. Instead, your application stores a password verifier: a result produced by a
// deliberately expensive password-hashing process, along with the information needed to verify a future login.

// Hashing is a mathematical process that takes input of almost any length and produces a fixed-length output,
// called a hash or digest, commonly 256 bits or 32 bytes depending on the algorithm used, mostly displayed as hexadecimal

// A good cryptographic hash function has several important properties.
//-Deterministic: the same input always produces the same output.
//- Fixed-length output: the digest has a defined length for the algorithm.
//- Preimage resistance: given a digest, it should be computationally infeasible to recover an input that produces it.
//- Second-preimage resistance: given one input, it should be infeasible to find a different input with the same digest.
//- Collision resistance: it should be infeasible to find any two different inputs with the same digest.
//- Avalanche effect: a tiny input change should drastically change the digest. (digest means output produced)

// A hash is not encryption.
//Property Hashing	Encryption
//Main purpose	Produce a digest or verifier	Protect data confidentiality
//Reversible with a key?	No decryption operation	Yes, with the correct key
//Same input and configuration	Same digest	Depends on the encryption mode and nonce
//Typical uses	Integrity, password verification	Protecting sensitive data at rest or in transit

// it's still not appropriate password storage.
// Why? SHA-256 is designed to be fast. Modern hardware can calculate enormous numbers of SHA-256 hashes,
// allowing attackers to test likely passwords very quickly.

// Password storage uses algorithms specifically designed to make each password guess expensive.

//Common choices include:
//-Argon2id — a modern, memory-hard password-hashing algorithm.
//- PBKDF2 — a password-based key derivation function with a configurable iteration count; widely supported and
//useful where standards or platform requirements call for it.
//- scrypt — another memory-hard password-hashing function.
//- bcrypt — a well-established password-hashing algorithm with a configurable work factor.

// MAIN CRUX
// is my salt stored alongside the userid in database, so while logging in we retrive that salt corresponding
// to userid then hash the password + salt to check whether it matches with hashed password
// being stored in db or not


