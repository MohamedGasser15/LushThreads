import 'package:flutter/material.dart';
import 'package:LushThreads/core/constants/app_routes.dart';
import 'package:LushThreads/core/theme/app_colors.dart';
import 'package:LushThreads/core/theme/app_text_styles.dart';
import 'package:LushThreads/shared/models/cart.dart';
import 'package:LushThreads/shared/widgets/app_bottom_nav.dart';


class CartScreen extends StatefulWidget {
  const CartScreen({super.key});

  @override
  State<CartScreen> createState() => _CartScreenState();
}

class _CartScreenState extends State<CartScreen> {
  final cart = CartManager.instance;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Column(
          children: [
            _buildHeader(),
            Expanded(
              child: cart.items.isEmpty ? _buildEmpty() : _buildList(),
            ),
            if (cart.items.isNotEmpty) _buildSummary(),
            AppBottomNav(currentIndex: 2),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader() {
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 14, 20, 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text('My Cart', style: AppTextStyles.titleLarge),
          if (cart.items.isNotEmpty)
            GestureDetector(
              onTap: () { setState(() => cart.clear()); },
              child: Text('Clear all',
                  style: AppTextStyles.labelMedium.copyWith(color: AppColors.error)),
            ),
        ],
      ),
    );
  }

  Widget _buildEmpty() {
    return Center(
      child: Column(mainAxisAlignment: MainAxisAlignment.center, children: [
        const Text('🛍️', style: TextStyle(fontSize: 64)),
        const SizedBox(height: 16),
        Text('Your cart is empty', style: AppTextStyles.titleMedium),
        const SizedBox(height: 8),
        Text('Add some items to get started!', style: AppTextStyles.bodyMedium),
        const SizedBox(height: 24),
        GestureDetector(
          onTap: () => Navigator.pushNamed(context, AppRoutes.shop),
          child: Container(
            padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 14),
            decoration: BoxDecoration(
              color: AppColors.primary, borderRadius: BorderRadius.circular(14)),
            child: Text('Start Shopping',
                style: AppTextStyles.labelLarge.copyWith(color: Colors.white)),
          ),
        ),
      ]),
    );
  }

  Widget _buildList() {
    return ListView.separated(
      padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
      itemCount: cart.items.length,
      separatorBuilder: (_, __) => const SizedBox(height: 12),
      itemBuilder: (_, i) {
        final item = cart.items[i];
        return Dismissible(
          key: Key('${item.product.id}-${item.selectedSize}'),
          direction: DismissDirection.endToStart,
          onDismissed: (_) => setState(() => cart.removeItem(i)),
          background: Container(
            alignment: Alignment.centerRight,
            padding: const EdgeInsets.only(right: 16),
            decoration: BoxDecoration(
              color: AppColors.error.withOpacity(0.15),
              borderRadius: BorderRadius.circular(16),
            ),
            child: const Icon(Icons.delete_outline_rounded,
                color: AppColors.error, size: 24),
          ),
          child: Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: AppColors.surface, borderRadius: BorderRadius.circular(16)),
            child: Row(
              children: [
                // Product image
                Container(
                  width: 80, height: 80,
                  decoration: BoxDecoration(
                    color: item.product.bgColor,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Center(
                    child: Text(item.product.emoji,
                        style: const TextStyle(fontSize: 36)),
                  ),
                ),
                const SizedBox(width: 12),
                // Info
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(item.product.brand.toUpperCase(),
                          style: AppTextStyles.labelSmall.copyWith(
                              color: AppColors.primary)),
                      const SizedBox(height: 2),
                      Text(item.product.name, maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                          style: AppTextStyles.labelMedium),
                      const SizedBox(height: 4),
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 8, vertical: 2),
                        decoration: BoxDecoration(
                          color: AppColors.primaryLight,
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: Text('Size: ${item.selectedSize}',
                            style: AppTextStyles.labelSmall.copyWith(
                                color: AppColors.primary)),
                      ),
                    ],
                  ),
                ),
                // Price + qty
                Column(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Text('\$${item.totalPrice.toStringAsFixed(2)}',
                        style: AppTextStyles.priceSmall),
                    const SizedBox(height: 8),
                    Row(children: [
                      _qtyBtn(Icons.remove, () {
                        setState(() => cart.decrementQty(i));
                      }),
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 10),
                        child: Text('${item.quantity}',
                            style: AppTextStyles.labelLarge),
                      ),
                      _qtyBtn(Icons.add, () {
                        setState(() => cart.incrementQty(i));
                      }),
                    ]),
                  ],
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _qtyBtn(IconData icon, VoidCallback onTap) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 28, height: 28,
        decoration: BoxDecoration(
          color: Colors.white, borderRadius: BorderRadius.circular(8),
          border: Border.all(color: AppColors.divider),
        ),
        child: Icon(icon, size: 14, color: AppColors.textMain),
      ),
    );
  }

  Widget _buildSummary() {
    return Container(
      padding: const EdgeInsets.fromLTRB(20, 16, 20, 8),
      decoration: const BoxDecoration(
        color: Colors.white,
        border: Border(top: BorderSide(color: AppColors.divider)),
      ),
      child: Column(
        children: [
          _summaryRow('Subtotal', '\$${cart.subtotal.toStringAsFixed(2)}'),
          const SizedBox(height: 6),
          _summaryRow('Shipping',
              cart.shipping == 0 ? 'Free' : '\$${cart.shipping.toStringAsFixed(2)}'),
          if (cart.shipping == 0)
            Padding(
              padding: const EdgeInsets.only(top: 4),
              child: Row(children: [
                const Icon(Icons.check_circle_outline_rounded,
                    size: 14, color: AppColors.success),
                const SizedBox(width: 4),
                Text('Free shipping on orders over \$100',
                    style: AppTextStyles.labelSmall.copyWith(
                        color: AppColors.success)),
              ]),
            ),
          const SizedBox(height: 6),
          const Divider(color: AppColors.divider),
          _summaryRow('Total', '\$${cart.total.toStringAsFixed(2)}',
              isTotal: true),
          const SizedBox(height: 12),
          SizedBox(
            width: double.infinity,
            child: GestureDetector(
              onTap: () => Navigator.pushNamed(context, AppRoutes.checkout),
              child: Container(
                height: 50,
                decoration: BoxDecoration(
                  color: AppColors.primary,
                  borderRadius: BorderRadius.circular(14),
                ),
                child: Center(
                  child: Text('Proceed to Checkout',
                      style: AppTextStyles.labelLarge.copyWith(color: Colors.white)),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _summaryRow(String label, String value, {bool isTotal = false}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(label, style: isTotal
            ? AppTextStyles.labelLarge
            : AppTextStyles.bodyMedium),
        Text(value, style: isTotal
            ? AppTextStyles.priceSmall
            : AppTextStyles.labelMedium),
      ],
    );
  }
}
