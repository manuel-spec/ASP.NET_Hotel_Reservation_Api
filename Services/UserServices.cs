using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using TodoApi.DatabaseSettings;
using TodoApi.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;
using System.Web;


namespace TodoApi.Services;

public class UserServices
{
    private readonly IMongoCollection<User> _userCollections;
    private readonly IMongoCollection<ChangePasswordRequest> _resetPasswordCollections;
    private readonly IConfiguration _configuration;

    public UserServices(IOptions<ReservationDBSettings> hotelDBSettings, IConfiguration configuration)
    {
        _configuration = configuration; // Add this line
        MongoClient client = new MongoClient(hotelDBSettings.Value.ConnectionURI);
        IMongoDatabase database = client.GetDatabase(hotelDBSettings.Value.DatabaseName);
        _userCollections = database.GetCollection<User>(hotelDBSettings.Value.UserCollectionName);
        _resetPasswordCollections = database.GetCollection<ChangePasswordRequest>(hotelDBSettings.Value.UserCollectionName);
    }


    public async Task<List<User>> GetAllUsers() => await _userCollections.Find(r => true).ToListAsync();
    public async Task<User> GetUSer(String id) => await _userCollections.Find<User>(r => r.Id == id).FirstOrDefaultAsync();
    public async Task<User> GetUSerByEmail(String email) => await _userCollections.Find<User>(r => r.Email == email).FirstOrDefaultAsync();

    // public async Task<User> Create(User user)
    // {
    //     await _userCollections.InsertOneAsync(user);

    //     return user;
    // }
    // public async Task<User> Create(User user)
    // {
    //     user.IsEmailVerified = false;
    //     user.EmailVerificationToken = GenerateToken(user.Email);

    //     await _userCollections.InsertOneAsync(user);

    //     // Send email with verification link
    //     await SendEmailVerificationEmail(user.Email, user.EmailVerificationToken);

    //     return user;
    // }

    public async Task<User> Create(User user)
    {
        user.IsEmailVerified = false;
        user.EmailVerificationToken = GenerateToken(user.Email);

        // Hash the password before storing it
        user.Password = HashPassword(user.Password);

        await _userCollections.InsertOneAsync(user);

        // Send email with verification link
        await SendEmailVerificationEmail(user.Email, user.EmailVerificationToken);

        return user;
    }

    // Hash the password using BCrypt
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
























    // public string Authenticate(string email, string password)
    // {
    //     var user = _userCollections.Find(x => x.Email == email && x.Password == password && x.IsEmailVerified).FirstOrDefault();

    //     if (user == null)
    //         return null;

    //     // Generate a token during authentication
    //     var token = GenerateToken(email.ToString());
    //     return token;
    // }


    public string Authenticate(string email, string password)
    {
        // Find the user by email
        var user = _userCollections.Find(x => x.Email == email && x.IsEmailVerified).FirstOrDefault();


        if (user == null || !VerifyPassword(password, user.Password))
        {
            // Invalid email, password, or unverified email
            return null;
        }

        // Generate a token during authentication
        var token = GenerateToken(email.ToString());
        return token;
    }

    // Verify the hashed password using BCrypt
    private bool VerifyPassword(string enteredPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(enteredPassword, hashedPassword);
    }


    // token generator based on email
    public string GenerateToken(string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var jwtKey = _configuration["JwtSettings:Key"];

        if (string.IsNullOrEmpty(jwtKey))
        {
            // Handle the case where the key is missing or empty
            throw new ApplicationException("JWT Key is missing or empty in configuration.");
        }

        var tokenKey = Encoding.ASCII.GetBytes(jwtKey);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new Claim[]{
                new(ClaimTypes.Email, email),
            }),
            Expires = DateTime.UtcNow.AddDays(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }





    // public bool VerifyToken(string token, string userEmail)
    // {
    //     try
    //     {
    //         var tokenHandler = new JwtSecurityTokenHandler();
    //         var jwtKey = _configuration["JwtSettings:Key"];

