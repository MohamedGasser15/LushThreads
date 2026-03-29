import 'package:flutter/material.dart';

class AppRoutes {
  AppRoutes._();

  static const String splash        = '/';
  static const String onboarding    = '/onboarding';
  static const String home          = '/home';
  static const String shop          = '/shop';
  static const String productDetail = '/product-detail';
  static const String cart          = '/cart';
  static const String checkout      = '/checkout';
  static const String orderSuccess  = '/order-success';
  static const String profile       = '/profile';
  static const String login         = '/login';
  static const String register      = '/register';
  static const String wishlist      = '/wishlist';

  // ── Smooth fade+slide navigation ─────────────────────────────────────────
  static Future<void> navigateTo(BuildContext context, String route,
      {Object? arguments}) {
    return Navigator.of(context).pushAndRemoveUntil(
      _fadeSlideRoute(route, arguments),
      (r) => false,
    );
  }

  static Future<void> push(BuildContext context, String route,
      {Object? arguments}) {
    return Navigator.of(context).push(
      _fadeSlideRoute(route, arguments),
    );
  }

  static PageRouteBuilder _fadeSlideRoute(String route, Object? arguments) {
    return PageRouteBuilder(
      settings: RouteSettings(name: route, arguments: arguments),
      transitionDuration: const Duration(milliseconds: 280),
      reverseTransitionDuration: const Duration(milliseconds: 220),
      pageBuilder: (context, animation, secondaryAnimation) {
        // pageBuilder بيرجع widget فاضي — الـ routes بتتحدد في main.dart
        return const SizedBox.shrink();
      },
      transitionsBuilder: (context, animation, secondaryAnimation, child) {
        // Fade + subtle upward slide
        final fadeTween = Tween<double>(begin: 0.0, end: 1.0)
            .chain(CurveTween(curve: Curves.easeOut));
        final slideTween =
            Tween<Offset>(begin: const Offset(0, 0.04), end: Offset.zero)
                .chain(CurveTween(curve: Curves.easeOut));

        return FadeTransition(
          opacity: animation.drive(fadeTween),
          child: SlideTransition(
            position: animation.drive(slideTween),
            child: child,
          ),
        );
      },
    );
  }
}