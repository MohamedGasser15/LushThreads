import 'package:LushThreads/core/constants/app_routes.dart';
import 'package:LushThreads/core/theme/app_text_styles.dart';
import 'package:LushThreads/shared/providers/app_state.dart';
import 'package:flutter/material.dart';
import '../../../../core/theme/app_colors.dart';

// ─── Login Screen ─────────────────────────────────────────────────────────────
class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});

  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _emailCtrl    = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _obscure       = true;

  @override
  void dispose() { _emailCtrl.dispose(); _passwordCtrl.dispose(); super.dispose(); }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(28),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 20),
              // Logo
              Text('LushThreads', style: AppTextStyles.logoStyle.copyWith(fontSize: 28)),
              const SizedBox(height: 8),
              Text('Welcome back!', style: AppTextStyles.headlineMedium),
              const SizedBox(height: 6),
              Text('Sign in to continue shopping',
                  style: AppTextStyles.bodyMedium),
              const SizedBox(height: 40),

              // Email
              _label('Email Address'),
              const SizedBox(height: 8),
              _textField(
                  controller: _emailCtrl,
                  hint: 'you@example.com',
                  icon: Icons.email_outlined,
                  keyboardType: TextInputType.emailAddress),
              const SizedBox(height: 16),

              // Password
              _label('Password'),
              const SizedBox(height: 8),
              _textField(
                  controller: _passwordCtrl,
                  hint: '••••••••',
                  icon: Icons.lock_outline,
                  obscure: _obscure,
                  suffix: GestureDetector(
                    onTap: () => setState(() => _obscure = !_obscure),
                    child: Icon(
                        _obscure ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                        size: 18, color: AppColors.hint),
                  )),
              const SizedBox(height: 10),

              // Forgot password
              Align(
                alignment: Alignment.centerRight,
                child: Text('Forgot Password?',
                    style: AppTextStyles.labelMedium
                        .copyWith(color: AppColors.primary)),
              ),
              const SizedBox(height: 28),

              // Login btn
              SizedBox(
                width: double.infinity,
                child: GestureDetector(
                  onTap: () async {
                    await AppState.instance.loginAsUser();
                    if (!mounted) return;
                    Navigator.pushNamedAndRemoveUntil(
                        context, AppRoutes.home, (r) => false);
                  },
                  child: Container(
                    height: 52,
                    decoration: BoxDecoration(
                        color: AppColors.primary,
                        borderRadius: BorderRadius.circular(14)),
                    child: Center(
                      child: Text('Sign In',
                          style: AppTextStyles.labelLarge
                              .copyWith(color: Colors.white, fontSize: 16)),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 20),

              // Divider
              Row(children: [
                const Expanded(child: Divider(color: AppColors.divider)),
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                  child: Text('or continue with',
                      style: AppTextStyles.bodySmall),
                ),
                const Expanded(child: Divider(color: AppColors.divider)),
              ]),
              const SizedBox(height: 20),

              // Social btns
              Row(children: [
                Expanded(child: _socialBtn('Google', '🇬')),
                const SizedBox(width: 12),
                Expanded(child: _socialBtn('Apple', '🍎')),
              ]),
              const SizedBox(height: 16),

              // Guest mode
              SizedBox(
                width: double.infinity,
                child: GestureDetector(
                  onTap: () async {
                    await AppState.instance.loginAsGuest();
                    if (!mounted) return;
                    Navigator.pushNamedAndRemoveUntil(
                        context, AppRoutes.home, (r) => false);
                  },
                  child: Container(
                    height: 52,
                    decoration: BoxDecoration(
                      color: AppColors.surface,
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(color: AppColors.divider),
                    ),
                    child: Center(
                      child: Row(
                        mainAxisSize: MainAxisSize.min,
                        children: [
                          const Icon(Icons.person_outline_rounded,
                              size: 18, color: AppColors.textMuted),
                          const SizedBox(width: 8),
                          Text('Continue as Guest',
                              style: AppTextStyles.labelLarge
                                  .copyWith(color: AppColors.textMuted)),
                        ],
                      ),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 28),

              // Register link
              Center(
                child: GestureDetector(
                  onTap: () => Navigator.pushNamed(context, AppRoutes.register),
                  child: RichText(
                    text: TextSpan(
                      text: "Don't have an account? ",
                      style: AppTextStyles.bodyMedium,
                      children: [
                        TextSpan(text: 'Sign Up',
                            style: AppTextStyles.labelMedium
                                .copyWith(color: AppColors.primary)),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _label(String text) =>
      Text(text, style: AppTextStyles.labelMedium);

  Widget _textField({
    required TextEditingController controller,
    required String hint,
    required IconData icon,
    bool obscure = false,
    Widget? suffix,
    TextInputType? keyboardType,
  }) {
    return TextField(
      controller: controller,
      obscureText: obscure,
      keyboardType: keyboardType,
      decoration: InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: AppColors.hint, fontSize: 14),
        prefixIcon: Icon(icon, size: 18, color: AppColors.hint),
        suffixIcon: suffix != null ? Padding(
            padding: const EdgeInsets.only(right: 12), child: suffix) : null,
        suffixIconConstraints: const BoxConstraints(),
        filled: true, fillColor: AppColors.surface,
        border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: BorderSide.none),
        contentPadding: const EdgeInsets.symmetric(
            horizontal: 14, vertical: 14),
      ),
    );
  }

  Widget _socialBtn(String label, String emoji) {
    return Container(
      height: 48,
      decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(color: AppColors.divider)),
      child: Center(
        child: Text('$emoji  $label',
            style: AppTextStyles.labelMedium),
      ),
    );
  }
}

// ─── Register Screen ──────────────────────────────────────────────────────────
class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});

  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _nameCtrl     = TextEditingController();
  final _emailCtrl    = TextEditingController();
  final _passwordCtrl = TextEditingController();
  bool _obscure       = true;
  bool _agreed        = false;

  @override
  void dispose() {
    _nameCtrl.dispose(); _emailCtrl.dispose(); _passwordCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(28),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 10),
              GestureDetector(
                onTap: () => Navigator.pop(context),
                child: const Icon(Icons.arrow_back_ios_new_rounded,
                    size: 18, color: AppColors.textMain),
              ),
              const SizedBox(height: 24),
              Text('Create Account', style: AppTextStyles.headlineMedium),
              const SizedBox(height: 6),
              Text('Join LushThreads today!', style: AppTextStyles.bodyMedium),
              const SizedBox(height: 36),

              _label('Full Name'),
              const SizedBox(height: 8),
              _textField(_nameCtrl, 'Your full name', Icons.person_outline),
              const SizedBox(height: 16),

              _label('Email Address'),
              const SizedBox(height: 8),
              _textField(_emailCtrl, 'you@example.com', Icons.email_outlined,
                  keyboardType: TextInputType.emailAddress),
              const SizedBox(height: 16),

              _label('Password'),
              const SizedBox(height: 8),
              _textField(_passwordCtrl, '••••••••', Icons.lock_outline,
                  obscure: _obscure,
                  suffix: GestureDetector(
                    onTap: () => setState(() => _obscure = !_obscure),
                    child: Icon(
                        _obscure ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                        size: 18, color: AppColors.hint),
                  )),
              const SizedBox(height: 16),

              // Terms checkbox
              GestureDetector(
                onTap: () => setState(() => _agreed = !_agreed),
                child: Row(children: [
                  Container(
                    width: 22, height: 22,
                    decoration: BoxDecoration(
                      color: _agreed ? AppColors.primary : Colors.transparent,
                      borderRadius: BorderRadius.circular(6),
                      border: Border.all(
                          color: _agreed ? AppColors.primary : AppColors.hint),
                    ),
                    child: _agreed
                        ? const Icon(Icons.check, size: 14, color: Colors.white)
                        : null,
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: RichText(
                      text: TextSpan(
                        text: 'I agree to the ',
                        style: AppTextStyles.bodySmall,
                        children: [
                          TextSpan(text: 'Terms & Conditions',
                              style: AppTextStyles.labelSmall
                                  .copyWith(color: AppColors.primary)),
                          const TextSpan(text: ' and '),
                          TextSpan(text: 'Privacy Policy',
                              style: AppTextStyles.labelSmall
                                  .copyWith(color: AppColors.primary)),
                        ],
                      ),
                    ),
                  ),
                ]),
              ),
              const SizedBox(height: 28),

              // Email verification notice
              Container(
                padding: const EdgeInsets.all(14),
                decoration: BoxDecoration(
                    color: const Color(0xFFFFF3CD),
                    borderRadius: BorderRadius.circular(12),
                    border: Border.all(color: AppColors.accent.withOpacity(0.5))),
                child: Row(children: [
                  const Icon(Icons.mail_outline_rounded,
                      color: Color(0xFF856404), size: 20),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text('A verification email will be sent after registration.',
                        style: AppTextStyles.bodySmall
                            .copyWith(color: const Color(0xFF856404))),
                  ),
                ]),
              ),
              const SizedBox(height: 24),

              SizedBox(
                width: double.infinity,
                child: GestureDetector(
                  onTap: () async {
                    if (!_agreed) return;
                    await AppState.instance.loginAsUser();
                    if (!mounted) return;
                    Navigator.pushNamedAndRemoveUntil(
                        context, AppRoutes.home, (r) => false);
                  },
                  child: Container(
                    height: 52,
                    decoration: BoxDecoration(
                        color: _agreed ? AppColors.primary : AppColors.hint,
                        borderRadius: BorderRadius.circular(14)),
                    child: Center(
                      child: Text('Create Account',
                          style: AppTextStyles.labelLarge
                              .copyWith(color: Colors.white, fontSize: 16)),
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 24),

              Center(
                child: GestureDetector(
                  onTap: () => Navigator.pop(context),
                  child: RichText(
                    text: TextSpan(
                      text: 'Already have an account? ',
                      style: AppTextStyles.bodyMedium,
                      children: [
                        TextSpan(text: 'Sign In',
                            style: AppTextStyles.labelMedium
                                .copyWith(color: AppColors.primary)),
                      ],
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _label(String text) => Text(text, style: AppTextStyles.labelMedium);

  Widget _textField(TextEditingController ctrl, String hint, IconData icon,
      {bool obscure = false, Widget? suffix, TextInputType? keyboardType}) {
    return TextField(
      controller: ctrl,
      obscureText: obscure,
      keyboardType: keyboardType,
      decoration: InputDecoration(
        hintText: hint,
        hintStyle: const TextStyle(color: AppColors.hint, fontSize: 14),
        prefixIcon: Icon(icon, size: 18, color: AppColors.hint),
        suffixIcon: suffix != null
            ? Padding(padding: const EdgeInsets.only(right: 12), child: suffix)
            : null,
        suffixIconConstraints: const BoxConstraints(),
        filled: true, fillColor: AppColors.surface,
        border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: BorderSide.none),
        contentPadding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14),
      ),
    );
  }
}
