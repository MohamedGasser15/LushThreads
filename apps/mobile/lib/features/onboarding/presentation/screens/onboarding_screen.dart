import 'dart:async';
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
    with SingleTickerProviderStateMixin {
  final PageController _pageCtrl = PageController();
  late AnimationController _progressCtrl;
  int _currentPage = 0;
  bool _isAnimating = false;

  static const _duration = Duration(seconds: 4);

  static const _pages = [
    _OnboardingPage(
      emoji: '🛍️',
      title: 'Discover Premium Fashion',
      subtitle: 'Browse thousands of styles from top brands like Zara, H&M, and Adidas — all in one place.',
    ),
    _OnboardingPage(
      emoji: '⚡',
      title: 'Exclusive Deals Daily',
      subtitle: 'Get up to 70% off on seasonal collections. New offers drop every day — never miss a deal.',
    ),
    _OnboardingPage(
      emoji: '❤️',
      title: 'Save Your Wishlist',
      subtitle: 'Heart any item you love and save it for later. Your wishlist is always just a tap away.',
    ),
    _OnboardingPage(
      emoji: '📦',
      title: 'Track Every Order',
      subtitle: 'From checkout to your doorstep — track your orders in real time and get notified instantly.',
    ),
  ];

  @override
  void initState() {
    super.initState();
    _progressCtrl = AnimationController(vsync: this, duration: _duration);
    WidgetsBinding.instance.addPostFrameCallback((_) => _startPage(0));
  }

  void _startPage(int page) {
    if (!mounted) return;
    _progressCtrl.reset();
    _progressCtrl.forward().then((_) {
      if (mounted && page == _currentPage) _goNext();
    });
  }

  void _goNext() {
    if (_isAnimating) return;
    if (_currentPage < _pages.length - 1) {
      _animateTo(_currentPage + 1);
    } else {
      _finish();
    }
  }

  void _goPrev() {
    if (_isAnimating || _currentPage == 0) return;
    _animateTo(_currentPage - 1);
  }

  void _animateTo(int page) {
    if (_isAnimating) return;
    _isAnimating = true;
    _progressCtrl.stop();
    _pageCtrl.animateToPage(
      page,
      duration: const Duration(milliseconds: 400),
      curve: Curves.easeInOut,
    ).then((_) {
      if (!mounted) return;
      setState(() {
        _currentPage = page;
        _isAnimating = false;
      });
      _startPage(page);
    });
  }

  void _jumpTo(int page) {
    if (_isAnimating || page == _currentPage) return;
    _animateTo(page);
  }

  void _finish() {
    AppState.instance.completeOnboarding();
    Navigator.pushReplacementNamed(context, AppRoutes.login);
  }

  @override
  void dispose() {
    _pageCtrl.dispose();
    _progressCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            // ── Story progress bar ──────────────────────────────────────────
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 16, 20, 0),
              child: _StoryProgressBar(
                currentPage: _currentPage,
                totalPages: _pages.length,
                progressCtrl: _progressCtrl,
                onTap: _jumpTo,
              ),
            ),
            const SizedBox(height: 8),

            // ── Page content ────────────────────────────────────────────────
            Expanded(
              child: Stack(
                children: [
                  PageView.builder(
                    controller: _pageCtrl,
                    physics: const NeverScrollableScrollPhysics(),
                    itemCount: _pages.length,
                    itemBuilder: (_, i) => _PageContent(page: _pages[i]),
                  ),
                  // Left tap → prev
                  Positioned(
                    left: 0, top: 0, bottom: 0,
                    child: GestureDetector(
                      behavior: HitTestBehavior.translucent,
                      onTap: _goPrev,
                      child: const SizedBox(width: 90),
                    ),
                  ),
                  // Right tap → next
                  Positioned(
                    right: 0, top: 0, bottom: 0,
                    child: GestureDetector(
                      behavior: HitTestBehavior.translucent,
                      onTap: _goNext,
                      child: const SizedBox(width: 90),
                    ),
                  ),
                ],
              ),
            ),

            // ── Bottom buttons ──────────────────────────────────────────────
            Padding(
              padding: const EdgeInsets.fromLTRB(28, 0, 28, 40),
              child: Column(
                children: [
                  // Get Started / Next
                  SizedBox(
                    width: double.infinity,
                    height: 56,
                    child: ElevatedButton(
                      onPressed: _currentPage == _pages.length - 1 ? _finish : _goNext,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: AppColors.primary,
                        foregroundColor: Colors.white,
                        shape: RoundedRectangleBorder(
                            borderRadius: BorderRadius.circular(28)),
                        elevation: 0,
                        shadowColor: Colors.transparent,
                      ),
                      child: Text(
                        _currentPage == _pages.length - 1 ? 'Get Started' : 'Next',
                        style: const TextStyle(
                            fontSize: 16, fontWeight: FontWeight.w700),
                      ),
                    ),
                  ),
                  const SizedBox(height: 14),
                  // Continue as Guest
                  GestureDetector(
                    onTap: () {
                      AppState.instance.completeOnboarding();
                      AppState.instance.loginAsGuest();
                      Navigator.pushReplacementNamed(context, AppRoutes.home);
                    },
                    child: const Text(
                      'Continue as Guest',
                      style: TextStyle(
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                        color: AppColors.textMuted,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ─── Story Progress Bar ───────────────────────────────────────────────────────
class _StoryProgressBar extends StatefulWidget {
  final int currentPage;
  final int totalPages;
  final AnimationController progressCtrl;
  final Function(int) onTap;

  const _StoryProgressBar({
    required this.currentPage,
    required this.totalPages,
    required this.progressCtrl,
    required this.onTap,
  });

  @override
  State<_StoryProgressBar> createState() => _StoryProgressBarState();
}

class _StoryProgressBarState extends State<_StoryProgressBar> {
  late Animation<double> _anim;

  @override
  void initState() {
    super.initState();
    _rebuildAnim();
  }

  @override
  void didUpdateWidget(_StoryProgressBar old) {
    super.didUpdateWidget(old);
    if (old.currentPage != widget.currentPage) _rebuildAnim();
  }

  void _rebuildAnim() {
    _anim = Tween<double>(begin: 0, end: 1).animate(
      CurvedAnimation(parent: widget.progressCtrl, curve: Curves.linear),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Row(
      children: List.generate(widget.totalPages, (i) {
        return Expanded(
          child: GestureDetector(
            onTap: () => widget.onTap(i),
            child: Container(
              margin: const EdgeInsets.symmetric(horizontal: 3),
              height: 3,
              decoration: BoxDecoration(
                color: AppColors.primary.withOpacity(0.15),
                borderRadius: BorderRadius.circular(2),
              ),
              child: Stack(children: [
                // completed
                if (i < widget.currentPage)
                  Container(
                    decoration: BoxDecoration(
                      color: AppColors.primary,
                      borderRadius: BorderRadius.circular(2),
                    ),
                  ),
                // active — animated
                if (i == widget.currentPage)
                  AnimatedBuilder(
                    animation: _anim,
                    builder: (_, __) => FractionallySizedBox(
                      alignment: Alignment.centerLeft,
                      widthFactor: _anim.value,
                      child: Container(
                        decoration: BoxDecoration(
                          color: AppColors.primary,
                          borderRadius: BorderRadius.circular(2),
                        ),
                      ),
                    ),
                  ),
              ]),
            ),
          ),
        );
      }),
    );
  }
}

// ─── Single page content ──────────────────────────────────────────────────────
class _PageContent extends StatefulWidget {
  final _OnboardingPage page;
  const _PageContent({required this.page});

  @override
  State<_PageContent> createState() => _PageContentState();
}

class _PageContentState extends State<_PageContent>
    with SingleTickerProviderStateMixin {
  late AnimationController _ctrl;
  late Animation<double> _scale;
  late Animation<double> _fade;
  late Animation<Offset> _slide;

  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 500));
    _scale = Tween<double>(begin: 0.7, end: 1.0).animate(
        CurvedAnimation(parent: _ctrl, curve: Curves.elasticOut));
    _fade = Tween<double>(begin: 0.0, end: 1.0).animate(
        CurvedAnimation(parent: _ctrl, curve: const Interval(0, 0.5)));
    _slide = Tween<Offset>(
            begin: const Offset(0, 0.1), end: Offset.zero)
        .animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeOut));
    _ctrl.forward();
  }

  @override
  void dispose() {
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return FadeTransition(
      opacity: _fade,
      child: SlideTransition(
        position: _slide,
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              // Emoji circle
              ScaleTransition(
                scale: _scale,
                child: Container(
                  width: 200, height: 200,
                  decoration: BoxDecoration(
                    color: AppColors.primaryLight,
                    shape: BoxShape.circle,
                  ),
                  child: Center(
                    child: Text(widget.page.emoji,
                        style: const TextStyle(fontSize: 88)),
                  ),
                ),
              ),
              const SizedBox(height: 48),

              // Title
              Text(
                widget.page.title,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 26,
                  fontWeight: FontWeight.w800,
                  color: AppColors.textMain,
                  height: 1.2,
                ),
              ),
              const SizedBox(height: 14),

              // Subtitle
              Text(
                widget.page.subtitle,
                textAlign: TextAlign.center,
                style: const TextStyle(
                  fontSize: 15,
                  fontWeight: FontWeight.w400,
                  color: AppColors.textMuted,
                  height: 1.65,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// ─── Data class ───────────────────────────────────────────────────────────────
class _OnboardingPage {
  final String emoji;
  final String title;
  final String subtitle;

  const _OnboardingPage({
    required this.emoji,
    required this.title,
    required this.subtitle,
  });
}