    //         if (string.IsNullOrEmpty(jwtKey))
    //         {
    //             // Handle the case where the key is missing or empty
    //             throw new ApplicationException("JWT Key is missing or empty in configuration.");
    //         }

    //         var tokenKey = Encoding.ASCII.GetBytes(jwtKey);

    //         var validationParameters = new TokenValidationParameters
    //         {
    //             ValidateIssuerSigningKey = true,
    //             IssuerSigningKey = new SymmetricSecurityKey(tokenKey),
    //             ValidateIssuer = false,
    //             ValidateAudience = false,
    //             ClockSkew = TimeSpan.Zero
    //         };

    //         // Validate token
    //         ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

    //         // Check if the token contains the email claim
    //         var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email);

    //         if (emailClaim == null || emailClaim.Value != userEmail)
    //         {
    //             return false; // Token doesn't match the provided email
    //         }

    //         return true;
    //     }
    //     catch (Exception)
    //     {
    //         return false;
    //     }
    // }






    // email verification logic
    public async Task SendEmailVerificationEmail(string userEmail, string verificationToken)
    {
        var emailSettings = _configuration.GetSection("EmailSettings").Get<EmailSettings>();

        var subject = "Email Verification";
        var body = $"Click the following link to verify your email: {GetVerificationLink(userEmail, verificationToken)}";

        using (var client = new SmtpClient(emailSettings.SmtpServer, emailSettings.SmtpPort))
        {
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(emailSettings.SmtpUsername, emailSettings.SmtpPassword);
            client.EnableSsl = true;

            var message = new MailMessage
            {
                From = new MailAddress(emailSettings.SenderEmail, emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(userEmail));

            await client.SendMailAsync(message);
        }
    }



    private string GetVerificationLink(string userEmail, string verificationToken)
    {

        var verificationEndpoint = "http://10.240.69.41:5000/user/verify-email";

        var link = $"<a href=\"{verificationEndpoint}?email={userEmail}&token={verificationToken}\">Verify Email</a>";

        return link;
    }


    public async Task<bool> VerifyEmailAsync(string email, string token)
    {
        var user = await _userCollections.Find(x => x.Email == email && x.EmailVerificationToken == token).FirstOrDefaultAsync();

        if (user != null)
        {
            // Mark email as verified
            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;

            // Save the updated user document
            await _userCollections.ReplaceOneAsync(x => x.Id == user.Id, user);

            return true;
        }

        return false;
    }






    public async Task<bool> ChangePassword(string email, string currentPassword, string newPassword)
    {
        var user = await _userCollections.Find(x => x.Email == email && x.IsEmailVerified).FirstOrDefaultAsync();

        if (user == null)
        {
            return false;
        }

        if (!VerifyPassword(currentPassword, user.Password))
        {
            return false;
        }

        user.Password = HashPassword(newPassword);

        await _userCollections.ReplaceOneAsync(x => x.Id == user.Id, user);

        return true;
    }
















    // public async Task<bool> ForgotPassword(string email)
    // {
    //     var user = await _userCollections.Find(x => x.Email == email).FirstOrDefaultAsync();
    //     if (user == null)
    //     {
    //         // User with the specified email not found
    //         return false;
    //     }

    //     // Generate a reset password token
    //     user.ResetPasswordToken = GenerateToken(email);
    //     user.ResetPasswordExpiration = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

    //     // Save the updated user document with reset password information
    //     await _userCollections.ReplaceOneAsync(x => x.Id == user.Id, user);

    //     // Send email with reset password link
    //     await SendResetPasswordEmail(user.Email, user.ResetPasswordToken);

    //     return true;
    // }



    // private async Task SendResetPasswordEmail(string userEmail, string resetPasswordToken)
    // {
    //     // Implement your email sending logic here
    //     // Include a link with the reset password token in the email
    //     // Example link: https://yourwebsite.com/reset-password?email=userEmail&token=resetPasswordToken
    // }
}