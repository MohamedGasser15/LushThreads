import 'package:flutter/material.dart';
import '../../core/theme/app_colors.dart';
import '../../core/constants/app_routes.dart';
import '../models/cart.dart';

class HideOnScrollBottomNav extends StatefulWidget {
  final int currentIndex;
  final ScrollController scrollController;
  final Widget child;

  const HideOnScrollBottomNav({
    super.key,
    required this.currentIndex,
    required this.scrollController,
    required this.child,
  });

  @override
  State<HideOnScrollBottomNav> createState() => _HideOnScrollBottomNavState();
}

class _HideOnScrollBottomNavState extends State<HideOnScrollBottomNav>
    with SingleTickerProviderStateMixin {
  late AnimationController _ctrl;
  late Animation<Offset> _slide;
  bool _visible = true;
  double _last = 0;

  @override
  void initState() {
    super.initState();
    _ctrl = AnimationController(
        vsync: this, duration: const Duration(milliseconds: 250));
    _slide = Tween<Offset>(begin: Offset.zero, end: const Offset(0, 1.5))
        .animate(CurvedAnimation(parent: _ctrl, curve: Curves.easeInOut));
    widget.scrollController.addListener(_onScroll);
  }

  void _onScroll() {
    final off = widget.scrollController.offset;
    final diff = off - _last;
    if (diff > 6 && _visible && off > 60) {
      _visible = false;
      _ctrl.forward();
    } else if (diff < -6 && !_visible) {
      _visible = true;
      _ctrl.reverse();
    }
    _last = off;
  }

  @override
  void dispose() {
    widget.scrollController.removeListener(_onScroll);
    _ctrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        // ✅ المحتوى بياخد المساحة كلها — مفيش padding
        widget.child,
        // ✅ Nav فوق المحتوى كـ overlay
        Positioned(
          left: 0, right: 0, bottom: 0,
          child: SlideTransition(
            position: _slide,
            child: _NavBar(currentIndex: widget.currentIndex),
          ),
        ),
      ],
    );
  }
}

class AppBottomNav extends StatelessWidget {
  final int currentIndex;
  const AppBottomNav({super.key, required this.currentIndex});

  @override
  Widget build(BuildContext context) => _NavBar(currentIndex: currentIndex);
}

class _NavBar extends StatelessWidget {
  final int currentIndex;
  const _NavBar({required this.currentIndex});

  static const _items = [
    (icon: Icons.home_rounded,            label: 'Home',       route: AppRoutes.home),
    (icon: Icons.grid_view_rounded,       label: 'Categories', route: AppRoutes.shop),
    (icon: Icons.favorite_border_rounded, label: 'Wishlist',   route: AppRoutes.home),
    (icon: Icons.shopping_bag_rounded,    label: 'Cart',       route: AppRoutes.cart),
    (icon: Icons.person_rounded,          label: 'Profile',    route: AppRoutes.profile),
  ];

  @override
  Widget build(BuildContext context) {
    final cartCount = CartManager.instance.totalCount;

    return Container(
      height: 70,
      decoration: BoxDecoration(
        color: Colors.white,
        boxShadow: [
          BoxShadow(
            color: AppColors.primary.withOpacity(0.10),
            blurRadius: 24,
            offset: const Offset(0, -6),
          ),
        ],
        borderRadius: const BorderRadius.vertical(top: Radius.circular(20)),
      ),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceAround,
        children: List.generate(_items.length, (i) {
          final it = _items[i];
          final active = i == currentIndex;
          final isCart = i == 3;

          return GestureDetector(
            onTap: () {
              if (i == currentIndex) return;
              Navigator.pushNamedAndRemoveUntil(
                  context, it.route, (r) => false);
            },
            behavior: HitTestBehavior.opaque,
            child: SizedBox(
              width: 60,
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Stack(clipBehavior: Clip.none, children: [
                    AnimatedContainer(
                      duration: const Duration(milliseconds: 200),
                      padding: const EdgeInsets.all(6),
                      decoration: BoxDecoration(
                        color: active ? AppColors.primaryLight : Colors.transparent,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Icon(it.icon, size: 22,
                          color: active ? AppColors.primary : AppColors.hint),
                    ),
                    if (isCart && cartCount > 0)
                      Positioned(
                        top: -2, right: -4,
                        child: Container(
                          width: 16, height: 16,
                          decoration: const BoxDecoration(
                              color: AppColors.accent, shape: BoxShape.circle),
                          child: Center(
                            child: Text('$cartCount',
                                style: const TextStyle(
                                    fontSize: 9,
                                    fontWeight: FontWeight.w700,
                                    color: AppColors.textMain)),
                          ),
                        ),
                      ),
                  ]),
                  const SizedBox(height: 3),
                  AnimatedDefaultTextStyle(
                    duration: const Duration(milliseconds: 200),
                    style: TextStyle(
                      fontSize: 10,
                      fontWeight: FontWeight.w600,
                      color: active ? AppColors.primary : AppColors.hint,
                    ),
                    child: Text(it.label),
                  ),
                ],
              ),
            ),
          );
        }),
      ),
    );
  }
}