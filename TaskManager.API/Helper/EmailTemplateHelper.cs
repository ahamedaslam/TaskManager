namespace TaskManager.MultiTenant.Helper
{
    public static class EmailTemplateHelper
    {
        public static string OtpTemplate(
            string userName,
            string otp,
            int expiryMinutes,
            string appName)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>OTP Verification</title>
</head>
<body style='margin:0; padding:0; background-color:#f4f6f8; font-family:Arial, sans-serif;'>

<table width='100%' cellpadding='0' cellspacing='0' style='padding:30px 0;'>
    <tr>
        <td align='center'>
            <table width='520' cellpadding='0' cellspacing='0'
                   style='background:#ffffff; border-radius:10px; padding:30px;
                          box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

                <!-- Header -->
                <tr>
                    <td style='text-align:center; padding-bottom:20px;'>
                        <h2 style='margin:0; color:#2d6cdf;'>{appName}</h2>
                        <p style='margin:5px 0 0; color:#777; font-size:14px;'>
                            Secure Account Verification
                        </p>
                    </td>
                </tr>

                <!-- Body -->
                <tr>
                    <td style='color:#333; font-size:15px;'>
                        <p>Hi <strong>{userName}</strong>,</p>

                        <p>
                            We received a request to verify your identity.
                            Please use the One-Time Password (OTP) below:
                        </p>

                        <div style='
                            margin:25px 0;
                            padding:15px;
                            text-align:center;
                            font-size:32px;
                            letter-spacing:6px;
                            font-weight:bold;
                            color:#2d6cdf;
                            background:#f1f5ff;
                            border-radius:8px;'>
                            {otp}
                        </div>

                        <p>
                            ⏳ This OTP will expire in
                            <strong>{expiryMinutes} minutes</strong>.
                        </p>

                        <p style='color:#777; font-size:13px; margin-top:20px;'>
                            If you did not request this code, please ignore this email.
                            For your security, do not share this OTP with anyone.
                        </p>
                    </td>
                </tr>

                <!-- Footer -->
                <tr>
                    <td style='border-top:1px solid #eee; padding-top:20px;
                               text-align:center; font-size:12px; color:#999;'>
                        <p style='margin:0;'>
                            © {DateTime.UtcNow.Year} {appName}. All rights reserved.
                        </p>
                        <p style='margin:5px 0 0;'>
                            {appName} – Security Operations
                        </p>
                    </td>
                </tr>

            </table>
        </td>
    </tr>
</table>

</body>
</html>";
        }
    }
}
