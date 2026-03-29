import 'package:flutter/material.dart';
import '../../../../core/theme/app_colors.dart';
import '../../../../core/constants/app_routes.dart';
import '../../../../shared/providers/app_state.dart';

class OnboardingScreen extends StatefulWidget {
  const OnboardingScreen({super.key});

  @override
  State<OnboardingScreen> createState() => _OnboardingScreenState();
}

class _OnboardingScreenState extends State<OnboardingScreen>
    with TickerProviderStateMixin {
  final PageController _pageCtrl = PageController();
  int _currentPage = 0;

  late AnimationController _contentCtrl;
  late Animation<double> _contentFade;
  late Animation<Offset> _contentSlide;

  static const _pages = [
    _OnboardingPage(
      emoji: '🛍️',
      bgColor: Color(0xFFE1F5EE),
      accentColor: AppColors.primary,
      title: 'Discover Premium Fashion',
      subtitle:
          'Browse thousands of styles from top brands like Zara, H&M, and Adidas — all in one place.',
    ),
    _OnboardingPage(
      emoji: '⚡',
      bgColor: Color(0xFFFFF9E6),
      accentColor: Color(0xFFD4AF37),
      title: 'Exclusive Deals Daily',
      subtitle:
          'Get up to 70% off on seasonal collections. New offers drop every day — never miss a deal.',
    ),
    _OnboardingPage(
      emoji: '❤️',
      bgColor: Color(0xFFFFEEF3),
      accentColor: Color(0xFFFF2D87),
      title: 'Save Your Wishlist',
      subtitle:
          'Heart any item you love and save it for later. Your wishlist is always just a tap away.',
    ),
    _OnboardingPage(
      emoji: '📦',
      bgColor: Color(0xFFE8F4FD),
      accentColor: Color(0xFF1565C0),
      title: 'Track Every Order',
      subtitle:
          'From checkout to your doorstep — track your orders in real time and get notified instantly.',
    ),
  ];

  @override
  void initState() {
    super.initState();
    _contentCtrl = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 400));
    _contentFade = Tween<double>(begin: 0.0, end: 1.0)
        .animate(CurvedAnimation(parent: _contentCtrl, curve: Curves.easeOut));
    _contentSlide = Tween<Offset>(
            begin: const Offset(0.0, 0.12), end: Offset.zero)
        .animate(
            CurvedAnimation(parent: _contentCtrl, curve: Curves.easeOut));
    _contentCtrl.forward();
  }

  @override
  void dispose() {
    _pageCtrl.dispose();
    _contentCtrl.dispose();
    super.dispose();
  }

  void _next() {
    if (_currentPage < _pages.length - 1) {
      _pageCtrl.nextPage(
        duration: const Duration(milliseconds: 350),
        curve: Curves.easeInOut,
      );
    } else {
      _finish();
    }
  }

  void _skip() => _finish();

  void _finish() {
    AppState.instance.completeOnboarding();
    Navigator.pushReplacementNamed(context, AppRoutes.login);
  }

  void _onPageChanged(int page) {
    setState(() => _currentPage = page);
    _contentCtrl.reset();
    _contentCtrl.forward();
  }

  @override
  Widget build(BuildContext context) {
    final page = _pages[_currentPage];

    return Scaffold(
      backgroundColor: page.bgColor,
      body: SafeArea(
        child: AnimatedContainer(
          duration: const Duration(milliseconds: 400),
          color: page.bgColor,
          child: Column(
            children: [
              // ── Skip button ─────────────────────────────────────────────
              Align(
                alignment: Alignment.topRight,
                child: Padding(
                  padding: const EdgeInsets.fromLTRB(0, 12, 20, 0),
                  child: GestureDetector(
                    onTap: _skip,
                    child: Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 16, vertical: 8),
                      decoration: BoxDecoration(
                        color: Colors.black.withOpacity(0.06),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: const Text('Skip',
                          style: TextStyle(
                              fontSize: 13,
                              fontWeight: FontWeight.w600,
                              color: Color(0xFF4A4A4A))),
                    ),
                  ),
                ),
              ),

              // ── PageView ───────────────────────────────────────────────
              Expanded(
                child: PageView.builder(
                  controller: _pageCtrl,
                  onPageChanged: _onPageChanged,
                  itemCount: _pages.length,
                  itemBuilder: (_, i) => _buildPageContent(_pages[i]),
                ),
              ),

              // ── Bottom area ─────────────────────────────────────────────
              Padding(
                padding: const EdgeInsets.fromLTRB(32, 0, 32, 40),
                child: Column(
                  children: [
                    // Dots
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: List.generate(_pages.length, (i) {
                        final active = i == _currentPage;
                        return AnimatedContainer(
                          duration: const Duration(milliseconds: 300),
                          margin: const EdgeInsets.symmetric(horizontal: 4),
                          width: active ? 24 : 8,
                          height: 8,
                          decoration: BoxDecoration(
                            color: active
                                ? page.accentColor
                                : page.accentColor.withOpacity(0.25),
                            borderRadius: BorderRadius.circular(4),
                          ),
                        );
                      }),
                    ),
                    const SizedBox(height: 32),

                    // Next / Get Started button
                    SizedBox(
                      width: double.infinity,
                      child: GestureDetector(
                        onTap: _next,
                        child: AnimatedContainer(
                          duration: const Duration(milliseconds: 300),
                          height: 56,
                          decoration: BoxDecoration(
                            color: page.accentColor,
                            borderRadius: BorderRadius.circular(28),
                            boxShadow: [
                              BoxShadow(
                                color: page.accentColor.withOpacity(0.35),
                                blurRadius: 20,
                                offset: const Offset(0, 8),
                              ),
                            ],
                          ),
                          child: Center(
                            child: Text(
                              _currentPage == _pages.length - 1
                                  ? 'Get Started'
                                  : 'Next',
                              style: const TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.w700,
                                color: Colors.white,
                              ),
                            ),
                          ),
                        ),
                      ),
                    ),

                    // Login link
                    if (_currentPage == _pages.length - 1) ...[
                      const SizedBox(height: 16),
                      GestureDetector(
                        onTap: () {
                          AppState.instance.completeOnboarding();
                          AppState.instance.loginAsGuest();
                          Navigator.pushReplacementNamed(
                              context, AppRoutes.home);
                        },
                        child: Text(
                          'Continue as Guest',
                          style: TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            color: page.accentColor,
                          ),
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildPageContent(_OnboardingPage page) {
    return SlideTransition(
      position: _contentSlide,
      child: FadeTransition(
        opacity: _contentFade,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              // Emoji illustration
              Container(
                width: 200,
                height: 200,
                decoration: BoxDecoration(
                  color: page.accentColor.withOpacity(0.12),
                  shape: BoxShape.circle,
                ),
                child: Center(
                  child: Text(page.emoji,
                      style: const TextStyle(fontSize: 90)),
                ),
              ),
              const SizedBox(height: 48),

              // Title
              Text(
                page.title,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.w800,
                  color: Color(0xFF1A1A1A),
                  height: 1.2,
                ),
              ),
              const SizedBox(height: 16),

              // Subtitle
              Text(
                page.subtitle,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.w400,
                  color: Color(0xFF4A4A4A),
                  height: 1.6,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _OnboardingPage {
  final String emoji;
  final Color bgColor;
  final Color accentColor;
  final String title;
  final String subtitle;

  const _OnboardingPage({
    required this.emoji,
    required this.bgColor,
    required this.accentColor,
    required this.title,
    required this.subtitle,
  });
}
