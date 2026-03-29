import 'package:flutter/material.dart';
import 'package:LushThreads/core/constants/app_routes.dart';
import 'package:LushThreads/core/theme/app_colors.dart';
import 'package:LushThreads/core/theme/app_text_styles.dart';
import 'package:LushThreads/shared/models/cart.dart';
import 'package:LushThreads/shared/models/product.dart';
import 'package:LushThreads/shared/widgets/app_bottom_nav.dart';
import 'package:LushThreads/shared/widgets/product_card.dart';
import 'dart:async';
class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _selectedCat = 0;
  final _scrollCtrl = ScrollController();
  final _bannerCtrl = PageController();
  int _currentBanner = 0;
  Timer? _bannerTimer;

  // ── Banner data ─────────────────────────────────────────────────────────
  static const _banners = [
    _BannerData(
      tag: 'Limited Offer',
      title: 'Up to 70%\nOff Today',
      subtitle: 'Exclusive deals across all categories',
      gradient: [Color(0xFF088170), Color(0xFF05594D)],
    ),
    _BannerData(
      tag: 'New Arrivals',
      title: 'Fresh Styles\nJust Dropped',
      subtitle: 'Shop the latest from Zara & H&M',
      gradient: [Color(0xFF1565C0), Color(0xFF0D47A1)],
    ),
    _BannerData(
      tag: 'Summer Sale',
      title: 'Cool Looks\nHot Prices',
      subtitle: 'Up to 50% off on summer collection',
      gradient: [Color(0xFFD4AF37), Color(0xFFB8860B)],
    ),
  ];

  List<Product> get _filtered {
    if (_selectedCat == 0) return allProducts.take(6).toList();
    final cat = allCategories[_selectedCat];
    return allProducts
        .where((p) => p.category.contains(cat.replaceAll("'", '')))
        .take(6)
        .toList();
  }

  @override
  void initState() {
    super.initState();
    _startBannerTimer();
  }

  void _startBannerTimer() {
    _bannerTimer = Timer.periodic(const Duration(seconds: 3), (_) {
      if (!mounted) return;
      final next = (_currentBanner + 1) % _banners.length;
      _bannerCtrl.animateToPage(
        next,
        duration: const Duration(milliseconds: 500),
        curve: Curves.easeInOut,
      );
    });
  }

  @override
  void dispose() {
    _bannerTimer?.cancel();
    _bannerCtrl.dispose();
    _scrollCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            _buildTopBar(),
            Expanded(
              child: HideOnScrollBottomNav(
                currentIndex: 0,
                scrollController: _scrollCtrl,
                child: SingleChildScrollView(
                  controller: _scrollCtrl,
                  // ✅ padding أسفل عشان آخر عنصر يظهر فوق الـ nav
                  padding: const EdgeInsets.only(bottom: 80),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      _buildSearchBar(),
                      _buildHeroBanners(),
                      const SizedBox(height: 16),
                      _buildPromoStrip(),
                      const SizedBox(height: 20),
                      _buildCategories(),
                      const SizedBox(height: 20),
                      _buildProductsGrid(),
                      const SizedBox(height: 20),
                      _buildBrands(),
                    ],
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  // ── Top Bar ───────────────────────────────────────────────────────────────
  Widget _buildTopBar() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 8, 20, 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Text('Good morning 👋',
                style: AppTextStyles.bodySmall.copyWith(color: AppColors.hint)),
            Text('LushThreads', style: AppTextStyles.logoStyle),
          ]),
          Row(children: [
            _iconBtn(const Icon(Icons.notifications_outlined,
                size: 20, color: AppColors.textMain)),
            const SizedBox(width: 10),
            _cartBtn(),
            const SizedBox(width: 10),
            const CircleAvatar(
                radius: 18,
                backgroundColor: AppColors.primary,
                child: Text('M',
                    style: TextStyle(
                        color: Colors.white,
                        fontWeight: FontWeight.w700,
                        fontSize: 14))),
          ]),
        ],
      ),
    );
  }

  Widget _iconBtn(Widget icon) => Container(
        width: 36, height: 36,
        decoration: const BoxDecoration(
            color: AppColors.surface, shape: BoxShape.circle),
        child: Center(child: icon),
      );

  Widget _cartBtn() {
    final count = CartManager.instance.totalCount;
    return GestureDetector(
      onTap: () => Navigator.pushNamed(context, AppRoutes.cart),
      child: _iconBtn(
        Stack(clipBehavior: Clip.none, children: [
          const Icon(Icons.shopping_bag_outlined,
              size: 20, color: AppColors.textMain),
          if (count > 0)
            Positioned(
              top: -4, right: -4,
              child: Container(
                width: 15, height: 15,
                decoration: const BoxDecoration(
                    color: AppColors.accent, shape: BoxShape.circle),
                child: Center(
                    child: Text('$count',
                        style: const TextStyle(
                            fontSize: 9,
                            fontWeight: FontWeight.w700,
                            color: AppColors.textMain))),
              ),
            ),
        ]),
      ),
    );
  }

  // ── Search ────────────────────────────────────────────────────────────────
  Widget _buildSearchBar() {
    return GestureDetector(
      onTap: () => Navigator.pushNamed(context, AppRoutes.shop),
      child: Padding(
        padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 12),
          decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(12)),
          child: const Row(children: [
            Icon(Icons.search, size: 18, color: AppColors.hint),
            SizedBox(width: 8),
            Text('Search clothes, brands...',
                style: TextStyle(fontSize: 13, color: AppColors.hint)),
          ]),
        ),
      ),
    );
  }

  // ── Dynamic Hero Banners ──────────────────────────────────────────────────
  Widget _buildHeroBanners() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Column(
        children: [
          SizedBox(
            height: 200,
            child: PageView.builder(
              controller: _bannerCtrl,
              onPageChanged: (i) => setState(() => _currentBanner = i),
              itemCount: _banners.length,
              itemBuilder: (_, i) => _buildBannerSlide(_banners[i]),
            ),
          ),
          const SizedBox(height: 10),
          // Dots
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: List.generate(_banners.length, (i) {
              final active = i == _currentBanner;
              return AnimatedContainer(
                duration: const Duration(milliseconds: 300),
                margin: const EdgeInsets.symmetric(horizontal: 3),
                width: active ? 20 : 6,
                height: 6,
                decoration: BoxDecoration(
                  color: active
                      ? AppColors.primary
                      : AppColors.primary.withOpacity(0.25),
                  borderRadius: BorderRadius.circular(3),
                ),
              );
            }),
          ),
        ],
      ),
    );
  }

