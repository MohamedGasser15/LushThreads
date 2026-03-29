import 'package:flutter/material.dart';
import 'core/theme/app_theme.dart';
import 'core/constants/app_routes.dart';

import 'features/splash/presentation/screens/splash_screen.dart';
import 'features/onboarding/presentation/screens/onboarding_screen.dart';
import 'features/home/presentation/screens/home_screen.dart';
import 'features/shop/presentation/screens/shop_screen.dart';
import 'features/product_detail/presentation/screens/product_detail_screen.dart';
import 'features/cart/presentation/screens/cart_screen.dart';
import 'features/cart/presentation/screens/checkout_screen.dart';
import 'features/auth/presentation/screens/auth_screens.dart';
import 'features/profile/presentation/screens/profile_screen.dart';

void main() => runApp(const LushThreadsApp());

class LushThreadsApp extends StatelessWidget {
  const LushThreadsApp({super.key});

  // ── Page map ──────────────────────────────────────────────────────────────
  static final Map<String, WidgetBuilder> _pages = {
    AppRoutes.splash:        (_) => const SplashScreen(),
    AppRoutes.onboarding:    (_) => const OnboardingScreen(),
    AppRoutes.home:          (_) => const HomeScreen(),
    AppRoutes.shop:          (_) => const ShopScreen(),
    AppRoutes.productDetail: (_) => const ProductDetailScreen(),
    AppRoutes.cart:          (_) => const CartScreen(),
    AppRoutes.checkout:      (_) => const CheckoutScreen(),
    AppRoutes.orderSuccess:  (_) => const OrderSuccessScreen(),
    AppRoutes.login:         (_) => const LoginScreen(),
    AppRoutes.register:      (_) => const RegisterScreen(),
    AppRoutes.profile:       (_) => const ProfileScreen(),
  };

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'LushThreads',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      initialRoute: AppRoutes.splash,
      // ── Smooth transitions on ALL routes ──────────────────────────────────
      onGenerateRoute: (settings) {
        final builder = _pages[settings.name];
        if (builder == null) return null;

        return PageRouteBuilder(
          settings: settings,
          transitionDuration: const Duration(milliseconds: 280),
          reverseTransitionDuration: const Duration(milliseconds: 220),
          pageBuilder: (ctx, _, __) => builder(ctx),
          transitionsBuilder: (_, animation, __, child) {
            return FadeTransition(
              opacity: CurvedAnimation(
                  parent: animation, curve: Curves.easeOut),
              child: SlideTransition(
                position: Tween<Offset>(
                  begin: const Offset(0, 0.03),
                  end: Offset.zero,
                ).animate(CurvedAnimation(
                    parent: animation, curve: Curves.easeOut)),
                child: child,
              ),
            );
          },
        );
      },
    );
  }
}