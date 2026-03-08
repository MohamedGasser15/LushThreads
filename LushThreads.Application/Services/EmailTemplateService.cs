using LushThreads.Application.ServiceInterfaces;
using LushThreads.Domain.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LushThreads.Application.Services
{
    public class EmailTemplateService : IEmailTemplateService
    {
        public string GetOrderDeliveredEmail(ApplicationUser user, int orderNumber, string orderLink)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #eaeaea; padding-bottom: 15px; }}
        .header h1 {{ color: #088178; margin: 0; font-size: 24px; }}
        .content {{ margin-bottom: 25px; line-height: 1.6; }}
        .content p {{ font-size: 16px; color: #333333; margin-bottom: 15px; }}
        .order-number {{ font-size: 28px; font-weight: bold; color: #088178; letter-spacing: 3px; text-align: center; margin: 25px 0; padding: 15px; background-color: #f8f9fa; border-radius: 6px; border: 1px dashed #088178; }}
        .security-alert {{ background-color: #f8f9fa; border-left: 4px solid #088178; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 14px; color: #777; margin-top: 25px; border-top: 1px solid #eaeaea; padding-top: 15px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #088178; color: white; text-decoration: none; border-radius: 4px; margin: 20px auto; text-align: center; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'><h1>Order Delivered</h1></div>
        <div class='content'>
            <p>Hello {user.Name},</p>
            <p>Your order has been successfully delivered. We hope you enjoy your purchase!</p>
            <div class='order-number'>Order Number: {orderNumber}</div>
            <p>If you have any questions, feel free to reach out to us.</p>
            <div class='security-alert'><p><strong>Security Tip:</strong> Ensure communications are directly from LushThreads.</p></div>
            <p>Track your order status:</p>
            <a href='{orderLink}' class='button'>Track Your Order</a>
            <p>If the button doesn't work, copy and paste this link:</p>
            <p style='word-break: break-all;'>{orderLink}</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to {user.Email} as part of our order delivery process.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string GetOrderShippedEmail(ApplicationUser user, int orderNumber, string orderLink)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #eaeaea; padding-bottom: 15px; }}
        .header h1 {{ color: #088178; margin: 0; font-size: 24px; }}
        .content {{ margin-bottom: 25px; line-height: 1.6; }}
        .content p {{ font-size: 16px; color: #333333; margin-bottom: 15px; }}
        .order-number {{ font-size: 28px; font-weight: bold; color: #088178; letter-spacing: 3px; text-align: center; margin: 25px 0; padding: 15px; background-color: #f8f9fa; border-radius: 6px; border: 1px dashed #088178; }}
        .security-alert {{ background-color: #f8f9fa; border-left: 4px solid #088178; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 14px; color: #777; margin-top: 25px; border-top: 1px solid #eaeaea; padding-top: 15px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #088178; color: white; text-decoration: none; border-radius: 4px; margin: 20px auto; text-align: center; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'><h1>Your Order Has Shipped!</h1></div>
        <div class='content'>
            <p>Hello {user.Name},</p>
            <p>Your order has been shipped and is on its way!</p>
            <div class='order-number'>Order Number: {orderNumber}</div>
            <p>What's next:</p>
            <ul>
                <li>Your package is with our shipping partner</li>
                <li>You'll receive tracking updates</li>
                <li>Check estimated delivery in tracking info</li>
            </ul>
            <div class='security-alert'><p><strong>Security Tip:</strong> Ensure communications are directly from LushThreads.</p></div>
            <p>Track your shipment:</p>
            <a href='{orderLink}' class='button'>Track Your Package</a>
            <p>If the button doesn't work, copy and paste this link:</p>
            <p style='word-break: break-all;'>{orderLink}</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to {user.Email} as part of our order shipping process.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string GetOrderInProcessEmail(ApplicationUser user, int orderNumber, string orderLink)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #eaeaea; padding-bottom: 15px; }}
        .header h1 {{ color: #088178; margin: 0; font-size: 24px; }}
        .content {{ margin-bottom: 25px; line-height: 1.6; }}
        .content p {{ font-size: 16px; color: #333333; margin-bottom: 15px; }}
        .order-number {{ font-size: 28px; font-weight: bold; color: #088178; letter-spacing: 3px; text-align: center; margin: 25px 0; padding: 15px; background-color: #e6f4f1; border-radius: 6px; border: 1px dashed #088178; }}
        .info-box {{ background-color: #e6f4f1; border-left: 4px solid #088178; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 14px; color: #777; margin-top: 25px; border-top: 1px solid #eaeaea; padding-top: 15px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #088178; color: white; text-decoration: none; border-radius: 4px; margin: 20px auto; text-align: center; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'><h1>Your Order Is Being Processed</h1></div>
        <div class='content'>
            <p>Hello {user.Name},</p>
            <p>Your order is being processed. Thank you for your purchase!</p>
            <div class='order-number'>Order #{orderNumber}</div>
            <p>What's next:</p>
            <ul>
                <li>Your items are being prepared for shipment</li>
                <li>You'll receive a shipping confirmation soon</li>
                <li>Track your order status from your account</li>
            </ul>
            <div class='info-box'><p><strong>Need help?</strong> Reply to this email or contact our support team.</p></div>
            <p>Track your order status:</p>
            <a href='{orderLink}' class='button'>View Order Status</a>
            <p>If the button doesn't work, copy and paste this link:</p>
            <p style='word-break: break-all;'>{orderLink}</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to {user.Email} regarding your recent order.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string GetOrderCancelledEmail(ApplicationUser user, int orderNumber, string orderLink)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #eaeaea; padding-bottom: 15px; }}
        .header h1 {{ color: #088178; margin: 0; font-size: 24px; }}
        .content {{ margin-bottom: 25px; line-height: 1.6; }}
        .content p {{ font-size: 16px; color: #333333; margin-bottom: 15px; }}
        .order-number {{ font-size: 28px; font-weight: bold; color: #088178; letter-spacing: 3px; text-align: center; margin: 25px 0; padding: 15px; background-color: #e6f4f1; border-radius: 6px; border: 1px dashed #088178; }}
        .info-box {{ background-color: #e6f4f1; border-left: 4px solid #088178; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 14px; color: #777; margin-top: 25px; border-top: 1px solid #eaeaea; padding-top: 15px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #088178; color: white; text-decoration: none; border-radius: 4px; margin: 20px auto; text-align: center; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'><h1>Your Order Has Been Cancelled</h1></div>
        <div class='content'>
            <p>Hello {user.Name},</p>
            <p>Your order has been cancelled. Details below:</p>
            <div class='order-number'>Order #{orderNumber}</div>
            <p>Possible reasons for cancellation:</p>
            <ul>
                <li>Items no longer available</li>
                <li>Payment not completed</li>
                <li>Cancelled at your request</li>
            </ul>
            <div class='info-box'><p><strong>Need help?</strong> Contact our support team.</p></div>
            <p>View your order history:</p>
            <a href='{orderLink}' class='button'>View My Orders</a>
            <p>If the button doesn't work, copy and paste this link:</p>
            <p style='word-break: break-all;'>{orderLink}</p>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to {user.Email} regarding your cancelled order.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string GenerateEmailConfirmationEmail(ApplicationUser user, string code)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #eaeaea; padding-bottom: 15px; }}
        .header h1 {{ color: #088178; margin: 0; font-size: 24px; }}
        .content {{ margin-bottom: 25px; line-height: 1.6; }}
        .content p {{ font-size: 16px; color: #333333; margin-bottom: 15px; }}
        .verification-code {{ font-size: 28px; font-weight: bold; color: #088178; letter-spacing: 3px; text-align: center; margin: 25px 0; padding: 15px; background-color: #f8f9fa; border-radius: 6px; border: 1px dashed #088178; }}
        .security-alert {{ background-color: #f8f9fa; border-left: 4px solid #088178; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 14px; color: #777; margin-top: 25px; border-top: 1px solid #eaeaea; padding-top: 15px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #088178; color: white; text-decoration: none; border-radius: 4px; margin: 20px auto; text-align: center; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'><h1>Email Verification Code</h1></div>
        <div class='content'>
            <p>Hello {user.Name},</p>
            <p>Use this code to verify your email address:</p>
            <div class='verification-code'>{code}</div>
            <p>This code will expire in 15 minutes.</p>
            <div class='security-alert'><p><strong>Security Tip:</strong> Never share this code with anyone.</p></div>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to <strong>{user.Email}</strong> as part of our verification process.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string Generate2FASetupEmail(ApplicationUser user, string code)
        {
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #eaeaea; padding-bottom: 15px; }}
        .header h1 {{ color: #088178; margin: 0; font-size: 24px; }}
        .content {{ margin-bottom: 25px; line-height: 1.6; }}
        .content p {{ font-size: 16px; color: #333333; margin-bottom: 15px; }}
        .verification-code {{ font-size: 28px; font-weight: bold; color: #088178; letter-spacing: 3px; text-align: center; margin: 25px 0; padding: 15px; background-color: #f8f9fa; border-radius: 6px; border: 1px dashed #088178; }}
        .security-alert {{ background-color: #f8f9fa; border-left: 4px solid #088178; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 14px; color: #777; margin-top: 25px; border-top: 1px solid #eaeaea; padding-top: 15px; }}
        .button {{ display: inline-block; padding: 12px 24px; background-color: #088178; color: white; text-decoration: none; border-radius: 4px; margin: 20px auto; text-align: center; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'><h1>Two-Factor Authentication Setup</h1></div>
        <div class='content'>
            <p>Hello {user.Name},</p>
            <p>Use this code to enable two-factor authentication:</p>
            <div class='verification-code'>{code}</div>
            <p>This code will expire in 5 minutes.</p>
            <div class='security-alert'><p><strong>Security Tip:</strong> Never share this code with anyone.</p></div>
        </div>
        <div class='footer'>
            <p>© {DateTime.Now.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to <strong>{user.Email}</strong> as part of our 2FA setup process.</p>
        </div>
    </div>
</body>
</html>";
        }

        public string GeneratePasswordChangeEmail(ApplicationUser user, string ipAddress, string deviceName, DateTime changeTime, string passwordResetLink)
        {
            // Reuse the template from original controller
            return $@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: 0 auto; background-color: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1); }}
        .header {{ text-align: center; margin-bottom: 25px; border-bottom: 1px solid #eaeaea; padding-bottom: 15px; }}
        .header h1 {{ color: #088178; margin: 0; font-size: 24px; }}
        .content {{ margin-bottom: 25px; line-height: 1.6; }}
        .content p {{ font-size: 16px; color: #333333; margin-bottom: 15px; }}
        .security-alert {{ background-color: #f8f9fa; border-left: 4px solid #088178; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .footer {{ text-align: center; font-size: 14px; color: #777; margin-top: 25px; border-top: 1px solid #eaeaea; padding-top: 15px; }}
        .button {{ display: inline-block; padding: 10px 20px; background-color: #088178; color: white; text-decoration: none; border-radius: 4px; margin: 15px 0; }}
        .info-item {{ margin-bottom: 8px; }}
        .info-label {{ font-weight: bold; color: #555; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'><h1>Password Change Confirmation</h1></div>
        <div class='content'>
            <p>Hello {user.Name},</p>
            <p>Your LushThreads account password was successfully changed on {changeTime:MMMM dd, yyyy} at {changeTime:h:mm tt}.</p>
            <div class='security-alert'><p><strong>Security Notice:</strong> If you didn't make this change, please take immediate action to secure your account.</p></div>
            <div class='info-item'><span class='info-label'>Device:</span> {deviceName}</div>
            <div class='info-item'><span class='info-label'>IP Address:</span> {ipAddress}</div>
            <div class='info-item'><span class='info-label'>Time:</span> {changeTime:f}</div>
            <p>For your security, we recommend that you:</p>
            <ul>
                <li>Use a strong, unique password</li>
                <li>Enable two-factor authentication</li>
                <li>Review your recent account activity</li>
            </ul>
            <a href='{passwordResetLink}' class='button'>Secure My Account</a>
        </div>
        <div class='footer'>
            <p>© {changeTime.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to <strong>{user.Email}</strong> as part of our security notifications.</p>
        </div>
    </div>
</body>
</html>";
        }
        public string GenerateForgotPasswordEmail(
ApplicationUser user,
string ipAddress,
string deviceName,
DateTime requestTime,
string code,
string passwordResetLink)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 25px;
            border-bottom: 1px solid #eaeaea;
            padding-bottom: 15px;
        }}
        .header h1 {{
            color: #088178;
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            margin-bottom: 25px;
            line-height: 1.6;
        }}
        .content p {{
            font-size: 16px;
            color: #333333;
            margin-bottom: 15px;
        }}
        .verification-code {{
            font-size: 24px;
            font-weight: bold;
            color: #088178;
            letter-spacing: 3px;
            text-align: center;
            margin: 20px 0;
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 6px;
            border: 1px dashed #088178;
        }}
        .security-alert {{
            background-color: #f8f9fa;
            border-left: 4px solid #088178;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .footer {{
            text-align: center;
            font-size: 14px;
            color: #777;
            margin-top: 25px;
            border-top: 1px solid #eaeaea;
            padding-top: 15px;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            background-color: #088178;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            margin: 15px 0;
        }}
        .info-item {{
            margin-bottom: 8px;
        }}
        .info-label {{
            font-weight: bold;
            color: #555;
        }}
        .security-message {{
            color: #088178;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <h1>Password Reset Verification</h1>
        </div>
        
        <div class='content'>
            <p>Hello {user.Name},</p>
            
            <p class='security-message'>A password reset was requested for your account!</p>
            
            <div class='security-alert'>
                <p><strong>SECURITY NOTICE:</strong> Use this verification code to confirm your identity:</p>
                <div class='verification-code'>
                    {code}
                </div>
                <p>This code expires in 15 minutes.</p>
            </div>
            
            <div class='security-alert'>
                <p><strong>IMPORTANT:</strong> Do not share this code with anyone. LushThreads will never ask for your verification code.</p>
            </div>
            
            <p>Request details:</p>
            <div class='info-item'>
                <span class='info-label'>Device:</span> {deviceName}
            </div>
            <div class='info-item'>
                <span class='info-label'>IP Address:</span> {ipAddress}
            </div>
            <div class='info-item'>
                <span class='info-label'>Time:</span> {requestTime:f}
            </div>
            
            <p>To complete your password reset, click the button below:</p>
            
            <a href=""{passwordResetLink}"" class='button'>Reset Password</a>
            
            <p style=""word-break: break-all;"">Or copy this link manually: {passwordResetLink}</p>
            
            <p>If you didn't request this password reset, please secure your account immediately.</p>
        </div>
        
        <div class='footer'>
            <p>© {requestTime.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to {user.Email} as part of our security notifications.</p>
        </div>
    </div>
</body>
</html>";
        }
        public string Generate2FACodeEmail(ApplicationUser user, string code, string verificationLink)
        {
            return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 20px;
        }}
        .email-container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 25px;
            border-bottom: 1px solid #eaeaea;
            padding-bottom: 15px;
        }}
        .header h1 {{
            color: #088178;
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            margin-bottom: 25px;
            line-height: 1.6;
        }}
        .content p {{
            font-size: 16px;
            color: #333333;
            margin-bottom: 15px;
        }}
        .verification-code {{
            font-size: 28px;
            font-weight: bold;
            color: #088178;
            letter-spacing: 3px;
            text-align: center;
            margin: 25px 0;
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 6px;
            border: 1px dashed #088178;
        }}
        .security-alert {{
            background-color: #f8f9fa;
            border-left: 4px solid #088178;
            padding: 15px;
            margin: 20px 0;
            border-radius: 4px;
        }}
        .footer {{
            text-align: center;
            font-size: 14px;
            color: #777;
            margin-top: 25px;
            border-top: 1px solid #eaeaea;
            padding-top: 15px;
        }}
        .button {{
            display: inline-block;
            padding: 12px 24px;
            background-color: #088178;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            margin: 20px auto;
            text-align: center;
        }}
        .info-item {{
            margin-bottom: 8px;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            <h1>Two-Factor Authentication Code</h1>
        </div>
        
        <div class='content'>
            <p>Hello {user.Name},</p>
            
            <p>Your login attempt requires verification. Use this code to complete your sign-in:</p>
            
            <div class='verification-code'>
                {code}
            </div>
            
            <p>This code will expire in 15 minutes. If you didn't request this, please ignore this email.</p>
            
            <div class='security-alert'>
                <p><strong>Security Tip:</strong> Never share this code with anyone. Trendsvalley will never ask for your verification code.</p>
            </div>
            
            <p>Alternatively, you can click the button below to verify your email:</p>
            
            <a href=""{verificationLink}"" class='button'>Verify Email Address</a>
            
            <p>If the button doesn't work, copy and paste this link into your browser:</p>
            <p style=""word-break: break-all;"">{verificationLink}</p>
        </div>
        
        <div class='footer'>
            <p>© {DateTime.Now.Year} LushThreads. All rights reserved.</p>
            <p>This email was sent to {user.Email} as part of our verification process.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