Widget _buildBannerSlide(_BannerData banner) {
  return Container(
    margin: const EdgeInsets.symmetric(horizontal: 4),
    decoration: BoxDecoration(
      borderRadius: BorderRadius.circular(20),
      gradient: LinearGradient(
        colors: banner.gradient,
        begin: Alignment.topLeft,
        end: Alignment.bottomRight,
      ),
    ),
    padding: const EdgeInsets.all(20),
    child: Stack(children: [
      // ✅ Column داخل Flexible عشان ميتعداش الـ height
      Positioned.fill(
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          mainAxisAlignment: MainAxisAlignment.end,
          children: [
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
              decoration: BoxDecoration(
                  color: Colors.white.withOpacity(0.25),
                  borderRadius: BorderRadius.circular(6)),
              child: Text(banner.tag,
                  style: const TextStyle(
                      fontSize: 11,
                      fontWeight: FontWeight.w700,
                      color: Colors.white)),
            ),
            const SizedBox(height: 6),
            Text(banner.title,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
                style: const TextStyle(
                    fontSize: 22,
                    fontWeight: FontWeight.w800,
                    color: Colors.white,
                    height: 1.15)),
            const SizedBox(height: 3),
            Text(banner.subtitle,
                maxLines: 1,
                overflow: TextOverflow.ellipsis,
                style: TextStyle(
                    fontSize: 12,
                    color: Colors.white.withOpacity(0.85))),
            const SizedBox(height: 10),
            GestureDetector(
              onTap: () => Navigator.pushNamed(context, AppRoutes.shop),
              child: Container(
                padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 8),
                decoration: BoxDecoration(
                    color: Colors.white.withOpacity(0.25),
                    borderRadius: BorderRadius.circular(20),
                    border: Border.all(
                        color: Colors.white.withOpacity(0.5), width: 1)),
                child: const Text('Shop Now',
                    style: TextStyle(
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                        color: Colors.white)),
              ),
            ),
          ],
        ),
      ),
    ]),
  );
}
  // ── Promo ─────────────────────────────────────────────────────────────────
  Widget _buildPromoStrip() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Container(
        padding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
        decoration: BoxDecoration(
          color: AppColors.accentLight,
          borderRadius: BorderRadius.circular(14),
          border: const Border(
              left: BorderSide(color: AppColors.accent, width: 3)),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.spaceBetween,
          children: [
            Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
              Text('Summer Sale is Live!', style: AppTextStyles.labelLarge),
              const SizedBox(height: 2),
              Text('Grab cool styles from H&M & Zara',
                  style: AppTextStyles.bodySmall),
            ]),
            Text('50%', style: AppTextStyles.priceLarge),
          ],
        ),
      ),
    );
  }

  // ── Categories ────────────────────────────────────────────────────────────
  Widget _buildCategories() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Column(children: [
        _sectionHeader('Categories', 'See all',
            () => Navigator.pushNamed(context, AppRoutes.shop)),
        const SizedBox(height: 12),
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(
            children: List.generate(allCategories.length, (i) {
              final active = i == _selectedCat;
              return GestureDetector(
                onTap: () => setState(() => _selectedCat = i),
                child: AnimatedContainer(
                  duration: const Duration(milliseconds: 200),
                  margin: const EdgeInsets.only(right: 8),
                  padding: const EdgeInsets.symmetric(
                      horizontal: 16, vertical: 9),
                  decoration: BoxDecoration(
                    color: active ? AppColors.primary : AppColors.surface,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(allCategories[i],
                      style: TextStyle(
                          fontSize: 13,
                          fontWeight: FontWeight.w600,
                          color: active
                              ? Colors.white
                              : AppColors.textMuted)),
                ),
              );
            }),
          ),
        ),
      ]),
    );
  }

  // ── Products ──────────────────────────────────────────────────────────────
  Widget _buildProductsGrid() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Column(children: [
        _sectionHeader('Featured Products', 'View all',
            () => Navigator.pushNamed(context, AppRoutes.shop)),
        const SizedBox(height: 12),
        GridView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: _filtered.length,
          gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
            crossAxisCount: 2,
            crossAxisSpacing: 12,
            mainAxisSpacing: 12,
            childAspectRatio: 0.73,
          ),
          itemBuilder: (_, i) => ProductCard(
            product: _filtered[i],
            onCartChanged: () => setState(() {}),
          ),
        ),
      ]),
    );
  }

  // ── Brands ────────────────────────────────────────────────────────────────
  Widget _buildBrands() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Column(children: [
        _sectionHeader('Popular Brands', 'See all', null),
        const SizedBox(height: 12),
        SingleChildScrollView(
          scrollDirection: Axis.horizontal,
          child: Row(
            children: allBrands.skip(1).map((b) => Container(
                  margin: const EdgeInsets.only(right: 10),
                  padding: const EdgeInsets.symmetric(
                      horizontal: 24, vertical: 12),
                  decoration: BoxDecoration(
                    color: b == 'Zara'
                        ? AppColors.primary
                        : AppColors.surface,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: Text(b,
                      style: TextStyle(
                        fontSize: 13,
                        fontWeight: FontWeight.w600,
                        color: b == 'Zara'
                            ? Colors.white
                            : AppColors.textMuted,
                      )),
                )).toList(),
          ),
        ),
      ]),
    );
  }

  Widget _sectionHeader(
      String title, String action, VoidCallback? onTap) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(title, style: AppTextStyles.titleMedium),
        GestureDetector(
          onTap: onTap,
          child: Text(action,
              style: AppTextStyles.labelMedium
                  .copyWith(color: AppColors.primary)),
        ),
      ],
    );
  }
}

// ── Banner model ──────────────────────────────────────────────────────────────
class _BannerData {
  final String tag;
  final String title;
  final String subtitle;
  final List<Color> gradient;

  const _BannerData({
    required this.tag,
    required this.title,
    required this.subtitle,
    required this.gradient,
  });
}