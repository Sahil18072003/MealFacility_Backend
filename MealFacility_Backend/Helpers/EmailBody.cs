namespace MealFacility_Backend.Helpers
{
    public class EmailBody
    {
        public static string EmailStringBody(string email, string otp)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>OTP Email</title>
</head>
<body style=""font-family: Arial, sans-serif; background-color: #f4f4f4; padding: 20px;"">

    <div style=""background-color: #fff; padding: 20px; border-radius: 5px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
        <h2 style=""color: #333;"">Your OTP for Password Reset</h2>
        <p style=""color: #666;"">Your OTP for resetting the password is:</p>
        <div style=""font-size: 24px; font-weight: bold; color: #093cb3; margin-bottom: 20px;"">{otp}</div>
        <p style=""color: #666;"">Please use this OTP to reset your password. This OTP is valid for 5 minutes.</p>
        <p style=""color: #666;"">If you didn't request a password reset, please ignore this email.</p>
    </div>

    <div style=""margin-top: 20px; color: #999; font-size: 12px;"">
        <p>This email was sent to {email}.</p>
        <p>Please do not reply to this email.</p>
    </div>

</body>
</html>
>";
        }
    }
}
