import 'package:flutter/material.dart';
import 'package:LushThreads/core/constants/app_routes.dart';
import 'package:LushThreads/core/theme/app_colors.dart';
import 'package:LushThreads/core/theme/app_text_styles.dart';
import 'package:LushThreads/shared/widgets/app_bottom_nav.dart';
 
class ProfileScreen extends StatelessWidget {
  const ProfileScreen({super.key});

  static const _orders = [
    (emoji: '👗', brand: 'ZARA',   item: 'Floral Maxi Dress',   price: r'$89.00',  status: 'In Transit'),
    (emoji: '👟', brand: 'ADIDAS', item: 'Stan Smith Classic',   price: r'$120.00', status: 'Delivered'),
    (emoji: '👜', brand: 'H&M',    item: 'Quilted Shoulder Bag', price: r'$45.90',  status: 'Delivered'),
  ];

  static const _menuItems = [
    (icon: Icons.person_outline_rounded,  title: 'Personal Information', subtitle: 'Name, email, and phone number'),
    (icon: Icons.favorite_border_rounded, title: 'My Wishlist',          subtitle: '24 items saved for later'),
    (icon: Icons.location_on_outlined,    title: 'Shipping Addresses',   subtitle: '3 saved addresses'),
    (icon: Icons.credit_card_rounded,     title: 'Payment Methods',      subtitle: 'Visa •••• 4242'),
    (icon: Icons.history_rounded,         title: 'Order History',        subtitle: 'Manage your past purchases'),
    (icon: Icons.help_outline_rounded,    title: 'Support & FAQ',        subtitle: 'Get help with your orders'),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            Expanded(
              child: SingleChildScrollView(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    _buildHeaderStack(context),
                    const SizedBox(height: 24),
                    _buildRecentOrders(),
                    const SizedBox(height: 24),
                    _buildAccountSettings(),
                    const SizedBox(height: 24),
                    _buildLogoutBtn(context),
                    const SizedBox(height: 32),
                  ],
                ),
              ),
            ),
            AppBottomNav(currentIndex: 4),
          ],
        ),
      ),
    );
  }

  Widget _buildHeaderStack(BuildContext context) {
    return SizedBox(
      height: 350,
      child: Stack(
        clipBehavior: Clip.none,
        children: [
          // Teal gradient bg
          Container(
            height: 220,
            margin: const EdgeInsets.only(bottom: 60),
            decoration: const BoxDecoration(
              gradient: LinearGradient(
                colors: [Color(0xFF088170), Color(0xFF056154)],
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              borderRadius: BorderRadius.only(
                bottomLeft: Radius.circular(40),
                bottomRight: Radius.circular(40),
              ),
            ),
          ),
          // Top bar
          Positioned(
            top: 0, left: 0, right: 0,
            child: Padding(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  IconButton(
                    icon: const Icon(Icons.settings_rounded, color: Colors.white, size: 24),
                    onPressed: () {},
                  ),
                  Text('My Profile',
                      style: AppTextStyles.titleMedium
                          .copyWith(color: Colors.white, fontWeight: FontWeight.w600)),
                  IconButton(
                    icon: const Icon(Icons.notifications_none_rounded, color: Colors.white, size: 24),
                    onPressed: () {},
                  ),
                ],
              ),
            ),
          ),
          // Floating white card
          Positioned(
            left: 10, right: 10, bottom: 0,
            child: Container(
              padding: const EdgeInsets.all(32),
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(32),
                boxShadow: [
                  BoxShadow(
                    color: AppColors.primary.withOpacity(0.30),
                    blurRadius: 32,
                    offset: const Offset(0, 16),
                  ),
                ],
              ),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  // Avatar + gold edit badge
                  Stack(
                    alignment: Alignment.center,
                    children: [
                      Container(
                        width: 100, height: 100,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: AppColors.primaryLight,
                          border: Border.all(color: AppColors.background, width: 4),
                          boxShadow: [
                            BoxShadow(
                              color: AppColors.primary.withOpacity(0.20),
                              blurRadius: 16, offset: const Offset(0, 8),
                            ),
                          ],
                        ),
                        child: const Center(
                          child: Text('A',
                              style: TextStyle(fontSize: 38,
                                  fontWeight: FontWeight.w800,
                                  color: AppColors.primary)),
                        ),
                      ),
                      Positioned(
                        bottom: 0, right: 0,
                        child: Container(
                          width: 32, height: 32,
                          decoration: BoxDecoration(
                            color: const Color(0xFFD4AF37),
                            shape: BoxShape.circle,
                            border: Border.all(color: AppColors.surface, width: 3),
                          ),
                          child: const Icon(Icons.edit_rounded,
                              size: 16, color: AppColors.textMain),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 16),
                  Text('Alexandra Reed',
                      style: AppTextStyles.titleLarge
                          .copyWith(fontWeight: FontWeight.w800, fontSize: 22)),
                  const SizedBox(height: 4),
                  Text('alexandra.r@lushthreads.com',
                      style: AppTextStyles.bodyMedium
                          .copyWith(color: AppColors.textMuted)),
                  const Divider(color: AppColors.divider, thickness: 1, height: 32),
                  // Stats
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceAround,
                    children: [
                      _stat('12', 'Orders'),
                      Container(width: 1, height: 36, color: AppColors.divider),
                      _stat('24', 'Wishlist'),
                      Container(width: 1, height: 36, color: AppColors.divider),
                      _stat('450', 'Points'),
                    ],
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _stat(String value, String label) => Column(children: [
        Text(value,
            style: AppTextStyles.titleLarge
                .copyWith(color: AppColors.primary, fontWeight: FontWeight.w800)),
        const SizedBox(height: 2),
        Text(label, style: AppTextStyles.bodySmall),
      ]);

  Widget _buildRecentOrders() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 24),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text('Recent Orders',
                  style: AppTextStyles.titleLarge.copyWith(fontWeight: FontWeight.w700)),
              Text('View All',
                  style: AppTextStyles.labelLarge
                      .copyWith(color: AppColors.primary, fontWeight: FontWeight.w600)),
            ],
          ),
        ),
        const SizedBox(height: 14),
        SizedBox(
          height: 120,
          child: ListView.separated(
            scrollDirection: Axis.horizontal,
            padding: const EdgeInsets.symmetric(horizontal: 24),
            itemCount: _orders.length,
            separatorBuilder: (_, __) => const SizedBox(width: 12),
            itemBuilder: (_, i) => _orderCard(_orders[i]),
          ),
        ),
      ],
    );
  }

  Widget _orderCard(
      ({String emoji, String brand, String item, String price, String status}) o) {
    return Container(
      width: 240,
      padding: const EdgeInsets.all(12),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        children: [
          Container(
            width: 70, height: 70,
            decoration: BoxDecoration(
              color: AppColors.background,
              borderRadius: BorderRadius.circular(16),
            ),
            child: Center(child: Text(o.emoji, style: const TextStyle(fontSize: 30))),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                Text(o.brand,
                    style: AppTextStyles.labelSmall
                        .copyWith(color: AppColors.primary, fontWeight: FontWeight.w700)),
                const SizedBox(height: 2),
                Text(o.item,
                    maxLines: 1, overflow: TextOverflow.ellipsis,
                    style: AppTextStyles.bodyMedium
                        .copyWith(color: AppColors.textMain, fontWeight: FontWeight.w600)),
                const SizedBox(height: 6),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(o.price,
                        style: AppTextStyles.bodyMedium.copyWith(
                            color: const Color(0xFFD4AF37),
                            fontWeight: FontWeight.w700)),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                      decoration: BoxDecoration(
                        color: const Color(0xFF088170).withOpacity(0.12),
                        borderRadius: BorderRadius.circular(16),
                      ),
                      child: Text(o.status,
                          style: AppTextStyles.labelSmall
                              .copyWith(color: AppColors.primary, fontWeight: FontWeight.w600)),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAccountSettings() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Account Settings',
              style: AppTextStyles.titleLarge.copyWith(fontWeight: FontWeight.w700)),
          const SizedBox(height: 16),
          Container(
            decoration: BoxDecoration(
              color: AppColors.surface,
              borderRadius: BorderRadius.circular(20),
            ),
            child: Column(
              children: List.generate(_menuItems.length, (i) {
                final it = _menuItems[i];
                return Column(children: [
                  ListTile(
                    contentPadding:
                        const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
                    leading: Container(
                      width: 40, height: 40,
                      decoration: BoxDecoration(
                        color: AppColors.primaryLight,
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Icon(it.icon, color: AppColors.primary, size: 20),
                    ),
                    title: Text(it.title,
                        style: AppTextStyles.labelMedium
                            .copyWith(color: AppColors.textMain)),
                    subtitle: Text(it.subtitle,
                        style: AppTextStyles.bodySmall
                            .copyWith(color: AppColors.hint, fontSize: 12)),
                    trailing: const Icon(Icons.arrow_forward_ios_rounded,
                        size: 14, color: AppColors.hint),
                    onTap: () {},
                  ),
                  if (i < _menuItems.length - 1)
                    const Divider(height: 1, indent: 60, color: AppColors.divider),
                ]);
              }),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildLogoutBtn(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 24),
      child: GestureDetector(
        onTap: () => Navigator.pushNamedAndRemoveUntil(
            context, AppRoutes.login, (r) => false),
        child: Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(28),
            border: Border.all(color: AppColors.error),
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              const Icon(Icons.logout_rounded, color: AppColors.error, size: 20),
              const SizedBox(width: 8),
              Text('Log Out',
                  style: AppTextStyles.labelLarge.copyWith(color: AppColors.error)),
            ],
          ),
        ),
      ),
    );
  }
}